using System.Text.RegularExpressions;
using Library.Domain.Common;
using Library.Domain.Dtos;
using Library.Domain.Interfaces;
using Library.Infrastructure.Persistence;
using Library.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Library.Application;

internal sealed partial class BookService(LibraryDbContext db) : IBookService
{
    private static readonly Regex BookNumberRegex = MyRegex();

    public async Task<IReadOnlyList<(int SubjectId, string Name)>> GetSubjectsAsync(CancellationToken cancellationToken = default)
    {
        return await db.Subjects.AsNoTracking()
            .OrderBy(subject => subject.Name)
            .Select(subject => new ValueTuple<int, string>(subject.SubjectId, subject.Name))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<BookListItemDto>> SearchAsync(BookSearchQuery query, CancellationToken cancellationToken = default)
    {
        var data = db.Books.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.TitleContains))
        {
            var title = query.TitleContains.Trim();
            data = data.Where(book => book.Title.Contains(title));
        } 
        else if (!string.IsNullOrWhiteSpace(query.AuthorContains))
        {
            var author = query.AuthorContains.Trim();
            data = data.Where(book => book.AuthorOrEditor.Contains(author));
        }

        if (query.SubjectId is not null)
            data = data.Where(book => book.SubjectId == query.SubjectId);
        
        return await data
            .OrderBy(b => b.Title)
            .Select(b => new BookListItemDto(
                b.BookId,
                b.BookNumber,
                b.Title,
                b.AuthorOrEditor,
                b.Subject.Name,
                b.Isbn,
                db.Loans.Any(l => l.BookId == b.BookId)
            ))
            .ToListAsync(cancellationToken);
    }

    public async Task<Result<BookDetailsDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var book = await db.Books.AsNoTracking().Include(x => x.Subject).FirstOrDefaultAsync(x => x.BookId == id, cancellationToken);
        return book is null
            ? Result<BookDetailsDto>.Fail("Buch nicht gefunden.")
            : Result<BookDetailsDto>.Ok(MapDetails(book));
    }

    public async Task<Result<BookDetailsDto>> GetByBookNumberAsync(string bookNumber, CancellationToken cancellationToken = default)
    {
        var key = bookNumber.Trim();
        var book = await db.Books.AsNoTracking().Include(x => x.Subject).FirstOrDefaultAsync(x => x.BookNumber == key, cancellationToken);
        return book is null
            ? Result<BookDetailsDto>.Fail("Buch nicht gefunden.")
            : Result<BookDetailsDto>.Ok(MapDetails(book));
    }

    public async Task<Result<int>> CreateAsync(BookUpsertDto dto, CancellationToken cancellationToken = default)
    {
        var bookNumber = dto.BookNumber.Trim();

        if (!BookNumberRegex.IsMatch(bookNumber))
            return Result<int>.Fail("Buchnummer-Format ungültig (erwartet: xxxxx-jjjj).");

        var exists = await db.Books.AnyAsync(book => book.BookNumber == bookNumber, cancellationToken);
        if (exists) return Result<int>.Fail("Diese Buchnummer existiert bereits.");

        var entity = new Book
        {
            BookNumber = bookNumber,
            Title = dto.Title.Trim(),
            AuthorOrEditor = dto.AuthorOrEditor.Trim(),
            SubjectId = dto.SubjectId,
            Isbn = string.IsNullOrWhiteSpace(dto.Isbn) ? null : dto.Isbn.Trim(),
            Publisher = string.IsNullOrWhiteSpace(dto.Publisher) ? null : dto.Publisher.Trim(),
            PublisherCity = string.IsNullOrWhiteSpace(dto.PublisherCity) ? null : dto.PublisherCity.Trim(),
            PublishedOn = dto.PublishedOn,
            CreatedAtUtc = DateTime.UtcNow
        };

        db.Books.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        return Result<int>.Ok(entity.BookId);
    }

    public async Task<Result> UpdateAsync(int id, BookUpsertDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await db.Books.FirstOrDefaultAsync(book => book.BookId == id, cancellationToken);
        if (entity is null) return Result.Fail("Buch nicht gefunden.");

        var bookNumber = dto.BookNumber.Trim();
        if (!BookNumberRegex.IsMatch(bookNumber))
            return Result.Fail("Buchnummer-Format ungültig (erwartet: xxxxx-jjjj).");

        var duplicate = await db.Books.AnyAsync(book => book.BookId != id && book.BookNumber == bookNumber, cancellationToken);
        if (duplicate) return Result.Fail("Diese Buchnummer existiert bereits.");

        entity.BookNumber = bookNumber;
        entity.Title = dto.Title.Trim();
        entity.AuthorOrEditor = dto.AuthorOrEditor.Trim();
        entity.SubjectId = dto.SubjectId;
        entity.Isbn = string.IsNullOrWhiteSpace(dto.Isbn) ? null : dto.Isbn.Trim();
        entity.Publisher = string.IsNullOrWhiteSpace(dto.Publisher) ? null : dto.Publisher.Trim();
        entity.PublisherCity = string.IsNullOrWhiteSpace(dto.PublisherCity) ? null : dto.PublisherCity.Trim();
        entity.PublishedOn = dto.PublishedOn;

        await db.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await db.Books.FirstOrDefaultAsync(book => book.BookId == id, cancellationToken);
        if (entity is null) return Result.Fail("Buch nicht gefunden.");

        var isLoaned = await db.Loans.AnyAsync(loan => loan.BookId == id, cancellationToken);
        if (isLoaned) return Result.Fail("Buch ist aktuell ausgeliehen und kann nicht gelöscht werden.");

        db.Books.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }

    private static BookDetailsDto MapDetails(Book book)
    {
        return new BookDetailsDto(
            book.BookId,
            book.BookNumber,
            book.Title,
            book.AuthorOrEditor,
            book.Subject.Name,
            book.SubjectId,
            book.Isbn,
            book.Publisher,
            book.PublisherCity,
            book.PublishedOn
        );
    }

    [GeneratedRegex(@"^\d{5}-\d{4}$", RegexOptions.Compiled)]
    private static partial Regex MyRegex();
}
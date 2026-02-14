using System.Text.RegularExpressions;
using Library.DataAccess.Persistence;
using Library.DataAccess.Persistence.Entities;
using Library.Models.Common;
using Library.Models.Dtos;
using Library.Models.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Library.Services;

internal sealed partial class BookService(LibraryDbContext db) : IBookService
{
    private static readonly Regex BookNumberRegex = MyRegex();

    public async Task<IReadOnlyList<(int SubjectId, string Name)>> GetSubjectsAsync(
        CancellationToken cancellationToken = default)
    {
        return await db.Subjects.AsNoTracking()
            .OrderBy(subject => subject.Name)
            .Select(subject => new ValueTuple<int, string>(subject.SubjectId, subject.Name))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<BookListItemDto>> SearchAsync(
        BookSearchQuery query,
        CancellationToken cancellationToken = default)
    {
        var results = new List<BookListItemDto>();

        var baseQuery = db.Books.AsNoTracking();

        if (query.SubjectId is not null)
            baseQuery = baseQuery.Where(b => b.SubjectId == query.SubjectId);

        if (!string.IsNullOrWhiteSpace(query.SearchContains))
        {
            var title = query.SearchContains.Trim();

            var titleHits = await baseQuery
                .Where(b => b.Title.Contains(title))
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

            results.AddRange(titleHits);
        }

        if (!string.IsNullOrWhiteSpace(query.AuthorContains))
        {
            var author = query.AuthorContains.Trim();

            var authorHits = await baseQuery
                .Where(b => b.AuthorOrEditor.Contains(author))
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

            results.AddRange(authorHits);
        }

        if (!string.IsNullOrWhiteSpace(query.SearchContains) ||
            !string.IsNullOrWhiteSpace(query.AuthorContains))
            return results
                .GroupBy(x => x.BookId)
                .Select(g => g.First())
                .OrderBy(x => x.Title)
                .ToList();


        var allHits = await baseQuery
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

        results.AddRange(allHits);


        return results
            .GroupBy(x => x.BookId)
            .Select(g => g.First())
            .OrderBy(x => x.Title)
            .ToList();
    }


    public async Task<Results<BookDetailsDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var book = await db.Books.AsNoTracking().Include(x => x.Subject)
            .FirstOrDefaultAsync(x => x.BookId == id, cancellationToken);
        return book is null
            ? Results<BookDetailsDto>.Fail("Buch nicht gefunden.")
            : Results<BookDetailsDto>.Ok(MapDetails(book));
    }

    public async Task<Results<BookDetailsDto>> GetByBookNumberAsync(string bookNumber,
        CancellationToken cancellationToken = default)
    {
        var key = bookNumber.Trim();
        var book = await db.Books.AsNoTracking().Include(x => x.Subject)
            .FirstOrDefaultAsync(x => x.BookNumber == key, cancellationToken);
        return book is null
            ? Results<BookDetailsDto>.Fail("Buch nicht gefunden.")
            : Results<BookDetailsDto>.Ok(MapDetails(book));
    }

    public async Task<Results<int>> CreateAsync(BookUpsertDto dto, CancellationToken cancellationToken = default)
    {
        var bookNumber = dto.BookNumber.Trim();

        if (!BookNumberRegex.IsMatch(bookNumber))
            return Results<int>.Fail("Buchnummer-Format ungültig (erwartet: xxxxx-jjjj).");

        var exists = await db.Books.AnyAsync(book => book.BookNumber == bookNumber, cancellationToken);
        if (exists) return Results<int>.Fail("Diese Buchnummer existiert bereits.");

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
        return Results<int>.Ok(entity.BookId);
    }

    public async Task<Results> UpdateAsync(int id, BookUpsertDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await db.Books.FirstOrDefaultAsync(book => book.BookId == id, cancellationToken);
        if (entity is null) return Results.Fail("Buch nicht gefunden.");

        var bookNumber = dto.BookNumber.Trim();
        if (!BookNumberRegex.IsMatch(bookNumber))
            return Results.Fail("Buchnummer-Format ungültig (erwartet: xxxxx-jjjj).");

        var duplicate = await db.Books.AnyAsync(book => book.BookId != id && book.BookNumber == bookNumber,
            cancellationToken);
        if (duplicate) return Results.Fail("Diese Buchnummer existiert bereits.");

        entity.BookNumber = bookNumber;
        entity.Title = dto.Title.Trim();
        entity.AuthorOrEditor = dto.AuthorOrEditor.Trim();
        entity.SubjectId = dto.SubjectId;
        entity.Isbn = string.IsNullOrWhiteSpace(dto.Isbn) ? null : dto.Isbn.Trim();
        entity.Publisher = string.IsNullOrWhiteSpace(dto.Publisher) ? null : dto.Publisher.Trim();
        entity.PublisherCity = string.IsNullOrWhiteSpace(dto.PublisherCity) ? null : dto.PublisherCity.Trim();
        entity.PublishedOn = dto.PublishedOn;

        await db.SaveChangesAsync(cancellationToken);
        return Results.Ok();
    }

    public async Task<Results> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await db.Books.FirstOrDefaultAsync(book => book.BookId == id, cancellationToken);
        if (entity is null) return Results.Fail("Buch nicht gefunden.");

        var isLoaned = await db.Loans.AnyAsync(loan => loan.BookId == id, cancellationToken);
        if (isLoaned) return Results.Fail("Buch ist aktuell ausgeliehen und kann nicht gelöscht werden.");

        db.Books.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
        return Results.Ok();
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
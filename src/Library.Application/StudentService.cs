using Library.Domain.Common;
using Library.Domain.Dtos;
using Library.Domain.Interfaces;
using Library.Infrastructure.Persistence;
using Library.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Library.Application;

internal sealed class StudentService(LibraryDbContext db) : IStudentService
{
    public async Task<IReadOnlyList<StudentListItemDto>> SearchAsync(StudentSearchQuery query, CancellationToken cancellationToken = default)
    {
        var q = db.Students.AsNoTracking();

        if (string.IsNullOrWhiteSpace(query.NameContains))
            return await q
                .OrderBy(student => student.LastName).ThenBy(student => student.FirstName)
                .Select(student => new StudentListItemDto(
                    student.StudentId,
                    student.CardNumber,
                    student.FirstName,
                    student.LastName,
                    student.IsActive,
                    student.Loans.Count
                ))
                .ToListAsync(cancellationToken);
        {
            var term = query.NameContains.Trim();
            q = q.Where(student => (student.FirstName + " " + student.LastName).Contains(term) || student.LastName.Contains(term) || student.FirstName.Contains(term));
        }

        return await q
            .OrderBy(student => student.LastName).ThenBy(student => student.FirstName)
            .Select(student => new StudentListItemDto(
                student.StudentId,
                student.CardNumber,
                student.FirstName,
                student.LastName,
                student.IsActive,
                student.Loans.Count
            ))
            .ToListAsync(cancellationToken);
    }

    public async Task<Result<StudentDetailsDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var student = await db.Students.AsNoTracking().FirstOrDefaultAsync(x => x.StudentId == id, cancellationToken);
        return student is null
            ? Result<StudentDetailsDto>.Fail("Schüler nicht gefunden.")
            : Result<StudentDetailsDto>.Ok(new StudentDetailsDto(student.StudentId, student.CardNumber, student.FirstName, student.LastName, student.IsActive));
    }

    public async Task<Result<StudentDetailsDto>> GetByCardNumberAsync(string cardNumber, CancellationToken cancellationToken = default)
    {
        var key = cardNumber.Trim();
        var student = await db.Students.AsNoTracking().FirstOrDefaultAsync(x => x.CardNumber == key, cancellationToken);
        return student is null
            ? Result<StudentDetailsDto>.Fail("Schüler nicht gefunden.")
            : Result<StudentDetailsDto>.Ok(new StudentDetailsDto(student.StudentId, student.CardNumber, student.FirstName, student.LastName, student.IsActive));
    }

    public async Task<Result<int>> CreateAsync(StudentUpsertDto dto, CancellationToken cancellationToken = default)
    {
        var card = dto.CardNumber.Trim();
        var exists = await db.Students.AnyAsync(student => student.CardNumber == card, cancellationToken);
        if (exists) return Result<int>.Fail("Diese Ausweisnummer existiert bereits.");

        var entity = new Student
        {
            CardNumber = card,
            FirstName = dto.FirstName.Trim(),
            LastName = dto.LastName.Trim(),
            IsActive = dto.IsActive
        };

        db.Students.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        return Result<int>.Ok(entity.StudentId);
    }

    public async Task<Result> UpdateAsync(int id, StudentUpsertDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await db.Students.FirstOrDefaultAsync(student => student.StudentId == id, cancellationToken);
        if (entity is null) return Result.Fail("Schüler nicht gefunden.");

        var card = dto.CardNumber.Trim();
        var duplicate = await db.Students.AnyAsync(student => student.StudentId != id && student.CardNumber == card, cancellationToken);
        if (duplicate) return Result.Fail("Diese Ausweisnummer existiert bereits.");

        entity.CardNumber = card;
        entity.FirstName = dto.FirstName.Trim();
        entity.LastName = dto.LastName.Trim();
        entity.IsActive = dto.IsActive;

        await db.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }

    public async Task<Result> DeactivateAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await db.Students.FirstOrDefaultAsync(student => student.StudentId == id, cancellationToken);
        if (entity is null) return Result.Fail("Schüler nicht gefunden.");

        entity.IsActive = false;
        await db.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}

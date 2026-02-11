using Library.DataAccess.Persistence;
using Library.DataAccess.Persistence.Entities;
using Library.Models.Common;
using Library.Models.Dtos;
using Library.Models.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Library.Services;

internal sealed class LoanService(LibraryDbContext db) : ILoanService
{
    public async Task<IReadOnlyList<LoanListItemDto>> GetActiveLoansAsync(CancellationToken cancellationToken = default)
    {
        return await db.Loans.AsNoTracking()
            .Include(loan => loan.Book)
            .Include(loan => loan.Student)
            .OrderByDescending(loan => loan.LoanedAtUtc)
            .Select(loan => new LoanListItemDto(
                loan.LoanId,
                loan.Book.BookNumber,
                loan.Book.Title,
                loan.Student.CardNumber,
                loan.Student.FirstName + " " + loan.Student.LastName,
                loan.LoanedAtUtc
            ))
            .ToListAsync(cancellationToken);
    }

    public async Task<Results<CheckoutVerificationDto>> VerifyCheckoutAsync(CheckoutRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var studentCard = request.StudentCardNumber.Trim();
        var bookNumber = request.BookNumber.Trim();

        var student = await db.Students.AsNoTracking()
            .FirstOrDefaultAsync(student => student.CardNumber == studentCard, cancellationToken);

        if (student is null)
            return Results<CheckoutVerificationDto>.Ok(new CheckoutVerificationDto(
                studentCard,
                "(nicht gefunden)",
                bookNumber,
                "(unbekannt)",
                false,
                "Schüler mit dieser Ausweisnummer wurde nicht gefunden. Bitte zuerst erfassen."
            ));

        if (!student.IsActive)
            return Results<CheckoutVerificationDto>.Ok(new CheckoutVerificationDto(
                studentCard,
                student.FirstName + " " + student.LastName,
                bookNumber,
                "(unbekannt)",
                false,
                "Schüler ist deaktiviert."
            ));

        var book = await db.Books.AsNoTracking()
            .FirstOrDefaultAsync(book => book.BookNumber == bookNumber, cancellationToken);

        if (book is null)
            return Results<CheckoutVerificationDto>.Ok(new CheckoutVerificationDto(
                studentCard,
                student.FirstName + " " + student.LastName,
                bookNumber,
                "(nicht gefunden)",
                false,
                "Buch existiert noch nicht. Bitte zuerst erfassen (On-Demand)."
            ));

        var isLoaned = await db.Loans.AsNoTracking().AnyAsync(loan => loan.BookId == book.BookId, cancellationToken);
        if (isLoaned)
            return Results<CheckoutVerificationDto>.Ok(new CheckoutVerificationDto(
                studentCard,
                student.FirstName + " " + student.LastName,
                bookNumber,
                book.Title,
                false,
                "Dieses Buch ist aktuell ausgeliehen."
            ));

        return Results<CheckoutVerificationDto>.Ok(new CheckoutVerificationDto(
            studentCard,
            student.FirstName + " " + student.LastName,
            bookNumber,
            book.Title,
            true,
            null
        ));
    }

    public async Task<Results> ConfirmCheckoutAsync(CheckoutRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var verification = await VerifyCheckoutAsync(request, cancellationToken);
        if (!verification.IsSuccess || verification.Value is null)
            return Results.Fail(verification.Error ?? "Unbekannter Fehler.");

        if (!verification.Value.CanCheckout)
            return Results.Fail(verification.Value.BlockingReason ?? "Ausleihe nicht möglich.");

        var studentCard = request.StudentCardNumber.Trim();
        var bookNumber = request.BookNumber.Trim();

        var studentId = await db.Students.Where(student => student.CardNumber == studentCard).Select(s => s.StudentId)
            .FirstAsync(cancellationToken);
        var bookId = await db.Books.Where(book => book.BookNumber == bookNumber).Select(b => b.BookId)
            .FirstAsync(cancellationToken);

        var isLoaned = await db.Loans.AnyAsync(loan => loan.BookId == bookId, cancellationToken);
        if (isLoaned) return Results.Fail("Dieses Buch ist aktuell ausgeliehen.");

        db.Loans.Add(new Loan
        {
            StudentId = studentId,
            BookId = bookId,
            LoanedAtUtc = DateTime.UtcNow
        });

        try
        {
            await db.SaveChangesAsync(cancellationToken);
            return Results.Ok();
        }
        catch (DbUpdateException)
        {
            return Results.Fail("Ausleihe konnte nicht gespeichert werden (Constraint-Verletzung).");
        }
    }

    public async Task<Results<ReturnVerificationDto>> VerifyReturnAsync(string bookNumber,
        CancellationToken cancellationToken = default)
    {
        var bookNumberTrimmed = bookNumber.Trim();

        var loan = await db.Loans.AsNoTracking()
            .Include(loan => loan.Book)
            .Include(loan => loan.Student)
            .FirstOrDefaultAsync(loan => loan.Book.BookNumber == bookNumberTrimmed, cancellationToken);

        if (loan is null)
            return Results<ReturnVerificationDto>.Ok(new ReturnVerificationDto(
                bookNumberTrimmed,
                "(nicht gefunden)",
                "(unbekannt)",
                "(unbekannt)",
                default,
                false,
                "Keine aktive Ausleihe zu dieser Buchnummer gefunden."
            ));

        return Results<ReturnVerificationDto>.Ok(new ReturnVerificationDto(
            loan.Book.BookNumber,
            loan.Book.Title,
            loan.Student.CardNumber,
            loan.Student.FirstName + " " + loan.Student.LastName,
            loan.LoanedAtUtc,
            true,
            null
        ));
    }

    public async Task<Results> ConfirmReturnAsync(string bookNumber, CancellationToken cancellationToken = default)
    {
        var bookNumberTrimmed = bookNumber.Trim();

        var loan = await db.Loans
            .Include(l => l.Book)
            .FirstOrDefaultAsync(l => l.Book.BookNumber == bookNumberTrimmed, cancellationToken);

        if (loan is null) return Results.Fail("Keine aktive Ausleihe gefunden.");

        var (year, month, _) = DateTime.UtcNow;
        var bookId = loan.BookId;

        var stat = await db.MonthlyBookStats
            .FirstOrDefaultAsync(s => s.Year == year && s.Month == month && s.BookId == bookId, cancellationToken);

        if (stat is null)
        {
            stat = new MonthlyBookStat
            {
                Year = year,
                Month = month,
                BookId = bookId,
                LoanCount = 0
            };
            db.MonthlyBookStats.Add(stat);
        }

        stat.LoanCount += 1;

        db.Loans.Remove(loan);
        await db.SaveChangesAsync(cancellationToken);

        return Results.Ok();
    }
}
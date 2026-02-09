using Library.Domain.Common;
using Library.Domain.Dtos;
using Library.Domain.Interfaces;
using Library.Infrastructure.Persistence;
using Library.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Library.Application;

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

    public async Task<Result<CheckoutVerificationDto>> VerifyCheckoutAsync(CheckoutRequestDto request, CancellationToken cancellationToken = default)
    {
        var studentCard = request.StudentCardNumber.Trim();
        var bookNumber = request.BookNumber.Trim();

        var student = await db.Students.AsNoTracking()
            .FirstOrDefaultAsync(student => student.CardNumber == studentCard, cancellationToken);

        if (student is null)
        {
            return Result<CheckoutVerificationDto>.Ok(new CheckoutVerificationDto(
                studentCard,
                "(nicht gefunden)",
                bookNumber,
                "(unbekannt)",
                CanCheckout: false,
                BlockingReason: "Schüler mit dieser Ausweisnummer wurde nicht gefunden. Bitte zuerst erfassen."
            ));
        }

        if (!student.IsActive)
        {
            return Result<CheckoutVerificationDto>.Ok(new CheckoutVerificationDto(
                studentCard,
                student.FirstName + " " + student.LastName,
                bookNumber,
                "(unbekannt)",
                CanCheckout: false,
                BlockingReason: "Schüler ist deaktiviert."
            ));
        }

        var book = await db.Books.AsNoTracking()
            .FirstOrDefaultAsync(book => book.BookNumber == bookNumber, cancellationToken);

        if (book is null)
        {
            return Result<CheckoutVerificationDto>.Ok(new CheckoutVerificationDto(
                studentCard,
                student.FirstName + " " + student.LastName,
                bookNumber,
                "(nicht gefunden)",
                CanCheckout: false,
                BlockingReason: "Buch existiert noch nicht. Bitte zuerst erfassen (On-Demand)."
            ));
        }

        var isLoaned = await db.Loans.AsNoTracking().AnyAsync(loan => loan.BookId == book.BookId, cancellationToken);
        if (isLoaned)
        {
            return Result<CheckoutVerificationDto>.Ok(new CheckoutVerificationDto(
                studentCard,
                student.FirstName + " " + student.LastName,
                bookNumber,
                book.Title,
                CanCheckout: false,
                BlockingReason: "Dieses Buch ist aktuell ausgeliehen."
            ));
        }

        return Result<CheckoutVerificationDto>.Ok(new CheckoutVerificationDto(
            studentCard,
            student.FirstName + " " + student.LastName,
            bookNumber,
            book.Title,
            CanCheckout: true,
            BlockingReason: null
        ));
    }

    public async Task<Result> ConfirmCheckoutAsync(CheckoutRequestDto request, CancellationToken cancellationToken = default)
    {
        var verification = await VerifyCheckoutAsync(request, cancellationToken);
        if (!verification.IsSuccess || verification.Value is null)
            return Result.Fail(verification.Error ?? "Unbekannter Fehler.");

        if (!verification.Value.CanCheckout)
            return Result.Fail(verification.Value.BlockingReason ?? "Ausleihe nicht möglich.");

        var studentCard = request.StudentCardNumber.Trim();
        var bookNumber = request.BookNumber.Trim();

        var studentId = await db.Students.Where(student => student.CardNumber == studentCard).Select(s => s.StudentId).FirstAsync(cancellationToken);
        var bookId = await db.Books.Where(book => book.BookNumber == bookNumber).Select(b => b.BookId).FirstAsync(cancellationToken);

        var isLoaned = await db.Loans.AnyAsync(loan => loan.BookId == bookId, cancellationToken);
        if (isLoaned) return Result.Fail("Dieses Buch ist aktuell ausgeliehen.");

        db.Loans.Add(new Loan
        {
            StudentId = studentId,
            BookId = bookId,
            LoanedAtUtc = DateTime.UtcNow
        });

        try
        {
            await db.SaveChangesAsync(cancellationToken);
            return Result.Ok();
        }
        catch (DbUpdateException)
        {
            return Result.Fail("Ausleihe konnte nicht gespeichert werden (Constraint-Verletzung).");
        }
    }

    public async Task<Result<ReturnVerificationDto>> VerifyReturnAsync(string bookNumber, CancellationToken cancellationToken = default)
    {
        var bookNumberTrimmed = bookNumber.Trim();

        var loan = await db.Loans.AsNoTracking()
            .Include(loan => loan.Book)
            .Include(loan => loan.Student)
            .FirstOrDefaultAsync(loan => loan.Book.BookNumber == bookNumberTrimmed, cancellationToken);

        if (loan is null)
        {
            return Result<ReturnVerificationDto>.Ok(new ReturnVerificationDto(
                bookNumberTrimmed,
                "(nicht gefunden)",
                "(unbekannt)",
                "(unbekannt)",
                default,
                CanReturn: false,
                BlockingReason: "Keine aktive Ausleihe zu dieser Buchnummer gefunden."
            ));
        }

        return Result<ReturnVerificationDto>.Ok(new ReturnVerificationDto(
            loan.Book.BookNumber,
            loan.Book.Title,
            loan.Student.CardNumber,
            loan.Student.FirstName + " " + loan.Student.LastName,
            loan.LoanedAtUtc,
            CanReturn: true,
            BlockingReason: null
        ));
    }

    public async Task<Result> ConfirmReturnAsync(string bookNumber, CancellationToken cancellationToken = default)
    {
        var bookNumberTrimmed = bookNumber.Trim();

        var loan = await db.Loans
            .Include(l => l.Book)
            .FirstOrDefaultAsync(l => l.Book.BookNumber == bookNumberTrimmed, cancellationToken);

        if (loan is null) return Result.Fail("Keine aktive Ausleihe gefunden.");

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

        return Result.Ok();
    }
}

using Library.Domain.Dtos;
using Library.Domain.Interfaces;
using Library.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Library.Application;

internal sealed class ReportService(LibraryDbContext db) : IReportService
{
    public async Task<IReadOnlyList<OverdueItemDto>> GetOverdueLoansAsync(int monthsThreshold = 3, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var cutoff = now.AddMonths(-monthsThreshold);

        var overdue = await db.Loans.AsNoTracking()
            .Include(loan => loan.Book)
            .Include(loan => loan.Student)
            .Where(loan => loan.LoanedAtUtc < cutoff)
            .OrderBy(loan => loan.Student.LastName).ThenBy(loan => loan.Student.FirstName).ThenBy(loan => loan.LoanedAtUtc)
            .Select(loan => new
            {
                loan.Student.CardNumber,
                FullName = loan.Student.FirstName + " " + loan.Student.LastName,
                loan.Book.BookNumber,
                loan.Book.Title,
                loan.LoanedAtUtc
            })
            .ToListAsync(cancellationToken);

        return overdue
            .Select(x =>
            {
                var due = x.LoanedAtUtc.AddMonths(monthsThreshold);
                var daysOverdue = (now - due).Days;
                return new OverdueItemDto(x.CardNumber, x.FullName, x.BookNumber, x.Title, x.LoanedAtUtc, daysOverdue);
            })
            .ToList();
    }

    public async Task<IReadOnlyList<StudentWithLoansDto>> GetStudentsWithActiveLoansAsync(CancellationToken cancellationToken = default)
{
    var raw = await db.Loans.AsNoTracking()
        .Include(loan => loan.Student)
        .Include(loan => loan.Book)
        .OrderBy(loan => loan.Student.LastName).ThenBy(loan => loan.Student.FirstName).ThenByDescending(loan => loan.LoanedAtUtc)
        .Select(loan => new
        {
            loan.Student.CardNumber,
            FullName = loan.Student.FirstName + " " + loan.Student.LastName,
            loan.Book.BookNumber,
            loan.Book.Title,
            loan.LoanedAtUtc
        })
        .ToListAsync(cancellationToken);

    return raw
        .GroupBy(x => new { x.CardNumber, x.FullName })
        .OrderBy(g => g.Key.FullName)
        .Select(g => new StudentWithLoansDto(
            g.Key.CardNumber,
            g.Key.FullName,
            g.Count(),
            g.Select(b => new StudentLoanBookDto(b.BookNumber, b.Title, b.LoanedAtUtc)).ToList()
        ))
        .ToList();
}

public async Task<IReadOnlyList<MonthlyBookStatDto>> GetMonthlyBookStatsAsync(int? year = null, CancellationToken cancellationToken = default)
    {
        var query = db.MonthlyBookStats.AsNoTracking()
            .Include(s => s.Book)
            .AsQueryable();

        if (year is not null)
            query = query.Where(s => s.Year == year);

        return await query
            .OrderByDescending(s => s.Year).ThenByDescending(s => s.Month).ThenBy(s => s.Book.Title)
            .Select(s => new MonthlyBookStatDto(s.Year, s.Month, s.Book.BookNumber, s.Book.Title, s.LoanCount))
            .ToListAsync(cancellationToken);
    }
}

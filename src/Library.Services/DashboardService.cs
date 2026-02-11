using Library.DataAccess.Persistence;
using Library.Models.Dtos;
using Library.Models.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Library.Services;

internal sealed class DashboardService(LibraryDbContext db) : IDashboardService
{
    public async Task<DashboardStatsDto> GetStatsAsync(int overdueMonthsThreshold = 3,
        CancellationToken cancellationToken = default)
    {
        var totalBooks = await db.Books.AsNoTracking().CountAsync(cancellationToken);
        var loanedBooks = await db.Loans.AsNoTracking().CountAsync(cancellationToken);
        var totalStudents = await db.Students.AsNoTracking().CountAsync(cancellationToken);

        var cutoff = DateTime.UtcNow.AddMonths(-overdueMonthsThreshold);
        var overdueLoans = await db.Loans.AsNoTracking().CountAsync(l => l.LoanedAtUtc < cutoff, cancellationToken);

        return new DashboardStatsDto(
            totalBooks,
            loanedBooks,
            totalStudents,
            overdueLoans
        );
    }
}
using Library.Models.Dtos;

namespace Library.Models.Interfaces;

public interface IReportService
{
    Task<IReadOnlyList<OverdueItemDto>> GetOverdueLoansAsync(int monthsThreshold = 3,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<StudentWithLoansDto>> GetStudentsWithActiveLoansAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MonthlyBookStatDto>> GetMonthlyBookStatsAsync(int? year = null,
        CancellationToken cancellationToken = default);
}
using Library.Models.Dtos;

namespace Library.Models.Interfaces;

public interface IDashboardService
{
    Task<DashboardStatsDto> GetStatsAsync(int overdueMonthsThreshold = 3,
        CancellationToken cancellationToken = default);
}
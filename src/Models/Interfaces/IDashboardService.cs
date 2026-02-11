using Library.Domain.Dtos;

namespace Library.Domain.Interfaces;

public interface IDashboardService
{
    Task<DashboardStatsDto> GetStatsAsync(int overdueMonthsThreshold = 3, CancellationToken cancellationToken = default);
}

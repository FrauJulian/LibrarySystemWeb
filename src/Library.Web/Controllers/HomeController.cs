using System.Diagnostics;
using Library.Domain.Interfaces;
using Library.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace Library.Web.Controllers;

public sealed class HomeController(IDashboardService dashboard) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
        => View(await dashboard.GetStatsAsync(overdueMonthsThreshold: 3, cancellationToken));

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
        => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}

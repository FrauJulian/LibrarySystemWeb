using System.Diagnostics;
using Library.Models.Interfaces;
using Library.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace Library.Web.Controllers;

public sealed class HomeController(IDashboardService dashboard) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        return View(await dashboard.GetStatsAsync(3, cancellationToken));
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
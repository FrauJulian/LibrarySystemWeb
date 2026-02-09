using Library.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Library.Web.Controllers;

public sealed class ReportsController(IReportService reports) : Controller
{
    public async Task<IActionResult> Overdue(int months = 3, string? query = null, CancellationToken cancellationToken = default)
    {
        ViewBag.Months = months;
        ViewBag.Q = query ?? "";
        var list = await reports.GetOverdueLoansAsync(months, cancellationToken);
        if (string.IsNullOrWhiteSpace(query)) return View(list);
        var s = query.Trim();
        list = list.Where(x =>
            x.StudentCardNumber.Contains(s, StringComparison.OrdinalIgnoreCase)
            || x.StudentFullName.Contains(s, StringComparison.OrdinalIgnoreCase)
            || x.BookNumber.Contains(s, StringComparison.OrdinalIgnoreCase)
            || x.BookTitle.Contains(s, StringComparison.OrdinalIgnoreCase)
        ).ToList();
        return View(list);
    }

    public async Task<IActionResult> Borrowers(string? q = null, CancellationToken cancellationToken = default)
    {
        ViewBag.Q = q ?? "";
        var list = await reports.GetStudentsWithActiveLoansAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(q)) return View(list);
        var s = q.Trim();
        list = list.Where(x =>
            x.StudentCardNumber.Contains(s, StringComparison.OrdinalIgnoreCase)
            || x.StudentFullName.Contains(s, StringComparison.OrdinalIgnoreCase)
            || x.Books.Any(b => b.BookNumber.Contains(s, StringComparison.OrdinalIgnoreCase) || b.BookTitle.Contains(s, StringComparison.OrdinalIgnoreCase))
        ).ToList();
        return View(list);
    }

    public async Task<IActionResult> Stats(int? year = null, CancellationToken cancellationToken = default)
    {
        ViewBag.Year = year;
        var list = await reports.GetMonthlyBookStatsAsync(year, cancellationToken);
        return View(list);
    }
}

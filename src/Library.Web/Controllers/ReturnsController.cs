using Library.Models.Dtos;
using Library.Models.Interfaces;
using Library.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace Library.Web.Controllers;

public sealed class ReturnsController(ILoanService loans, IBookService books) : Controller
{
    public async Task<IActionResult> Index(string? bookNumber = null, string? query = null,
        CancellationToken cancellationToken = default)
    {
        var viewModel = new ReturnPageViewModel { BookNumber = bookNumber ?? "", Q = query ?? "" };

        if (string.IsNullOrWhiteSpace(viewModel.Q)) return View(viewModel);

        var s = viewModel.Q.Trim();
        var results = await books.SearchAsync(new BookSearchQuery(s, s, null), cancellationToken);
        viewModel.Results = results.Where(b => b.BookNumber.Contains(s, StringComparison.OrdinalIgnoreCase)
                                               || b.Title.Contains(s, StringComparison.OrdinalIgnoreCase)
                                               || b.AuthorOrEditor.Contains(s, StringComparison.OrdinalIgnoreCase))
            .Take(15).ToList();

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Index(ReturnPageViewModel viewModel)
    {
        if (!ModelState.IsValid) return View(viewModel);
        return RedirectToAction(nameof(Verify), new { bookNumber = viewModel.BookNumber });
    }

    public async Task<IActionResult> Verify(string bookNumber, CancellationToken cancellationToken)
    {
        var res = await loans.VerifyReturnAsync(bookNumber, cancellationToken);
        if (!res.IsSuccess || res.Value is null) return BadRequest(res.Error);

        return View(new ReturnVerifyViewModel { BookNumber = bookNumber, Verification = res.Value });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Confirm(ReturnViewModel viewModel, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return RedirectToAction(nameof(Index), new { bookNumber = viewModel.BookNumber });

        var res = await loans.ConfirmReturnAsync(viewModel.BookNumber, cancellationToken);
        if (!res.IsSuccess)
        {
            TempData["Error"] = res.Error ?? "Fehler.";
            return RedirectToAction(nameof(Verify), new { bookNumber = viewModel.BookNumber });
        }

        TempData["Success"] = "Rückgabe gespeichert. (Statistik anonymisiert aktualisiert.)";
        return RedirectToAction(nameof(Index));
    }
}
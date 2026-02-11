using Library.Models.Dtos;
using Library.Models.Interfaces;
using Library.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace Library.Web.Controllers;

public sealed class LoansController(ILoanService loans, IBookService books, IStudentService students) : Controller
{
    public async Task<IActionResult> Checkout(string? studentCardNumber = null, string? bookNumber = null,
        string? studentQ = null, string? bookQ = null, CancellationToken cancellationToken = default)
    {
        return View(await BuildCheckoutPageVm(studentCardNumber, bookNumber, studentQ, bookQ, cancellationToken));
    }

    private async Task<CheckoutPageViewModel> BuildCheckoutPageVm(string? studentCardNumber, string? bookNumber,
        string? studentQ, string? bookQ, CancellationToken cancellationToken)
    {
        var viewModel = new CheckoutPageViewModel
        {
            StudentCardNumber = studentCardNumber ?? "",
            BookNumber = bookNumber ?? "",
            StudentQ = studentQ ?? "",
            BookQ = bookQ ?? ""
        };

        if (!string.IsNullOrWhiteSpace(viewModel.StudentQ))
        {
            var query = viewModel.StudentQ.Trim();
            viewModel.StudentResults = await students.SearchAsync(new StudentSearchQuery(query), cancellationToken);
        }

        if (!string.IsNullOrWhiteSpace(viewModel.BookQ))
        {
            var q = viewModel.BookQ.Trim();
            viewModel.BookResults = await books.SearchAsync(new BookSearchQuery(q, q, null), cancellationToken);
            viewModel.BookResults = viewModel.BookResults
                .Where(b => b.BookNumber.Contains(q, StringComparison.OrdinalIgnoreCase)
                            || b.Title.Contains(q, StringComparison.OrdinalIgnoreCase)
                            || b.AuthorOrEditor.Contains(q, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        return viewModel;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Checkout(CheckoutPageViewModel viewModel)
    {
        if (!ModelState.IsValid) return View(viewModel);
        return RedirectToAction(nameof(Verify),
            new { studentCardNumber = viewModel.StudentCardNumber, bookNumber = viewModel.BookNumber });
    }

    public async Task<IActionResult> Verify(string studentCardNumber, string bookNumber,
        CancellationToken cancellationToken)
    {
        var req = new CheckoutRequestDto(studentCardNumber, bookNumber);
        var res = await loans.VerifyCheckoutAsync(req, cancellationToken);

        if (!res.IsSuccess || res.Value is null)
            return BadRequest(res.Error);

        var bookExists = (await books.GetByBookNumberAsync(bookNumber, cancellationToken)).IsSuccess;

        ViewBag.BookExists = bookExists;
        ViewBag.StudentExists = (await students.GetByCardNumberAsync(studentCardNumber, cancellationToken)).IsSuccess;

        return View(new CheckoutVerifyViewModel { Request = req, Verification = res.Value });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Confirm(CheckoutViewModel viewModel, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return RedirectToAction(nameof(Checkout), new { viewModel.StudentCardNumber, viewModel.BookNumber });

        var res = await loans.ConfirmCheckoutAsync(
            new CheckoutRequestDto(viewModel.StudentCardNumber, viewModel.BookNumber), cancellationToken);
        if (!res.IsSuccess)
        {
            TempData["Error"] = res.Error ?? "Fehler.";
            return RedirectToAction(nameof(Verify),
                new { studentCardNumber = viewModel.StudentCardNumber, bookNumber = viewModel.BookNumber });
        }

        TempData["Success"] = "Ausleihe gespeichert.";
        return RedirectToAction(nameof(Active));
    }

    public async Task<IActionResult> Active(string? query = null, CancellationToken cancellationToken = default)
    {
        ViewBag.Q = query ?? "";
        var list = await loans.GetActiveLoansAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(query)) return View(list);
        var s = query.Trim();
        list = list.Where(x =>
            x.BookNumber.Contains(s, StringComparison.OrdinalIgnoreCase)
            || x.BookTitle.Contains(s, StringComparison.OrdinalIgnoreCase)
            || x.StudentCardNumber.Contains(s, StringComparison.OrdinalIgnoreCase)
            || x.StudentFullName.Contains(s, StringComparison.OrdinalIgnoreCase)
        ).ToList();
        return View(list);
    }
}
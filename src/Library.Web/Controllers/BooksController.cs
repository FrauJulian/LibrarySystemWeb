using Library.Models.Dtos;
using Library.Models.Interfaces;
using Library.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Library.Web.Controllers;

public sealed class BooksController(IBookService books) : Controller
{
    public async Task<IActionResult> Index([FromQuery] BookSearchViewModel viewModel,
        CancellationToken cancellationToken)
    {
        await LoadSubjectsAsync(cancellationToken, viewModel.SubjectId);
        var list = await books.SearchAsync(
            new BookSearchQuery(viewModel.TitleContains, viewModel.AuthorContains, viewModel.SubjectId),
            cancellationToken);
        ViewBag.Search = viewModel;
        return View(list);
    }

    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var res = await books.GetByIdAsync(id, cancellationToken);
        if (!res.IsSuccess || res.Value is null) return NotFound(res.Error);
        return View(res.Value);
    }

    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        await LoadSubjectsAsync(cancellationToken, null);
        return View(new BookUpsertViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BookUpsertViewModel viewModel, CancellationToken cancellationToken)
    {
        await LoadSubjectsAsync(cancellationToken, viewModel.SubjectId);
        if (!ModelState.IsValid) return View(viewModel);

        var res = await books.CreateAsync(new BookUpsertDto(
            viewModel.BookNumber, viewModel.Title, viewModel.AuthorOrEditor, viewModel.SubjectId, viewModel.Isbn,
            viewModel.Publisher, viewModel.PublisherCity, viewModel.PublishedOn
        ), cancellationToken);

        if (res.IsSuccess) return RedirectToAction(nameof(Details), new { id = res.Value });

        ModelState.AddModelError(string.Empty, res.Error ?? "Fehler.");
        return View(viewModel);
    }

    public async Task<IActionResult> CreateFromCheckout(string bookNumber, string studentCardNumber,
        CancellationToken cancellationToken)
    {
        await LoadSubjectsAsync(cancellationToken, null);
        var viewModel = new BookCreateFromCheckoutViewModel
        {
            BookNumber = bookNumber,
            StudentCardNumber = studentCardNumber
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateFromCheckout(BookCreateFromCheckoutViewModel viewModel,
        CancellationToken cancellationToken)
    {
        await LoadSubjectsAsync(cancellationToken, viewModel.SubjectId);
        if (!ModelState.IsValid) return View(viewModel);

        var res = await books.CreateAsync(new BookUpsertDto(
            viewModel.BookNumber, viewModel.Title, viewModel.AuthorOrEditor, viewModel.SubjectId, viewModel.Isbn,
            viewModel.Publisher, viewModel.PublisherCity, viewModel.PublishedOn
        ), cancellationToken);

        if (res.IsSuccess)
            return RedirectToAction("Verify", "Loans",
                new { studentCardNumber = viewModel.StudentCardNumber, bookNumber = viewModel.BookNumber });

        ModelState.AddModelError(string.Empty, res.Error ?? "Fehler.");
        return View(viewModel);
    }

    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var res = await books.GetByIdAsync(id, cancellationToken);
        if (!res.IsSuccess || res.Value is null) return NotFound(res.Error);

        await LoadSubjectsAsync(cancellationToken, res.Value.SubjectId);

        var vm = new BookUpsertViewModel
        {
            BookNumber = res.Value.BookNumber,
            Title = res.Value.Title,
            AuthorOrEditor = res.Value.AuthorOrEditor,
            SubjectId = res.Value.SubjectId,
            Isbn = res.Value.Isbn,
            Publisher = res.Value.Publisher,
            PublisherCity = res.Value.PublisherCity,
            PublishedOn = res.Value.PublishedOn
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, BookUpsertViewModel viewModel, CancellationToken cancellationToken)
    {
        await LoadSubjectsAsync(cancellationToken, viewModel.SubjectId);
        if (!ModelState.IsValid) return View(viewModel);

        var res = await books.UpdateAsync(id, new BookUpsertDto(
            viewModel.BookNumber, viewModel.Title, viewModel.AuthorOrEditor, viewModel.SubjectId, viewModel.Isbn,
            viewModel.Publisher, viewModel.PublisherCity, viewModel.PublishedOn
        ), cancellationToken);

        if (res.IsSuccess) return RedirectToAction(nameof(Details), new { id });

        ModelState.AddModelError(string.Empty, res.Error ?? "Fehler.");
        return View(viewModel);
    }

    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var res = await books.GetByIdAsync(id, cancellationToken);
        if (!res.IsSuccess || res.Value is null) return NotFound(res.Error);
        return View(res.Value);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken cancellationToken)
    {
        var res = await books.DeleteAsync(id, cancellationToken);
        if (!res.IsSuccess) return BadRequest(res.Error);
        return RedirectToAction(nameof(Index));
    }

    private async Task LoadSubjectsAsync(CancellationToken cancellationToken, int? selected)
    {
        var subjects = await books.GetSubjectsAsync(cancellationToken);
        ViewBag.Subjects = subjects.Select(student =>
            new SelectListItem(student.Name, student.SubjectId.ToString(), selected == student.SubjectId)).ToList();
    }
}
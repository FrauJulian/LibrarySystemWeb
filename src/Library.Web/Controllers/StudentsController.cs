using Library.Models.Dtos;
using Library.Models.Interfaces;
using Library.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace Library.Web.Controllers;

public sealed class StudentsController(IStudentService students) : Controller
{
    public async Task<IActionResult> Index([FromQuery] StudentSearchViewModel viewModel,
        CancellationToken cancellationToken)
    {
        var list = await students.SearchAsync(new StudentSearchQuery(viewModel.NameContains), cancellationToken);
        ViewBag.Search = viewModel;
        return View(list);
    }

    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var res = await students.GetByIdAsync(id, cancellationToken);
        if (!res.IsSuccess || res.Value is null) return NotFound(res.Error);
        return View(res.Value);
    }

    public IActionResult Create()
    {
        return View(new StudentUpsertViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(StudentUpsertViewModel viewModel, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return View(viewModel);

        var res = await students.CreateAsync(new StudentUpsertDto(
            viewModel.CardNumber, viewModel.FirstName, viewModel.LastName, viewModel.IsActive
        ), cancellationToken);

        if (res.IsSuccess) return RedirectToAction(nameof(Details), new { id = res.Value });

        ModelState.AddModelError(string.Empty, res.Error ?? "Fehler.");
        return View(viewModel);
    }

    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var res = await students.GetByIdAsync(id, cancellationToken);
        if (!res.IsSuccess || res.Value is null) return NotFound(res.Error);

        var vm = new StudentUpsertViewModel
        {
            CardNumber = res.Value.CardNumber,
            FirstName = res.Value.FirstName,
            LastName = res.Value.LastName,
            IsActive = res.Value.IsActive
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, StudentUpsertViewModel viewModel, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return View(viewModel);

        var res = await students.UpdateAsync(id, new StudentUpsertDto(
            viewModel.CardNumber, viewModel.FirstName, viewModel.LastName, viewModel.IsActive
        ), cancellationToken);

        if (res.IsSuccess) return RedirectToAction(nameof(Details), new { id });

        ModelState.AddModelError(string.Empty, res.Error ?? "Fehler.");
        return View(viewModel);
    }

    public async Task<IActionResult> Deactivate(int id, CancellationToken cancellationToken)
    {
        var res = await students.GetByIdAsync(id, cancellationToken);
        if (!res.IsSuccess || res.Value is null) return NotFound(res.Error);
        return View(res.Value);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeactivateConfirmed(int id, CancellationToken cancellationToken)
    {
        var res = await students.DeactivateAsync(id, cancellationToken);
        if (!res.IsSuccess) return BadRequest(res.Error);
        return RedirectToAction(nameof(Index));
    }
}
using Microsoft.AspNetCore.Mvc;
using PharmacyManagement.Models;
using PharmacyManagement.Services;

namespace PharmacyManagement.Controllers;

public class MedicineController : Controller
{
    private readonly IMedicineService _service;
    private readonly ICategoryService _categoryService;
    private readonly ISupplierService _supplierService;
    private readonly ILogger<MedicineController> _logger;

    public MedicineController(
        IMedicineService service,
        ICategoryService categoryService,
        ISupplierService supplierService,
        ILogger<MedicineController> logger)
    {
        _service = service;
        _categoryService = categoryService;
        _supplierService = supplierService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? search)
    {
        var medicines = await _service.GetAllAsync(search);

        ViewData["Search"] = search;

        return View(medicines);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var medicine = await _service.GetByIdAsync(id);

        if (medicine is null)
            return NotFound();

        return View(medicine);
    }

    // ============================
    // CREATE
    // ============================

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await LoadDropdownsAsync();

        return View(new Medicine
        {
            IsActive = true
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Medicine medicine)
    {
        if (!ModelState.IsValid)
        {
            await LoadDropdownsAsync();

            return View(medicine);
        }

        var id = await _service.AddAsync(medicine);

        _logger.LogInformation(
            "Medicine {MedicineId} created.",
            id);

        TempData["Success"] =
            "Medicine added successfully.";

        return RedirectToAction(nameof(Index));
    }

    // ============================
    // EDIT
    // ============================

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var medicine = await _service.GetByIdAsync(id);

        if (medicine is null)
            return NotFound();

        await LoadDropdownsAsync();

        return View(medicine);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Medicine medicine)
    {
        if (!ModelState.IsValid)
        {
            await LoadDropdownsAsync();

            return View(medicine);
        }

        var updated = await _service.UpdateAsync(medicine);

        if (!updated)
            return NotFound();

        TempData["Success"] =
            "Medicine updated successfully.";

        return RedirectToAction(nameof(Index));
    }

    // ============================
    // DELETE
    // ============================

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var medicine = await _service.GetByIdAsync(id);

        if (medicine is null)
            return NotFound();

        return View(medicine);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var deleted = await _service.DeleteAsync(id);

        if (!deleted)
            return NotFound();

        TempData["Success"] =
            "Medicine deleted successfully.";

        return RedirectToAction(nameof(Index));
    }

    // ============================
    // DROPDOWN DATA
    // ============================

    private async Task LoadDropdownsAsync()
    {
        ViewBag.Categories =
            await _categoryService.GetAllAsync();

        ViewBag.Suppliers =
            await _supplierService.GetAllAsync();
    }
}
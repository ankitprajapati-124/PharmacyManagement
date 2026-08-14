using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyManagement.Models;
using PharmacyManagement.Services;

namespace PharmacyManagement.Controllers;

[Authorize]
public class MedicineController : Controller
{
    private readonly IMedicineService _service;
    private readonly ICategoryService _categoryService;
    private readonly ISupplierService _supplierService;
    private readonly IAuditLogService _auditLogService;
    private readonly ILogger<MedicineController> _logger;

    public MedicineController(
        IMedicineService service,
        ICategoryService categoryService,
        ISupplierService supplierService,
        IAuditLogService auditLogService,
        ILogger<MedicineController> logger)
    {
        _service = service;
        _categoryService = categoryService;
        _supplierService = supplierService;
        _auditLogService = auditLogService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? search)
    {
        var medicines =
            await _service.GetAllAsync(search);

        ViewData["Search"] = search;

        return View(medicines);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var medicine =
            await _service.GetByIdAsync(id);

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
    public async Task<IActionResult> Create(
        Medicine medicine)
    {
        if (!ModelState.IsValid)
        {
            await LoadDropdownsAsync();

            return View(medicine);
        }

        var id =
            await _service.AddAsync(medicine);

        await _auditLogService.LogAsync(
            "Create",
            "Medicine",
            id,
            $"Medicine '{medicine.MedicineName}' was created.");

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
        var medicine =
            await _service.GetByIdAsync(id);

        if (medicine is null)
            return NotFound();

        await LoadDropdownsAsync();

        return View(medicine);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        Medicine medicine)
    {
        if (!ModelState.IsValid)
        {
            await LoadDropdownsAsync();

            return View(medicine);
        }

        var updated =
            await _service.UpdateAsync(medicine);

        if (!updated)
            return NotFound();

        await _auditLogService.LogAsync(
            "Update",
            "Medicine",
            medicine.MedicineId,
            $"Medicine '{medicine.MedicineName}' was updated.");

        _logger.LogInformation(
            "Medicine {MedicineId} updated.",
            medicine.MedicineId);

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
        var medicine =
            await _service.GetByIdAsync(id);

        if (medicine is null)
            return NotFound();

        return View(medicine);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(
        int id)
    {
        var medicine =
            await _service.GetByIdAsync(id);

        if (medicine is null)
            return NotFound();

        var deleted =
            await _service.DeleteAsync(id);

        if (!deleted)
            return NotFound();

        await _auditLogService.LogAsync(
            "Delete",
            "Medicine",
            id,
            $"Medicine '{medicine.MedicineName}' was deleted.");

        _logger.LogInformation(
            "Medicine {MedicineId} deleted.",
            id);

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
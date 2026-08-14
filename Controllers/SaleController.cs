using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyManagement.Models;
using PharmacyManagement.Services;

namespace PharmacyManagement.Controllers;

[Authorize]
public class SaleController : Controller
{
    private readonly ISaleService _service;
    private readonly IMedicineService _medicineService;
    private readonly IAuditLogService _auditLogService;
    private readonly ILogger<SaleController> _logger;

    public SaleController(
        ISaleService service,
        IMedicineService medicineService,
        IAuditLogService auditLogService,
        ILogger<SaleController> logger)
    {
        _service = service;
        _medicineService = medicineService;
        _auditLogService = auditLogService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var sales =
            await _service.GetAllAsync();

        return View(sales);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var sale =
            await _service.GetByIdAsync(id);

        if (sale is null)
            return NotFound();

        return View(sale);
    }

    // ============================
    // CREATE
    // ============================

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await LoadMedicinesAsync();

        return View(new Sale
        {
            SaleDate = DateTime.Now,
            Discount = 0
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        Sale sale)
    {
        if (!ModelState.IsValid)
        {
            await LoadMedicinesAsync();

            return View(sale);
        }

        try
        {
            var id =
                await _service.AddAsync(sale);

            await _auditLogService.LogAsync(
                "Create",
                "Sale",
                id,
                $"Sale #{id} was created and medicine stock was updated.");

            _logger.LogInformation(
                "Sale {SaleId} created.",
                id);

            TempData["Success"] =
                "Sale completed successfully and medicine stock updated.";

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error while creating sale.");

            ModelState.AddModelError(
                "",
                ex.Message);

            await LoadMedicinesAsync();

            return View(sale);
        }
    }

    // ============================
    // DELETE
    // ============================

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var sale =
            await _service.GetByIdAsync(id);

        if (sale is null)
            return NotFound();

        return View(sale);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(
        int id)
    {
        try
        {
            var sale =
                await _service.GetByIdAsync(id);

            if (sale is null)
                return NotFound();

            var deleted =
                await _service.DeleteAsync(id);

            if (!deleted)
                return NotFound();

            await _auditLogService.LogAsync(
                "Delete",
                "Sale",
                id,
                $"Sale #{id} was deleted and medicine stock was restored.");

            _logger.LogInformation(
                "Sale {SaleId} deleted.",
                id);

            TempData["Success"] =
                "Sale deleted and medicine stock restored.";

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error while deleting sale {SaleId}.",
                id);

            TempData["Error"] =
                ex.Message;

            return RedirectToAction(nameof(Index));
        }
    }

    // ============================
    // MEDICINE DROPDOWN
    // ============================

    private async Task LoadMedicinesAsync()
    {
        var medicines =
            await _medicineService.GetAllAsync();

        ViewBag.Medicines = medicines;
    }
}
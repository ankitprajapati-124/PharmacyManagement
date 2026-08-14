using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyManagement.Models;
using PharmacyManagement.Services;

namespace PharmacyManagement.Controllers;

[Authorize(Roles = "Admin,Pharmacist")]
public class PurchaseController : Controller
{
    private readonly IPurchaseService _service;
    private readonly ISupplierService _supplierService;
    private readonly IMedicineService _medicineService;
    private readonly IAuditLogService _auditLogService;
    private readonly ILogger<PurchaseController> _logger;

    public PurchaseController(
        IPurchaseService service,
        ISupplierService supplierService,
        IMedicineService medicineService,
        IAuditLogService auditLogService,
        ILogger<PurchaseController> logger)
    {
        _service = service;
        _supplierService = supplierService;
        _medicineService = medicineService;
        _auditLogService = auditLogService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var purchases =
            await _service.GetAllAsync();

        return View(purchases);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var purchase =
            await _service.GetByIdAsync(id);

        if (purchase is null)
            return NotFound();

        return View(purchase);
    }

    // ============================
    // CREATE
    // ============================

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await LoadDropdownsAsync();

        return View(new Purchase
        {
            PurchaseDate = DateTime.Today
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        Purchase purchase)
    {
        if (!ModelState.IsValid)
        {
            await LoadDropdownsAsync();

            return View(purchase);
        }

        try
        {
            var id =
                await _service.AddAsync(purchase);

            await _auditLogService.LogAsync(
                "Create",
                "Purchase",
                id,
                $"Purchase #{id} was created and medicine stock was updated.");

            _logger.LogInformation(
                "Purchase {PurchaseId} created.",
                id);

            TempData["Success"] =
                "Purchase added successfully and medicine stock updated.";

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error while creating purchase.");

            ModelState.AddModelError(
                "",
                ex.Message);

            await LoadDropdownsAsync();

            return View(purchase);
        }
    }

    // ============================
    // DELETE
    // ============================

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var purchase =
            await _service.GetByIdAsync(id);

        if (purchase is null)
            return NotFound();

        return View(purchase);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(
        int id)
    {
        try
        {
            var purchase =
                await _service.GetByIdAsync(id);

            if (purchase is null)
                return NotFound();

            var deleted =
                await _service.DeleteAsync(id);

            if (!deleted)
                return NotFound();

            await _auditLogService.LogAsync(
                "Delete",
                "Purchase",
                id,
                $"Purchase #{id} was deleted and medicine stock was reversed.");

            _logger.LogInformation(
                "Purchase {PurchaseId} deleted.",
                id);

            TempData["Success"] =
                "Purchase deleted and medicine stock reversed.";

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error while deleting purchase {PurchaseId}.",
                id);

            TempData["Error"] =
                ex.Message;

            return RedirectToAction(nameof(Index));
        }
    }

    // ============================
    // DROPDOWN DATA
    // ============================

    private async Task LoadDropdownsAsync()
    {
        var suppliers =
            await _supplierService.GetAllAsync();

        var medicines =
            await _medicineService.GetAllAsync();

        ViewBag.Suppliers = suppliers;
        ViewBag.Medicines = medicines;
   }
}
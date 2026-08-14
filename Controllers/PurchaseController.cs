using System.Security.Claims;
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

    // =========================================================
    // CURRENT USER
    // =========================================================

    private int GetCurrentUserId()
    {
        var userIdValue =
            User.FindFirst(
                ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(
                userIdValue,
                out var userId))
        {
            throw new InvalidOperationException(
                "Unable to determine the logged-in user.");
        }

        return userId;
    }

    // =========================================================
    // INDEX
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var userId =
            GetCurrentUserId();

        var isAdmin =
            User.IsInRole("Admin");

        var purchases =
            await _service.GetAllAsync(
                userId,
                isAdmin);

        return View(purchases);
    }

    // =========================================================
    // DETAILS
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> Details(
        int id)
    {
        var userId =
            GetCurrentUserId();

        var isAdmin =
            User.IsInRole("Admin");

        var purchase =
            await _service.GetByIdAsync(
                id,
                userId,
                isAdmin);

        if (purchase is null)
            return NotFound();

        return View(purchase);
    }

    // =========================================================
    // CREATE
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await LoadDropdownsAsync();

        return View(
            new Purchase
            {
                PurchaseDate =
                    DateTime.Today
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
            // UserId is taken from authentication,
            // NOT from the submitted form.
            var userId =
                GetCurrentUserId();

            var id =
                await _service.AddAsync(
                    purchase,
                    userId);

            await _auditLogService.LogAsync(
                "Create",
                "Purchase",
                id,
                $"Purchase #{id} was created and medicine stock was updated.");

            _logger.LogInformation(
                "Purchase {PurchaseId} created by User {UserId}.",
                id,
                userId);

            TempData["Success"] =
                "Purchase added successfully and medicine stock updated.";

            return RedirectToAction(
                nameof(Index));
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

    // =========================================================
    // DELETE - GET
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> Delete(
        int id)
    {
        var userId =
            GetCurrentUserId();

        var isAdmin =
            User.IsInRole("Admin");

        var purchase =
            await _service.GetByIdAsync(
                id,
                userId,
                isAdmin);

        if (purchase is null)
            return NotFound();

        return View(purchase);
    }

    // =========================================================
    // DELETE - POST
    // =========================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(
        int id)
    {
        try
        {
            var userId =
                GetCurrentUserId();

            var isAdmin =
                User.IsInRole("Admin");

            // Verify ownership/access first.
            var purchase =
                await _service.GetByIdAsync(
                    id,
                    userId,
                    isAdmin);

            if (purchase is null)
                return NotFound();

            var deleted =
                await _service.DeleteAsync(
                    id,
                    userId,
                    isAdmin);

            if (!deleted)
                return NotFound();

            await _auditLogService.LogAsync(
                "Delete",
                "Purchase",
                id,
                $"Purchase #{id} was deleted and medicine stock was reversed.");

            _logger.LogInformation(
                "Purchase {PurchaseId} deleted by User {UserId}.",
                id,
                userId);

            TempData["Success"] =
                "Purchase deleted and medicine stock reversed.";

            return RedirectToAction(
                nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error while deleting purchase {PurchaseId}.",
                id);

            TempData["Error"] =
                ex.Message;

            return RedirectToAction(
                nameof(Index));
        }
    }

    // =========================================================
    // DROPDOWNS
    // =========================================================

    private async Task LoadDropdownsAsync()
    {
        var suppliers =
            await _supplierService.GetAllAsync();

        var medicines =
            await _medicineService.GetAllAsync();

        ViewBag.Suppliers =
            suppliers;

        ViewBag.Medicines =
            medicines;
    }
}
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyManagement.Services;

namespace PharmacyManagement.Controllers;

[Authorize(Roles = "Admin,Pharmacist")]
public class ReportsController : Controller
{
    private readonly IReportService _reportService;
    private readonly ILogger<ReportsController> _logger;

    public ReportsController(
        IReportService reportService,
        ILogger<ReportsController> logger)
    {
        _reportService = reportService;
        _logger = logger;
    }

    // ============================
    // CURRENT USER
    // ============================

    private int GetCurrentUserId()
    {
        var userId =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier);

        if (!int.TryParse(
                userId,
                out var id))
        {
            throw new InvalidOperationException(
                "Current user ID is missing.");
        }

        return id;
    }

    private bool IsAdmin()
    {
        return User.IsInRole("Admin");
    }


    // ============================
    // SALES REPORT
    // ============================

    [HttpGet]
    public async Task<IActionResult> Sales(
        DateTime? fromDate,
        DateTime? toDate)
    {
        var currentUserId =
            GetCurrentUserId();

        var isAdmin =
            IsAdmin();

        try
        {
            if (fromDate.HasValue &&
                toDate.HasValue &&
                fromDate.Value.Date >
                toDate.Value.Date)
            {
                TempData["Error"] =
                    "From Date cannot be later than To Date.";

                return View(
                    await _reportService
                        .GetSalesReportAsync(
                            null,
                            null,
                            currentUserId,
                            isAdmin));
            }

            var report =
                await _reportService
                    .GetSalesReportAsync(
                        fromDate,
                        toDate,
                        currentUserId,
                        isAdmin);

            return View(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error while loading sales report.");

            TempData["Error"] =
                "Unable to load sales report.";

            return View(
                await _reportService
                    .GetSalesReportAsync(
                        null,
                        null,
                        currentUserId,
                        isAdmin));
        }
    }


    // ============================
    // PURCHASE REPORT
    // ============================

    [HttpGet]
    public async Task<IActionResult> Purchases(
        DateTime? fromDate,
        DateTime? toDate)
    {
        var currentUserId =
            GetCurrentUserId();

        var isAdmin =
            IsAdmin();

        try
        {
            if (fromDate.HasValue &&
                toDate.HasValue &&
                fromDate.Value.Date >
                toDate.Value.Date)
            {
                TempData["Error"] =
                    "From Date cannot be later than To Date.";

                return View(
                    await _reportService
                        .GetPurchaseReportAsync(
                            null,
                            null,
                            currentUserId,
                            isAdmin));
            }

            var report =
                await _reportService
                    .GetPurchaseReportAsync(
                        fromDate,
                        toDate,
                        currentUserId,
                        isAdmin);

            return View(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error while loading purchase report.");

            TempData["Error"] =
                "Unable to load purchase report.";

            return View(
                await _reportService
                    .GetPurchaseReportAsync(
                        null,
                        null,
                        currentUserId,
                        isAdmin));
        }
    }


    // ============================
    // STOCK REPORT
    // ============================

    [HttpGet]
    public async Task<IActionResult> Stock()
    {
        try
        {
            var report =
                await _reportService
                    .GetStockReportAsync();

            return View(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error while loading stock report.");

            TempData["Error"] =
                "Unable to load stock report.";

            return RedirectToAction(
                nameof(Sales));
        }
    }
}
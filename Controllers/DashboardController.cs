using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyManagement.Services;

namespace PharmacyManagement.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly IDashboardService _service;

    public DashboardController(
        IDashboardService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var userIdValue =
            User.FindFirst(
                ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(
                userIdValue,
                out var userId))
        {
            return Unauthorized();
        }

        var isAdmin =
            User.IsInRole("Admin");

        var canViewPurchases =
            User.IsInRole("Admin") ||
            User.IsInRole("Pharmacist");

        var dashboard =
            await _service.GetDashboardAsync(
                userId,
                isAdmin,
                canViewPurchases);

        return View(dashboard);
    }
}
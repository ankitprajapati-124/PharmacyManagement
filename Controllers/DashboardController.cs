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
        var dashboard =
            await _service.GetDashboardAsync();

        return View(dashboard);
    }
}
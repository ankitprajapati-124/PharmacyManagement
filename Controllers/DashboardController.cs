using Microsoft.AspNetCore.Mvc;

namespace PharmacyManagement.Controllers;

public class DashboardController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}

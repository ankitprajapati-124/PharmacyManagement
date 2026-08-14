using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyManagement.Services;

namespace PharmacyManagement.Controllers;

[Authorize(Roles = "Admin")]
public class AuditLogsController : Controller
{
    private readonly IAuditLogService _auditLogService;
    private readonly ILogger<AuditLogsController> _logger;

    public AuditLogsController(
        IAuditLogService auditLogService,
        ILogger<AuditLogsController> logger)
    {
        _auditLogService = auditLogService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        try
        {
            var logs = await _auditLogService.GetAllAsync();

            return View(logs);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error while loading audit logs.");

            TempData["Error"] =
                "Unable to load audit logs.";

            return View(
                Array.Empty<Models.AuditLog>());
        }
    }
}
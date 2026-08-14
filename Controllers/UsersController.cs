using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyManagement.Models;
using PharmacyManagement.Services;

namespace PharmacyManagement.Controllers;

[Authorize(Roles = "Admin")]
public class UsersController : Controller
{
    private readonly IUserService _userService;
    private readonly IAuditLogService _auditLogService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(
        IUserService userService,
        IAuditLogService auditLogService,
        ILogger<UsersController> logger)
    {
        _userService = userService;
        _auditLogService = auditLogService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var users =
            await _userService.GetAllAsync();

        return View(users);
    }

    // ============================
    // CREATE USER
    // ============================

    [HttpGet]
    public IActionResult Create()
    {
        return View(
            new CreateUserViewModel
            {
                Role = "Staff"
            });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        CreateUserViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            var userId =
                await _userService.CreateAsync(
                    model.Username,
                    model.FullName,
                    model.Password,
                    model.Role);

            await _auditLogService.LogAsync(
                "Create",
                "User",
                userId,
                $"User '{model.Username}' was created with role '{model.Role}'.");

            _logger.LogInformation(
                "Admin created user {UserId} with username {Username}.",
                userId,
                model.Username);

            TempData["Success"] =
                $"User '{model.Username}' created successfully.";

            return RedirectToAction(
                nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error while creating user {Username}.",
                model.Username);

            ModelState.AddModelError(
                "",
                ex.Message);

            return View(model);
        }
    }

    // ============================
    // ACTIVATE / DEACTIVATE
    // ============================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetActive(
        int id,
        bool isActive)
    {
        try
        {
            var currentUserId =
                User.FindFirst(
                    System.Security.Claims.ClaimTypes.NameIdentifier);

            if (int.TryParse(
                    currentUserId?.Value,
                    out var loggedInUserId) &&
                loggedInUserId == id &&
                !isActive)
            {
                TempData["Error"] =
                    "You cannot deactivate your own account.";

                return RedirectToAction(
                    nameof(Index));
            }

            var user =
                await GetUserAsync(id);

            if (user is null)
                return NotFound();

            var updated =
                await _userService.SetActiveAsync(
                    id,
                    isActive);

            if (!updated)
                return NotFound();

            var action =
                isActive
                    ? "Activate"
                    : "Deactivate";

            await _auditLogService.LogAsync(
                action,
                "User",
                id,
                $"User '{user.Username}' was " +
                $"{(isActive ? "activated" : "deactivated")}.");

            _logger.LogInformation(
                "User {UserId} active status changed to {IsActive}.",
                id,
                isActive);

            TempData["Success"] =
                isActive
                    ? "User activated successfully."
                    : "User deactivated successfully.";

            return RedirectToAction(
                nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error while changing active status for user {UserId}.",
                id);

            TempData["Error"] =
                ex.Message;

            return RedirectToAction(
                nameof(Index));
        }
    }

    // ============================
    // PRIVATE HELPER
    // ============================

    private async Task<Models.User?> GetUserAsync(
        int id)
    {
        var users =
            await _userService.GetAllAsync();

        return users.FirstOrDefault(
            user => user.UserId == id);
    }
}
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using PharmacyManagement.Models;
using PharmacyManagement.Services;

namespace PharmacyManagement.Controllers;

public class AccountController : Controller
{
    private readonly IAuthService _authService;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        IAuthService authService,
        ILogger<AccountController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Login(
        string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction(
                "Index",
                "Dashboard");
        }

        ViewBag.ReturnUrl = returnUrl;

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(
        LoginViewModel model,
        string? returnUrl = null)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user =
            await _authService.AuthenticateAsync(
                model.Username,
                model.Password);

        if (user is null)
        {
            ModelState.AddModelError(
                "",
                "Invalid username or password.");

            return View(model);
        }

        var claims = new List<Claim>
        {
            new(
                ClaimTypes.NameIdentifier,
                user.UserId.ToString()),

            new(
                ClaimTypes.Name,
                user.Username),

            new(
                ClaimTypes.GivenName,
                user.FullName),

            new(
                ClaimTypes.Role,
                user.Role)
        };

        var identity =
            new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults
                    .AuthenticationScheme);

        var principal =
            new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults
                .AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent =
                    model.RememberMe,

                ExpiresUtc =
                    DateTimeOffset.UtcNow
                    .AddHours(8)
            });

        _logger.LogInformation(
            "User {Username} logged in.",
            user.Username);

        if (!string.IsNullOrWhiteSpace(returnUrl) &&
            Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction(
            "Index",
            "Dashboard");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(
            CookieAuthenticationDefaults
                .AuthenticationScheme);

        return RedirectToAction(
            nameof(Login));
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }
}
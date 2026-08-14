using System.Security.Claims;
using PharmacyManagement.Models;
using PharmacyManagement.Repositories;

namespace PharmacyManagement.Services;

public class AuditLogService : IAuditLogService
{
    private readonly IAuditLogRepository _repository;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditLogService(
        IAuditLogRepository repository,
        IHttpContextAccessor httpContextAccessor)
    {
        _repository = repository;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<int> LogAsync(
        string action,
        string? entityName = null,
        int? entityId = null,
        string? description = null,
        int? userId = null,
        string? username = null)
    {
        var currentUser =
            _httpContextAccessor.HttpContext?.User;

        // Use explicitly supplied user information first.
        // This is important for Login because the authentication
        // cookie has only just been created.
        if (!userId.HasValue)
        {
            var userIdValue =
                currentUser?.FindFirst(
                    ClaimTypes.NameIdentifier)?.Value;

            if (int.TryParse(
                    userIdValue,
                    out var parsedUserId))
            {
                userId = parsedUserId;
            }
        }

        if (string.IsNullOrWhiteSpace(username))
        {
            username =
                currentUser?.Identity?.IsAuthenticated == true
                    ? currentUser.Identity.Name
                    : null;
        }

        var auditLog = new AuditLog
        {
            UserId = userId,
            Username = username,
            Action = action,
            EntityName = entityName,
            EntityId = entityId,
            Description = description,
            CreatedAt = DateTime.Now
        };

        return await _repository.AddAsync(auditLog);
    }

    public async Task<IReadOnlyList<AuditLog>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }
}
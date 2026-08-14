using PharmacyManagement.Models;

namespace PharmacyManagement.Services;

public interface IAuditLogService
{
    Task<int> LogAsync(
        string action,
        string? entityName = null,
        int? entityId = null,
        string? description = null,
        int? userId = null,
        string? username = null);

    Task<IReadOnlyList<AuditLog>> GetAllAsync();
}
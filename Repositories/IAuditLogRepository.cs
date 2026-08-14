using PharmacyManagement.Models;

namespace PharmacyManagement.Repositories;

public interface IAuditLogRepository
{
    Task<int> AddAsync(AuditLog auditLog);

    Task<IReadOnlyList<AuditLog>> GetAllAsync();
}
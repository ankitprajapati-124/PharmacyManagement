namespace PharmacyManagement.Models;

public class AuditLog
{
    public int AuditLogId { get; set; }

    public int? UserId { get; set; }

    public string? Username { get; set; }

    public string Action { get; set; } = string.Empty;

    public string? EntityName { get; set; }

    public int? EntityId { get; set; }

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }
}
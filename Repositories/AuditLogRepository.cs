using Microsoft.Data.SqlClient;
using PharmacyManagement.Models;
using System.Data;

namespace PharmacyManagement.Repositories;

public class AuditLogRepository : IAuditLogRepository
{
    private readonly string _connectionString;

    public AuditLogRepository(IConfiguration configuration)
    {
        _connectionString =
            configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "DefaultConnection is not configured.");
    }

    public async Task<int> AddAsync(
        AuditLog auditLog)
    {
        const string sql = """
            INSERT INTO AuditLogs
            (
                UserId,
                Username,
                Action,
                EntityName,
                EntityId,
                Description
            )
            VALUES
            (
                @UserId,
                @Username,
                @Action,
                @EntityName,
                @EntityId,
                @Description
            );

            SELECT CAST(SCOPE_IDENTITY() AS INT);
            """;

        await using var connection =
            new SqlConnection(_connectionString);

        await connection.OpenAsync();

        await using var command =
            new SqlCommand(sql, connection);

        command.Parameters.Add(
            "@UserId",
            SqlDbType.Int).Value =
                (object?)auditLog.UserId ??
                DBNull.Value;

        command.Parameters.Add(
            "@Username",
            SqlDbType.NVarChar,
            100).Value =
                (object?)auditLog.Username ??
                DBNull.Value;

        command.Parameters.Add(
            "@Action",
            SqlDbType.NVarChar,
            100).Value =
                auditLog.Action;

        command.Parameters.Add(
            "@EntityName",
            SqlDbType.NVarChar,
            100).Value =
                (object?)auditLog.EntityName ??
                DBNull.Value;

        command.Parameters.Add(
            "@EntityId",
            SqlDbType.Int).Value =
                (object?)auditLog.EntityId ??
                DBNull.Value;

        command.Parameters.Add(
            "@Description",
            SqlDbType.NVarChar,
            500).Value =
                (object?)auditLog.Description ??
                DBNull.Value;

        return Convert.ToInt32(
            await command.ExecuteScalarAsync());
    }

    public async Task<IReadOnlyList<AuditLog>> GetAllAsync()
    {
        var logs = new List<AuditLog>();

        const string sql = """
            SELECT
                AuditLogId,
                UserId,
                Username,
                Action,
                EntityName,
                EntityId,
                Description,
                CreatedAt
            FROM AuditLogs
            ORDER BY CreatedAt DESC;
            """;

        await using var connection =
            new SqlConnection(_connectionString);

        await connection.OpenAsync();

        await using var command =
            new SqlCommand(sql, connection);

        await using var reader =
            await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            logs.Add(
                new AuditLog
                {
                    AuditLogId =
                        reader.GetInt32(
                            reader.GetOrdinal(
                                "AuditLogId")),

                    UserId =
                        reader.IsDBNull(
                            reader.GetOrdinal(
                                "UserId"))
                            ? null
                            : reader.GetInt32(
                                reader.GetOrdinal(
                                    "UserId")),

                    Username =
                        reader.IsDBNull(
                            reader.GetOrdinal(
                                "Username"))
                            ? null
                            : reader.GetString(
                                reader.GetOrdinal(
                                    "Username")),

                    Action =
                        reader.GetString(
                            reader.GetOrdinal(
                                "Action")),

                    EntityName =
                        reader.IsDBNull(
                            reader.GetOrdinal(
                                "EntityName"))
                            ? null
                            : reader.GetString(
                                reader.GetOrdinal(
                                    "EntityName")),

                    EntityId =
                        reader.IsDBNull(
                            reader.GetOrdinal(
                                "EntityId"))
                            ? null
                            : reader.GetInt32(
                                reader.GetOrdinal(
                                    "EntityId")),

                    Description =
                        reader.IsDBNull(
                            reader.GetOrdinal(
                                "Description"))
                            ? null
                            : reader.GetString(
                                reader.GetOrdinal(
                                    "Description")),

                    CreatedAt =
                        reader.GetDateTime(
                            reader.GetOrdinal(
                                "CreatedAt"))
                });
        }

        return logs;
    }
}
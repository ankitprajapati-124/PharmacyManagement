using Microsoft.Data.SqlClient;
using PharmacyManagement.Models;
using System.Data;

namespace PharmacyManagement.Repositories;

public class UserRepository : IUserRepository
{
    private readonly string _connectionString;

    public UserRepository(IConfiguration configuration)
    {
        _connectionString =
            configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "DefaultConnection is not configured.");
    }

    public async Task<User?> GetByUsernameAsync(
        string username)
    {
        const string sql = """
            SELECT
                UserId,
                Username,
                PasswordHash,
                FullName,
                Role,
                IsActive,
                CreatedAt
            FROM Users
            WHERE Username = @Username;
            """;

        await using var connection =
            new SqlConnection(_connectionString);

        await connection.OpenAsync();

        await using var command =
            new SqlCommand(sql, connection);

        command.Parameters.Add(
            "@Username",
            SqlDbType.NVarChar,
            100).Value = username.Trim();

        await using var reader =
            await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
            return null;

        return MapUser(reader);
    }

    public async Task<int> AddAsync(User user)
    {
        const string sql = """
            INSERT INTO Users
            (
                Username,
                PasswordHash,
                FullName,
                Role,
                IsActive
            )
            VALUES
            (
                @Username,
                @PasswordHash,
                @FullName,
                @Role,
                @IsActive
            );

            SELECT CAST(SCOPE_IDENTITY() AS INT);
            """;

        await using var connection =
            new SqlConnection(_connectionString);

        await connection.OpenAsync();

        await using var command =
            new SqlCommand(sql, connection);

        command.Parameters.Add(
            "@Username",
            SqlDbType.NVarChar,
            100).Value =
                user.Username.Trim();

        command.Parameters.Add(
            "@PasswordHash",
            SqlDbType.NVarChar,
            500).Value =
                user.PasswordHash;

        command.Parameters.Add(
            "@FullName",
            SqlDbType.NVarChar,
            150).Value =
                user.FullName.Trim();

        command.Parameters.Add(
            "@Role",
            SqlDbType.NVarChar,
            50).Value =
                user.Role.Trim();

        command.Parameters.Add(
            "@IsActive",
            SqlDbType.Bit).Value =
                user.IsActive;

        return Convert.ToInt32(
            await command.ExecuteScalarAsync());
    }

    private static User MapUser(
        SqlDataReader reader)
    {
        return new User
        {
            UserId =
                reader.GetInt32(
                    reader.GetOrdinal("UserId")),

            Username =
                reader.GetString(
                    reader.GetOrdinal("Username")),

            PasswordHash =
                reader.GetString(
                    reader.GetOrdinal("PasswordHash")),

            FullName =
                reader.GetString(
                    reader.GetOrdinal("FullName")),

            Role =
                reader.GetString(
                    reader.GetOrdinal("Role")),

            IsActive =
                reader.GetBoolean(
                    reader.GetOrdinal("IsActive")),

            CreatedAt =
                reader.GetDateTime(
                    reader.GetOrdinal("CreatedAt"))
        };
    }
}
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

    public async Task<IReadOnlyList<User>> GetAllAsync()
    {
        var users = new List<User>();

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
            ORDER BY Username;
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
            users.Add(MapUser(reader));
        }

        return users;
    }

    public async Task<User?> GetByIdAsync(int id)
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
            WHERE UserId = @UserId;
            """;

        await using var connection =
            new SqlConnection(_connectionString);

        await connection.OpenAsync();

        await using var command =
            new SqlCommand(sql, connection);

        command.Parameters.Add(
            "@UserId",
            SqlDbType.Int).Value = id;

        await using var reader =
            await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
            return null;

        return MapUser(reader);
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
            100).Value =
                username.Trim();

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

    public async Task<bool> SetActiveAsync(
        int id,
        bool isActive)
    {
        const string sql = """
        UPDATE Users
        SET IsActive = @IsActive
        WHERE UserId = @UserId;
        """;

        await using var connection =
            new SqlConnection(_connectionString);

        await connection.OpenAsync();

        await using var command =
            new SqlCommand(sql, connection);

        command.Parameters.Add(
            "@UserId",
            SqlDbType.Int).Value = id;

        command.Parameters.Add(
            "@IsActive",
            SqlDbType.Bit).Value = isActive;

        var rowsAffected =
            await command.ExecuteNonQueryAsync();

        return rowsAffected == 1;
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
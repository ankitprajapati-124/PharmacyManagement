using Microsoft.Data.SqlClient;
using PharmacyManagement.Models;
using System.Data;

namespace PharmacyManagement.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly string _connectionString;

    public CategoryRepository(IConfiguration configuration)
    {
        _connectionString =
            configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' not found.");
    }

    public async Task<IEnumerable<Category>> GetAllAsync()
    {
        var categories = new List<Category>();

        const string sql = @"
            SELECT
                CategoryId,
                CategoryName
            FROM dbo.Categories
            ORDER BY CategoryName;";

        await using var connection =
            new SqlConnection(_connectionString);

        await using var command =
            new SqlCommand(sql, connection);

        await connection.OpenAsync();

        await using var reader =
            await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            categories.Add(new Category
            {
                CategoryId = reader.GetInt32(
                    reader.GetOrdinal("CategoryId")),

                CategoryName = reader.GetString(
                    reader.GetOrdinal("CategoryName"))
            });
        }

        return categories;
    }

    public async Task<Category?> GetByIdAsync(int id)
    {
        const string sql = @"
            SELECT
                CategoryId,
                CategoryName
            FROM dbo.Categories
            WHERE CategoryId = @CategoryId;";

        await using var connection =
            new SqlConnection(_connectionString);

        await using var command =
            new SqlCommand(sql, connection);

        command.Parameters.Add("@CategoryId", SqlDbType.Int)
            .Value = id;

        await connection.OpenAsync();

        await using var reader =
            await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
            return null;

        return new Category
        {
            CategoryId = reader.GetInt32(
                reader.GetOrdinal("CategoryId")),

            CategoryName = reader.GetString(
                reader.GetOrdinal("CategoryName"))
        };
    }

    public async Task<int> AddAsync(Category category)
    {
        const string sql = @"
            INSERT INTO dbo.Categories
            (
                CategoryName
            )
            VALUES
            (
                @CategoryName
            );

            SELECT CAST(SCOPE_IDENTITY() AS INT);";

        await using var connection =
            new SqlConnection(_connectionString);

        await using var command =
            new SqlCommand(sql, connection);

        command.Parameters.Add("@CategoryName", SqlDbType.NVarChar, 100)
            .Value = category.CategoryName;

        await connection.OpenAsync();

        return Convert.ToInt32(
            await command.ExecuteScalarAsync());
    }

    public async Task<bool> UpdateAsync(Category category)
    {
        const string sql = @"
            UPDATE dbo.Categories
            SET CategoryName = @CategoryName
            WHERE CategoryId = @CategoryId;";

        await using var connection =
            new SqlConnection(_connectionString);

        await using var command =
            new SqlCommand(sql, connection);

        command.Parameters.Add("@CategoryId", SqlDbType.Int)
            .Value = category.CategoryId;

        command.Parameters.Add("@CategoryName", SqlDbType.NVarChar, 100)
            .Value = category.CategoryName;

        await connection.OpenAsync();

        return await command.ExecuteNonQueryAsync() > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        const string sql = @"
        DELETE FROM dbo.Categories
        WHERE CategoryId = @CategoryId;";

        await using var connection =
            new SqlConnection(_connectionString);

        await using var command =
            new SqlCommand(sql, connection);

        command.Parameters.Add("@CategoryId", SqlDbType.Int)
            .Value = id;

        await connection.OpenAsync();

        try
        {
            return await command.ExecuteNonQueryAsync() > 0;
        }
        catch (SqlException ex) when (ex.Number == 547)
        {
            throw new InvalidOperationException(
                "This category cannot be deleted because it is being used by one or more medicines.",
                ex);
        }
    }
}
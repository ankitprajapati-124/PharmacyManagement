using Microsoft.Data.SqlClient;
using PharmacyManagement.Models;
using System.Data;

namespace PharmacyManagement.Repositories;

public class SupplierRepository : ISupplierRepository
{
    private readonly string _connectionString;

    public SupplierRepository(IConfiguration configuration)
    {
        _connectionString =
            configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' not found.");
    }

    public async Task<IEnumerable<Supplier>> GetAllAsync()
    {
        var suppliers = new List<Supplier>();

        const string sql = @"
            SELECT
                SupplierId,
                SupplierName,
                Phone,
                Email,
                Address,
                IsActive
            FROM dbo.Suppliers
            WHERE IsActive = 1
            ORDER BY SupplierName;";

        await using var connection =
            new SqlConnection(_connectionString);

        await using var command =
            new SqlCommand(sql, connection);

        await connection.OpenAsync();

        await using var reader =
            await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            suppliers.Add(new Supplier
            {
                SupplierId = reader.GetInt32(
                    reader.GetOrdinal("SupplierId")),

                SupplierName = reader.GetString(
                    reader.GetOrdinal("SupplierName")),

                Phone = reader.IsDBNull(
                    reader.GetOrdinal("Phone"))
                    ? null
                    : reader.GetString(
                        reader.GetOrdinal("Phone")),

                Email = reader.IsDBNull(
                    reader.GetOrdinal("Email"))
                    ? null
                    : reader.GetString(
                        reader.GetOrdinal("Email")),

                Address = reader.IsDBNull(
                    reader.GetOrdinal("Address"))
                    ? null
                    : reader.GetString(
                        reader.GetOrdinal("Address")),

                IsActive = reader.GetBoolean(
                    reader.GetOrdinal("IsActive"))
            });
        }

        return suppliers;
    }

    public async Task<Supplier?> GetByIdAsync(int id)
    {
        const string sql = @"
            SELECT
                SupplierId,
                SupplierName,
                Phone,
                Email,
                Address,
                IsActive
            FROM dbo.Suppliers
            WHERE SupplierId = @SupplierId;";

        await using var connection =
            new SqlConnection(_connectionString);

        await using var command =
            new SqlCommand(sql, connection);

        command.Parameters.Add("@SupplierId", SqlDbType.Int)
            .Value = id;

        await connection.OpenAsync();

        await using var reader =
            await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
            return null;

        return new Supplier
        {
            SupplierId = reader.GetInt32(
                reader.GetOrdinal("SupplierId")),

            SupplierName = reader.GetString(
                reader.GetOrdinal("SupplierName")),

            Phone = reader.IsDBNull(
                reader.GetOrdinal("Phone"))
                ? null
                : reader.GetString(
                    reader.GetOrdinal("Phone")),

            Email = reader.IsDBNull(
                reader.GetOrdinal("Email"))
                ? null
                : reader.GetString(
                    reader.GetOrdinal("Email")),

            Address = reader.IsDBNull(
                reader.GetOrdinal("Address"))
                ? null
                : reader.GetString(
                    reader.GetOrdinal("Address")),

            IsActive = reader.GetBoolean(
                reader.GetOrdinal("IsActive"))
        };
    }

    public async Task<int> AddAsync(Supplier supplier)
    {
        const string sql = @"
            INSERT INTO dbo.Suppliers
            (
                SupplierName,
                Phone,
                Email,
                Address,
                IsActive
            )
            VALUES
            (
                @SupplierName,
                @Phone,
                @Email,
                @Address,
                1
            );

            SELECT CAST(SCOPE_IDENTITY() AS INT);";

        await using var connection =
            new SqlConnection(_connectionString);

        await using var command =
            new SqlCommand(sql, connection);

        command.Parameters.Add(
            "@SupplierName",
            SqlDbType.NVarChar,
            150).Value = supplier.SupplierName;

        command.Parameters.Add(
            "@Phone",
            SqlDbType.NVarChar,
            20).Value =
            (object?)supplier.Phone ?? DBNull.Value;

        command.Parameters.Add(
            "@Email",
            SqlDbType.NVarChar,
            150).Value =
            (object?)supplier.Email ?? DBNull.Value;

        command.Parameters.Add(
            "@Address",
            SqlDbType.NVarChar,
            300).Value =
            (object?)supplier.Address ?? DBNull.Value;

        await connection.OpenAsync();

        return Convert.ToInt32(
            await command.ExecuteScalarAsync());
    }

    public async Task<bool> UpdateAsync(Supplier supplier)
    {
        const string sql = @"
            UPDATE dbo.Suppliers
            SET
                SupplierName = @SupplierName,
                Phone = @Phone,
                Email = @Email,
                Address = @Address
            WHERE SupplierId = @SupplierId;";

        await using var connection =
            new SqlConnection(_connectionString);

        await using var command =
            new SqlCommand(sql, connection);

        command.Parameters.Add(
            "@SupplierId",
            SqlDbType.Int).Value =
            supplier.SupplierId;

        command.Parameters.Add(
            "@SupplierName",
            SqlDbType.NVarChar,
            150).Value =
            supplier.SupplierName;

        command.Parameters.Add(
            "@Phone",
            SqlDbType.NVarChar,
            20).Value =
            (object?)supplier.Phone ?? DBNull.Value;

        command.Parameters.Add(
            "@Email",
            SqlDbType.NVarChar,
            150).Value =
            (object?)supplier.Email ?? DBNull.Value;

        command.Parameters.Add(
            "@Address",
            SqlDbType.NVarChar,
            300).Value =
            (object?)supplier.Address ?? DBNull.Value;

        await connection.OpenAsync();

        return await command.ExecuteNonQueryAsync() > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        const string sql = @"
            DELETE FROM dbo.Suppliers
            WHERE SupplierId = @SupplierId;";

        await using var connection =
            new SqlConnection(_connectionString);

        await using var command =
            new SqlCommand(sql, connection);

        command.Parameters.Add("@SupplierId", SqlDbType.Int)
            .Value = id;

        await connection.OpenAsync();

        try
        {
            return await command.ExecuteNonQueryAsync() > 0;
        }
        catch (SqlException ex) when (ex.Number == 547)
        {
            throw new InvalidOperationException(
                "This supplier cannot be deleted because it is being used by one or more medicines.",
                ex);
        }
    }
}
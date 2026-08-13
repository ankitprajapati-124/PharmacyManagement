using Microsoft.Data.SqlClient;
using PharmacyManagement.Models;
using System.Data;

namespace PharmacyManagement.Repositories;

public class MedicineRepository : IMedicineRepository
{
    private readonly string _connectionString;

    public MedicineRepository(IConfiguration configuration)
    {
        _connectionString =
            configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "DefaultConnection is not configured.");
    }

    public async Task<IReadOnlyList<Medicine>> GetAllAsync(string? search = null)
    {
        var medicines = new List<Medicine>();

        const string sql = """
        SELECT
            m.MedicineId,
            m.MedicineName,
            m.Manufacturer,
            m.BatchNo,
            m.ExpiryDate,
            m.PurchasePrice,
            m.SellingPrice,
            m.Quantity,
            m.IsActive,
            m.CreatedAt,
            m.CategoryId,
            m.SupplierId,
            c.CategoryName,
            s.SupplierName
        FROM Medicines AS m
        LEFT JOIN Categories AS c
            ON m.CategoryId = c.CategoryId
        LEFT JOIN Suppliers AS s
            ON m.SupplierId = s.SupplierId
        WHERE m.IsActive = 1
          AND (@Search IS NULL
               OR m.MedicineName LIKE '%' + @Search + '%')
        ORDER BY m.MedicineName;
        """;

        await using var connection =
            new SqlConnection(_connectionString);

        await connection.OpenAsync();

        await using var command =
            new SqlCommand(sql, connection);

        command.Parameters.Add(
            "@Search",
            SqlDbType.NVarChar,
            150).Value =
                string.IsNullOrWhiteSpace(search)
                    ? DBNull.Value
                    : search.Trim();

        await using var reader =
            await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            medicines.Add(MapMedicine(reader));
        }

        return medicines;
    }
    public async Task<Medicine?> GetByIdAsync(int id)
    {
        const string sql = """
        SELECT
            m.MedicineId,
            m.MedicineName,
            m.Manufacturer,
            m.BatchNo,
            m.ExpiryDate,
            m.PurchasePrice,
            m.SellingPrice,
            m.Quantity,
            m.IsActive,
            m.CreatedAt,
            m.CategoryId,
            m.SupplierId,
            c.CategoryName,
            s.SupplierName
        FROM Medicines AS m
        LEFT JOIN Categories AS c
            ON m.CategoryId = c.CategoryId
        LEFT JOIN Suppliers AS s
            ON m.SupplierId = s.SupplierId
        WHERE m.MedicineId = @MedicineId;
        """;

        await using var connection =
            new SqlConnection(_connectionString);

        await connection.OpenAsync();

        await using var command =
            new SqlCommand(sql, connection);

        command.Parameters.Add(
            "@MedicineId",
            SqlDbType.Int).Value = id;

        await using var reader =
            await command.ExecuteReaderAsync();

        return await reader.ReadAsync()
            ? MapMedicine(reader)
            : null;
    }

    public async Task<int> AddAsync(Medicine medicine)
    {
        const string sql = """
        INSERT INTO Medicines
        (
            MedicineName,
            Manufacturer,
            BatchNo,
            ExpiryDate,
            PurchasePrice,
            SellingPrice,
            Quantity,
            IsActive,
            CategoryId,
            SupplierId
        )
        VALUES
        (
            @MedicineName,
            @Manufacturer,
            @BatchNo,
            @ExpiryDate,
            @PurchasePrice,
            @SellingPrice,
            @Quantity,
            1,
            @CategoryId,
            @SupplierId
        );

        SELECT CAST(SCOPE_IDENTITY() AS INT);
        """;

        await using var connection =
            new SqlConnection(_connectionString);

        await connection.OpenAsync();

        await using var command =
            new SqlCommand(sql, connection);

        AddMedicineParameters(command, medicine);

        return Convert.ToInt32(
            await command.ExecuteScalarAsync());
    }

    public async Task<bool> UpdateAsync(Medicine medicine)
    {
        const string sql = """
        UPDATE Medicines
        SET
            MedicineName = @MedicineName,
            Manufacturer = @Manufacturer,
            BatchNo = @BatchNo,
            ExpiryDate = @ExpiryDate,
            PurchasePrice = @PurchasePrice,
            SellingPrice = @SellingPrice,
            Quantity = @Quantity,
            CategoryId = @CategoryId,
            SupplierId = @SupplierId
        WHERE MedicineId = @MedicineId;
        """;

        await using var connection =
            new SqlConnection(_connectionString);

        await connection.OpenAsync();

        await using var command =
            new SqlCommand(sql, connection);

        command.Parameters.Add("@MedicineId", SqlDbType.Int)
            .Value = medicine.MedicineId;

        AddMedicineParameters(command, medicine);

        return await command.ExecuteNonQueryAsync() > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        // Soft delete: keeps historical data intact.
        const string sql = """
            UPDATE Medicines
            SET IsActive = 0
            WHERE MedicineId = @MedicineId;
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@MedicineId", SqlDbType.Int).Value = id;

        return await command.ExecuteNonQueryAsync() > 0;
    }

    private static void AddMedicineParameters(
        SqlCommand command,
        Medicine medicine)
    {
        command.Parameters.Add(
            "@MedicineName",
            SqlDbType.NVarChar,
            150).Value = medicine.MedicineName.Trim();

        command.Parameters.Add(
            "@Manufacturer",
            SqlDbType.NVarChar,
            150).Value =
                (object?)medicine.Manufacturer?.Trim() ?? DBNull.Value;

        command.Parameters.Add(
            "@BatchNo",
            SqlDbType.NVarChar,
            50).Value =
                (object?)medicine.BatchNo?.Trim() ?? DBNull.Value;

        command.Parameters.Add(
            "@ExpiryDate",
            SqlDbType.Date).Value =
                (object?)medicine.ExpiryDate ?? DBNull.Value;

        var purchasePrice = command.Parameters.Add(
            "@PurchasePrice",
            SqlDbType.Decimal);
        purchasePrice.Precision = 12;
        purchasePrice.Scale = 2;
        purchasePrice.Value = medicine.PurchasePrice;

        var sellingPrice = command.Parameters.Add(
            "@SellingPrice",
            SqlDbType.Decimal);
        sellingPrice.Precision = 12;
        sellingPrice.Scale = 2;
        sellingPrice.Value = medicine.SellingPrice;

        command.Parameters.Add(
            "@Quantity",
            SqlDbType.Int).Value = medicine.Quantity;

        command.Parameters.Add(
            "@CategoryId",
            SqlDbType.Int)
            .Value = (object?)medicine.CategoryId ?? DBNull.Value;

        command.Parameters.Add(
            "@SupplierId",
            SqlDbType.Int)
            .Value = (object?)medicine.SupplierId ?? DBNull.Value;
    }

    private static Medicine MapMedicine(SqlDataReader reader)
    {
        return new Medicine
        {
            MedicineId = reader.GetInt32(
                reader.GetOrdinal("MedicineId")),

            MedicineName = reader.GetString(
                reader.GetOrdinal("MedicineName")),

            Manufacturer = reader.IsDBNull(
                reader.GetOrdinal("Manufacturer"))
                ? null
                : reader.GetString(
                    reader.GetOrdinal("Manufacturer")),

            BatchNo = reader.IsDBNull(
                reader.GetOrdinal("BatchNo"))
                ? null
                : reader.GetString(
                    reader.GetOrdinal("BatchNo")),

            ExpiryDate = reader.IsDBNull(
                reader.GetOrdinal("ExpiryDate"))
                ? null
                : reader.GetDateTime(
                    reader.GetOrdinal("ExpiryDate")),

            PurchasePrice = reader.GetDecimal(
                reader.GetOrdinal("PurchasePrice")),

            SellingPrice = reader.GetDecimal(
                reader.GetOrdinal("SellingPrice")),

            Quantity = reader.GetInt32(
                reader.GetOrdinal("Quantity")),

            IsActive = reader.GetBoolean(
                reader.GetOrdinal("IsActive")),

            CreatedAt = reader.GetDateTime(
                reader.GetOrdinal("CreatedAt")),

            CategoryId = reader.IsDBNull(
                reader.GetOrdinal("CategoryId"))
                ? null
                : reader.GetInt32(
                    reader.GetOrdinal("CategoryId")),

            SupplierId = reader.IsDBNull(
                reader.GetOrdinal("SupplierId"))
                ? null
                : reader.GetInt32(
                    reader.GetOrdinal("SupplierId")),

            CategoryName = reader.IsDBNull(
                reader.GetOrdinal("CategoryName"))
                ? null
                : reader.GetString(
                    reader.GetOrdinal("CategoryName")),

            SupplierName = reader.IsDBNull(
                reader.GetOrdinal("SupplierName"))
                ? null
                : reader.GetString(
                    reader.GetOrdinal("SupplierName"))
        };
    }
}
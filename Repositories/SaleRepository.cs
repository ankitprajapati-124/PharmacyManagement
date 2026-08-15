using Microsoft.Data.SqlClient;
using PharmacyManagement.Models;
using System.Data;

namespace PharmacyManagement.Repositories;

public class SaleRepository : ISaleRepository
{
    private readonly string _connectionString;

    public SaleRepository(IConfiguration configuration)
    {
        _connectionString =
            configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "DefaultConnection is not configured.");
    }

    // =========================================================
    // GET ALL
    // Admin = all sales
    // Other users = only their own sales
    // =========================================================

    public async Task<IReadOnlyList<Sale>> GetAllAsync(
        int currentUserId,
        bool isAdmin)
    {
        var sales = new List<Sale>();

        const string sql = """
            SELECT
                SaleId,
                UserId,
                CustomerName,
                CustomerMobile,
                SaleDate,
                InvoiceNo,
                Discount,
                TotalAmount,
                CreatedAt
            FROM Sales
            WHERE @IsAdmin = 1
               OR UserId = @UserId
            ORDER BY SaleId DESC;
            """;

        await using var connection =
            new SqlConnection(_connectionString);

        await connection.OpenAsync();

        await using var command =
            new SqlCommand(sql, connection);

        command.Parameters.Add(
            "@UserId",
            SqlDbType.Int).Value = currentUserId;

        command.Parameters.Add(
            "@IsAdmin",
            SqlDbType.Bit).Value = isAdmin;

        await using var reader =
            await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            sales.Add(MapSale(reader));
        }

        return sales;
    }

    // =========================================================
    // GET BY ID
    // Prevent User A from opening User B's sale
    // =========================================================

    public async Task<Sale?> GetByIdAsync(
        int id,
        int currentUserId,
        bool isAdmin)
    {
        const string saleSql = """
            SELECT
                SaleId,
                UserId,
                CustomerName,
                CustomerMobile,
                SaleDate,
                InvoiceNo,
                Discount,
                TotalAmount,
                CreatedAt
            FROM Sales
            WHERE SaleId = @SaleId
              AND (
                    @IsAdmin = 1
                    OR UserId = @UserId
                  );
            """;

        const string itemsSql = """
            SELECT
                si.SaleItemId,
                si.SaleId,
                si.MedicineId,
                m.MedicineName,
                m.Quantity AS AvailableStock,
                si.Quantity,
                si.SellingPrice
            FROM SaleItems si
            INNER JOIN Medicines m
                ON si.MedicineId = m.MedicineId
            WHERE si.SaleId = @SaleId
            ORDER BY si.SaleItemId;
            """;

        await using var connection =
            new SqlConnection(_connectionString);

        await connection.OpenAsync();

        Sale? sale;

        // =====================================================
        // Sale
        // =====================================================

        await using (var command =
            new SqlCommand(saleSql, connection))
        {
            command.Parameters.Add(
                "@SaleId",
                SqlDbType.Int).Value = id;

            command.Parameters.Add(
                "@UserId",
                SqlDbType.Int).Value = currentUserId;

            command.Parameters.Add(
                "@IsAdmin",
                SqlDbType.Bit).Value = isAdmin;

            await using var reader =
                await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
                return null;

            sale = MapSale(reader);
        }

        // =====================================================
        // Sale Items
        // =====================================================

        await using (var command =
            new SqlCommand(itemsSql, connection))
        {
            command.Parameters.Add(
                "@SaleId",
                SqlDbType.Int).Value = id;

            await using var reader =
                await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                sale.Items.Add(
                    MapSaleItem(reader));
            }
        }

        return sale;
    }

    // =========================================================
    // CREATE SALE
    // UserId comes from authenticated user
    // NOT from browser/form
    // =========================================================
    private static async Task<string> GenerateSaleInvoiceAsync(
    SqlConnection connection,
    SqlTransaction transaction,
    DateTime saleDate)
    {
        const string sql = """
        SELECT ISNULL(
            MAX(
                TRY_CONVERT(
                    INT,
                    RIGHT(InvoiceNo, 3)
                )
            ),
            0
        ) + 1
        FROM Sales WITH (UPDLOCK, HOLDLOCK)
        WHERE InvoiceNo LIKE @Pattern;
        """;

        await using var command =
            new SqlCommand(
                sql,
                connection,
                transaction);

        command.Parameters.Add(
            "@Pattern",
            SqlDbType.NVarChar,
            50).Value =
            $"SAL-{saleDate:yyyyMMdd}-%";

        var result =
            await command.ExecuteScalarAsync();

        var sequence =
            Convert.ToInt32(result);

        return $"SAL-{saleDate:yyyyMMdd}-{sequence:000}";
    }
    public async Task<int> AddAsync(
        Sale sale,
        int currentUserId)
    {
        const string saleSql = """
            INSERT INTO Sales
            (
                UserId,
                CustomerName,
                CustomerMobile,
                SaleDate,
                InvoiceNo,
                Discount,
                TotalAmount
            )
            VALUES
            (
                @UserId,
                @CustomerName,
                @CustomerMobile,
                @SaleDate,
                @InvoiceNo,
                @Discount,
                @TotalAmount
            );

            SELECT CAST(SCOPE_IDENTITY() AS INT);
            """;

        const string stockCheckSql = """
            SELECT Quantity
            FROM Medicines WITH (UPDLOCK, ROWLOCK)
            WHERE MedicineId = @MedicineId;
            """;

        const string itemSql = """
            INSERT INTO SaleItems
            (
                SaleId,
                MedicineId,
                Quantity,
                SellingPrice
            )
            VALUES
            (
                @SaleId,
                @MedicineId,
                @Quantity,
                @SellingPrice
            );
            """;

        const string stockUpdateSql = """
            UPDATE Medicines
            SET Quantity = Quantity - @Quantity
            WHERE MedicineId = @MedicineId
              AND Quantity >= @Quantity;
            """;

        await using var connection =
            new SqlConnection(_connectionString);

        await connection.OpenAsync();

        await using var transaction =
            await connection.BeginTransactionAsync();
        var invoiceNo =
    await GenerateSaleInvoiceAsync(
        connection,
        (SqlTransaction)transaction,
        sale.SaleDate);
        try
        {
            int saleId;

            // =================================================
            // 1. CREATE SALE
            // =================================================

            await using (var command =
                new SqlCommand(
                    saleSql,
                    connection,
                    (SqlTransaction)transaction))
            {
                command.Parameters.Add(
                    "@UserId",
                    SqlDbType.Int).Value =
                    currentUserId;

                command.Parameters.Add(
                    "@CustomerName",
                    SqlDbType.NVarChar,
                    150).Value =
                    (object?)sale.CustomerName?.Trim()
                    ?? DBNull.Value;

                command.Parameters.Add(
                    "@CustomerMobile",
                    SqlDbType.NVarChar,
                    20).Value =
                    (object?)sale.CustomerMobile?.Trim()
                    ?? DBNull.Value;

                command.Parameters.Add(
                    "@SaleDate",
                    SqlDbType.DateTime2).Value =
                    sale.SaleDate;

                command.Parameters.Add(
                    "@InvoiceNo",
                    SqlDbType.NVarChar,
                    50).Value =
                    invoiceNo;

                var discount =
                    command.Parameters.Add(
                        "@Discount",
                        SqlDbType.Decimal);

                discount.Precision = 12;
                discount.Scale = 2;
                discount.Value = sale.Discount;

                var totalAmount =
                    command.Parameters.Add(
                        "@TotalAmount",
                        SqlDbType.Decimal);

                totalAmount.Precision = 12;
                totalAmount.Scale = 2;
                totalAmount.Value = sale.TotalAmount;

                saleId =
                    Convert.ToInt32(
                        await command.ExecuteScalarAsync());
            }

            // =================================================
            // 2. PROCESS SALE ITEMS
            // =================================================

            foreach (var item in sale.Items)
            {
                if (item.Quantity <= 0)
                {
                    throw new InvalidOperationException(
                        "Sale quantity must be greater than zero.");
                }

                if (item.SellingPrice < 0)
                {
                    throw new InvalidOperationException(
                        "Selling price cannot be negative.");
                }

                // =============================================
                // Check stock
                // =============================================

                int availableStock;

                await using (var command =
                    new SqlCommand(
                        stockCheckSql,
                        connection,
                        (SqlTransaction)transaction))
                {
                    command.Parameters.Add(
                        "@MedicineId",
                        SqlDbType.Int).Value =
                        item.MedicineId;

                    var result =
                        await command.ExecuteScalarAsync();

                    if (result is null)
                    {
                        throw new InvalidOperationException(
                            $"Medicine ID {item.MedicineId} was not found.");
                    }

                    availableStock =
                        Convert.ToInt32(result);
                }

                if (availableStock < item.Quantity)
                {
                    throw new InvalidOperationException(
                        $"Insufficient stock for medicine ID {item.MedicineId}. " +
                        $"Available stock: {availableStock}.");
                }

                // =============================================
                // Insert sale item
                // =============================================

                await using (var command =
                    new SqlCommand(
                        itemSql,
                        connection,
                        (SqlTransaction)transaction))
                {
                    command.Parameters.Add(
                        "@SaleId",
                        SqlDbType.Int).Value =
                        saleId;

                    command.Parameters.Add(
                        "@MedicineId",
                        SqlDbType.Int).Value =
                        item.MedicineId;

                    command.Parameters.Add(
                        "@Quantity",
                        SqlDbType.Int).Value =
                        item.Quantity;

                    var price =
                        command.Parameters.Add(
                            "@SellingPrice",
                            SqlDbType.Decimal);

                    price.Precision = 12;
                    price.Scale = 2;
                    price.Value = item.SellingPrice;

                    await command.ExecuteNonQueryAsync();
                }

                // =============================================
                // Decrease stock
                // =============================================

                await using (var command =
                    new SqlCommand(
                        stockUpdateSql,
                        connection,
                        (SqlTransaction)transaction))
                {
                    command.Parameters.Add(
                        "@MedicineId",
                        SqlDbType.Int).Value =
                        item.MedicineId;

                    command.Parameters.Add(
                        "@Quantity",
                        SqlDbType.Int).Value =
                        item.Quantity;

                    var affectedRows =
                        await command.ExecuteNonQueryAsync();

                    if (affectedRows == 0)
                    {
                        throw new InvalidOperationException(
                            $"Unable to update stock for medicine ID {item.MedicineId}.");
                    }
                }
            }

            await transaction.CommitAsync();

            return saleId;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    // =========================================================
    // DELETE SALE
    //
    // Admin = can delete any sale
    // User  = can delete only own sale
    // =========================================================

    public async Task<bool> DeleteAsync(
        int id,
        int currentUserId,
        bool isAdmin)
    {
        const string getItemsSql = """
            SELECT
                si.MedicineId,
                si.Quantity
            FROM SaleItems si
            INNER JOIN Sales s
                ON si.SaleId = s.SaleId
            WHERE si.SaleId = @SaleId
              AND (
                    @IsAdmin = 1
                    OR s.UserId = @UserId
                  );
            """;

        const string restoreStockSql = """
            UPDATE Medicines
            SET Quantity = Quantity + @Quantity
            WHERE MedicineId = @MedicineId;
            """;

        const string deleteItemsSql = """
            DELETE FROM SaleItems
            WHERE SaleId = @SaleId;
            """;

        const string deleteSaleSql = """
            DELETE FROM Sales
            WHERE SaleId = @SaleId
              AND (
                    @IsAdmin = 1
                    OR UserId = @UserId
                  );
            """;

        await using var connection =
            new SqlConnection(_connectionString);

        await connection.OpenAsync();

        await using var transaction =
            await connection.BeginTransactionAsync();

        try
        {
            var items =
                new List<(int MedicineId, int Quantity)>();

            // =================================================
            // Get sale items only if user owns sale
            // =================================================

            await using (var command =
                new SqlCommand(
                    getItemsSql,
                    connection,
                    (SqlTransaction)transaction))
            {
                command.Parameters.Add(
                    "@SaleId",
                    SqlDbType.Int).Value = id;

                command.Parameters.Add(
                    "@UserId",
                    SqlDbType.Int).Value =
                    currentUserId;

                command.Parameters.Add(
                    "@IsAdmin",
                    SqlDbType.Bit).Value =
                    isAdmin;

                await using var reader =
                    await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    items.Add(
                        (
                            reader.GetInt32(
                                reader.GetOrdinal("MedicineId")),

                            reader.GetInt32(
                                reader.GetOrdinal("Quantity"))
                        ));
                }
            }

            if (items.Count == 0)
            {
                await transaction.RollbackAsync();
                return false;
            }

            // =================================================
            // Restore stock
            // =================================================

            foreach (var item in items)
            {
                await using var command =
                    new SqlCommand(
                        restoreStockSql,
                        connection,
                        (SqlTransaction)transaction);

                command.Parameters.Add(
                    "@MedicineId",
                    SqlDbType.Int).Value =
                    item.MedicineId;

                command.Parameters.Add(
                    "@Quantity",
                    SqlDbType.Int).Value =
                    item.Quantity;

                await command.ExecuteNonQueryAsync();
            }

            // =================================================
            // Delete sale items
            // =================================================

            await using (var command =
                new SqlCommand(
                    deleteItemsSql,
                    connection,
                    (SqlTransaction)transaction))
            {
                command.Parameters.Add(
                    "@SaleId",
                    SqlDbType.Int).Value = id;

                await command.ExecuteNonQueryAsync();
            }

            // =================================================
            // Delete sale
            // =================================================

            await using (var command =
                new SqlCommand(
                    deleteSaleSql,
                    connection,
                    (SqlTransaction)transaction))
            {
                command.Parameters.Add(
                    "@SaleId",
                    SqlDbType.Int).Value = id;

                command.Parameters.Add(
                    "@UserId",
                    SqlDbType.Int).Value =
                    currentUserId;

                command.Parameters.Add(
                    "@IsAdmin",
                    SqlDbType.Bit).Value =
                    isAdmin;

                var affectedRows =
                    await command.ExecuteNonQueryAsync();

                if (affectedRows == 0)
                {
                    await transaction.RollbackAsync();
                    return false;
                }
            }

            await transaction.CommitAsync();

            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    // =========================================================
    // MAP SALE
    // =========================================================

    private static Sale MapSale(
        SqlDataReader reader)
    {
        return new Sale
        {
            SaleId =
                reader.GetInt32(
                    reader.GetOrdinal("SaleId")),

            UserId =
                reader.GetInt32(
                    reader.GetOrdinal("UserId")),

            CustomerName =
                reader.IsDBNull(
                    reader.GetOrdinal("CustomerName"))
                    ? null
                    : reader.GetString(
                        reader.GetOrdinal("CustomerName")),

            CustomerMobile =
                reader.IsDBNull(
                    reader.GetOrdinal("CustomerMobile"))
                    ? null
                    : reader.GetString(
                        reader.GetOrdinal("CustomerMobile")),

            SaleDate =
                reader.GetDateTime(
                    reader.GetOrdinal("SaleDate")),

            InvoiceNo =
                reader.IsDBNull(
                    reader.GetOrdinal("InvoiceNo"))
                    ? null
                    : reader.GetString(
                        reader.GetOrdinal("InvoiceNo")),

            Discount =
                reader.GetDecimal(
                    reader.GetOrdinal("Discount")),

            TotalAmount =
                reader.GetDecimal(
                    reader.GetOrdinal("TotalAmount")),

            CreatedAt =
                reader.GetDateTime(
                    reader.GetOrdinal("CreatedAt"))
        };
    }

    // =========================================================
    // MAP SALE ITEM
    // =========================================================

    private static SaleItem MapSaleItem(
        SqlDataReader reader)
    {
        return new SaleItem
        {
            SaleItemId =
                reader.GetInt32(
                    reader.GetOrdinal("SaleItemId")),

            SaleId =
                reader.GetInt32(
                    reader.GetOrdinal("SaleId")),

            MedicineId =
                reader.GetInt32(
                    reader.GetOrdinal("MedicineId")),

            MedicineName =
                reader.GetString(
                    reader.GetOrdinal("MedicineName")),

            AvailableStock =
                reader.GetInt32(
                    reader.GetOrdinal("AvailableStock")),

            Quantity =
                reader.GetInt32(
                    reader.GetOrdinal("Quantity")),

            SellingPrice =
                reader.GetDecimal(
                    reader.GetOrdinal("SellingPrice"))
        };
    }
}
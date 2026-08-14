using Microsoft.Data.SqlClient;
using PharmacyManagement.Models;
using System.Data;

namespace PharmacyManagement.Repositories;

public class PurchaseRepository : IPurchaseRepository
{
    private readonly string _connectionString;

    public PurchaseRepository(IConfiguration configuration)
    {
        _connectionString =
            configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "DefaultConnection is not configured.");
    }

    // =========================================================
    // GET ALL
    // Admin     -> all purchases
    // User      -> own purchases
    // =========================================================

    public async Task<IReadOnlyList<Purchase>> GetAllAsync(
        int currentUserId,
        bool isAdmin)
    {
        var purchases = new List<Purchase>();

        const string sql = """
            SELECT
                p.PurchaseId,
                p.UserId,
                p.SupplierId,
                s.SupplierName,
                p.PurchaseDate,
                p.InvoiceNo,
                p.TotalAmount,
                p.CreatedAt
            FROM Purchases p
            INNER JOIN Suppliers s
                ON p.SupplierId = s.SupplierId
            WHERE @IsAdmin = 1
               OR p.UserId = @UserId
            ORDER BY p.PurchaseId DESC;
            """;

        await using var connection =
            new SqlConnection(_connectionString);

        await connection.OpenAsync();

        await using var command =
            new SqlCommand(sql, connection);

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
            purchases.Add(MapPurchase(reader));
        }

        return purchases;
    }

    // =========================================================
    // GET BY ID
    // Prevent one user from opening another user's purchase
    // =========================================================

    public async Task<Purchase?> GetByIdAsync(
        int id,
        int currentUserId,
        bool isAdmin)
    {
        const string purchaseSql = """
            SELECT
                p.PurchaseId,
                p.UserId,
                p.SupplierId,
                s.SupplierName,
                p.PurchaseDate,
                p.InvoiceNo,
                p.TotalAmount,
                p.CreatedAt
            FROM Purchases p
            INNER JOIN Suppliers s
                ON p.SupplierId = s.SupplierId
            WHERE p.PurchaseId = @PurchaseId
              AND (
                    @IsAdmin = 1
                    OR p.UserId = @UserId
                  );
            """;

        const string itemsSql = """
            SELECT
                pi.PurchaseItemId,
                pi.PurchaseId,
                pi.MedicineId,
                m.MedicineName,
                pi.Quantity,
                pi.PurchasePrice
            FROM PurchaseItems pi
            INNER JOIN Medicines m
                ON pi.MedicineId = m.MedicineId
            WHERE pi.PurchaseId = @PurchaseId
            ORDER BY pi.PurchaseItemId;
            """;

        await using var connection =
            new SqlConnection(_connectionString);

        await connection.OpenAsync();

        Purchase? purchase;

        // =====================================================
        // Purchase
        // =====================================================

        await using (var command =
            new SqlCommand(
                purchaseSql,
                connection))
        {
            command.Parameters.Add(
                "@PurchaseId",
                SqlDbType.Int).Value =
                id;

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

            if (!await reader.ReadAsync())
                return null;

            purchase = MapPurchase(reader);
        }

        // =====================================================
        // Purchase Items
        // =====================================================

        await using (var command =
            new SqlCommand(
                itemsSql,
                connection))
        {
            command.Parameters.Add(
                "@PurchaseId",
                SqlDbType.Int).Value =
                id;

            await using var reader =
                await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                purchase.Items.Add(
                    MapPurchaseItem(reader));
            }
        }

        return purchase;
    }

    // =========================================================
    // CREATE PURCHASE
    // UserId comes from authenticated user
    // =========================================================

    public async Task<int> AddAsync(
        Purchase purchase,
        int currentUserId)
    {
        const string purchaseSql = """
            INSERT INTO Purchases
            (
                UserId,
                SupplierId,
                PurchaseDate,
                InvoiceNo,
                TotalAmount
            )
            VALUES
            (
                @UserId,
                @SupplierId,
                @PurchaseDate,
                @InvoiceNo,
                @TotalAmount
            );

            SELECT CAST(SCOPE_IDENTITY() AS INT);
            """;

        const string itemSql = """
            INSERT INTO PurchaseItems
            (
                PurchaseId,
                MedicineId,
                Quantity,
                PurchasePrice
            )
            VALUES
            (
                @PurchaseId,
                @MedicineId,
                @Quantity,
                @PurchasePrice
            );
            """;

        const string stockSql = """
            UPDATE Medicines
            SET Quantity = Quantity + @Quantity
            WHERE MedicineId = @MedicineId;
            """;

        await using var connection =
            new SqlConnection(_connectionString);

        await connection.OpenAsync();

        await using var transaction =
            await connection.BeginTransactionAsync();

        try
        {
            int purchaseId;

            // =================================================
            // 1. CREATE PURCHASE
            // =================================================

            await using (var command =
                new SqlCommand(
                    purchaseSql,
                    connection,
                    (SqlTransaction)transaction))
            {
                command.Parameters.Add(
                    "@UserId",
                    SqlDbType.Int).Value =
                    currentUserId;

                command.Parameters.Add(
                    "@SupplierId",
                    SqlDbType.Int).Value =
                    purchase.SupplierId;

                command.Parameters.Add(
                    "@PurchaseDate",
                    SqlDbType.Date).Value =
                    purchase.PurchaseDate.Date;

                command.Parameters.Add(
                    "@InvoiceNo",
                    SqlDbType.NVarChar,
                    50).Value =
                    (object?)purchase.InvoiceNo?.Trim()
                    ?? DBNull.Value;

                var totalAmount =
                    command.Parameters.Add(
                        "@TotalAmount",
                        SqlDbType.Decimal);

                totalAmount.Precision = 12;
                totalAmount.Scale = 2;
                totalAmount.Value =
                    purchase.TotalAmount;

                purchaseId =
                    Convert.ToInt32(
                        await command.ExecuteScalarAsync());
            }

            // =================================================
            // 2. ADD PURCHASE ITEMS
            // =================================================

            foreach (var item in purchase.Items)
            {
                if (item.Quantity <= 0)
                {
                    throw new InvalidOperationException(
                        "Purchase quantity must be greater than zero.");
                }

                if (item.PurchasePrice < 0)
                {
                    throw new InvalidOperationException(
                        "Purchase price cannot be negative.");
                }

                // =============================================
                // Insert purchase item
                // =============================================

                await using (var command =
                    new SqlCommand(
                        itemSql,
                        connection,
                        (SqlTransaction)transaction))
                {
                    command.Parameters.Add(
                        "@PurchaseId",
                        SqlDbType.Int).Value =
                        purchaseId;

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
                            "@PurchasePrice",
                            SqlDbType.Decimal);

                    price.Precision = 12;
                    price.Scale = 2;
                    price.Value =
                        item.PurchasePrice;

                    await command.ExecuteNonQueryAsync();
                }

                // =============================================
                // Increase medicine stock
                // =============================================

                await using (var command =
                    new SqlCommand(
                        stockSql,
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
                            $"Medicine ID {item.MedicineId} was not found.");
                    }
                }
            }

            await transaction.CommitAsync();

            return purchaseId;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    // =========================================================
    // DELETE PURCHASE
    //
    // Admin     -> any purchase
    // User      -> only own purchase
    //
    // Stock is reversed exactly as before.
    // =========================================================

    public async Task<bool> DeleteAsync(
        int id,
        int currentUserId,
        bool isAdmin)
    {
        const string getItemsSql = """
            SELECT
                pi.MedicineId,
                pi.Quantity
            FROM PurchaseItems pi
            INNER JOIN Purchases p
                ON pi.PurchaseId = p.PurchaseId
            WHERE pi.PurchaseId = @PurchaseId
              AND (
                    @IsAdmin = 1
                    OR p.UserId = @UserId
                  );
            """;

        const string decreaseStockSql = """
            UPDATE Medicines
            SET Quantity = Quantity - @Quantity
            WHERE MedicineId = @MedicineId
              AND Quantity >= @Quantity;
            """;

        const string deleteItemsSql = """
            DELETE FROM PurchaseItems
            WHERE PurchaseId = @PurchaseId;
            """;

        const string deletePurchaseSql = """
            DELETE FROM Purchases
            WHERE PurchaseId = @PurchaseId
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
            // Get purchase items only if user has access
            // =================================================

            await using (var command =
                new SqlCommand(
                    getItemsSql,
                    connection,
                    (SqlTransaction)transaction))
            {
                command.Parameters.Add(
                    "@PurchaseId",
                    SqlDbType.Int).Value =
                    id;

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
            // Reverse stock
            // =================================================

            foreach (var item in items)
            {
                await using var command =
                    new SqlCommand(
                        decreaseStockSql,
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

                var affectedRows =
                    await command.ExecuteNonQueryAsync();

                if (affectedRows == 0)
                {
                    throw new InvalidOperationException(
                        "Purchase cannot be deleted because the current medicine stock is insufficient to reverse this purchase.");
                }
            }

            // =================================================
            // Delete purchase items
            // =================================================

            await using (var command =
                new SqlCommand(
                    deleteItemsSql,
                    connection,
                    (SqlTransaction)transaction))
            {
                command.Parameters.Add(
                    "@PurchaseId",
                    SqlDbType.Int).Value =
                    id;

                await command.ExecuteNonQueryAsync();
            }

            // =================================================
            // Delete purchase
            // =================================================

            await using (var command =
                new SqlCommand(
                    deletePurchaseSql,
                    connection,
                    (SqlTransaction)transaction))
            {
                command.Parameters.Add(
                    "@PurchaseId",
                    SqlDbType.Int).Value =
                    id;

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
    // MAP PURCHASE
    // =========================================================

    private static Purchase MapPurchase(
        SqlDataReader reader)
    {
        return new Purchase
        {
            PurchaseId =
                reader.GetInt32(
                    reader.GetOrdinal("PurchaseId")),

            UserId =
                reader.GetInt32(
                    reader.GetOrdinal("UserId")),

            SupplierId =
                reader.GetInt32(
                    reader.GetOrdinal("SupplierId")),

            SupplierName =
                reader.GetString(
                    reader.GetOrdinal("SupplierName")),

            PurchaseDate =
                reader.GetDateTime(
                    reader.GetOrdinal("PurchaseDate")),

            InvoiceNo =
                reader.IsDBNull(
                    reader.GetOrdinal("InvoiceNo"))
                    ? null
                    : reader.GetString(
                        reader.GetOrdinal("InvoiceNo")),

            TotalAmount =
                reader.GetDecimal(
                    reader.GetOrdinal("TotalAmount")),

            CreatedAt =
                reader.GetDateTime(
                    reader.GetOrdinal("CreatedAt"))
        };
    }

    // =========================================================
    // MAP PURCHASE ITEM
    // =========================================================

    private static PurchaseItem MapPurchaseItem(
        SqlDataReader reader)
    {
        return new PurchaseItem
        {
            PurchaseItemId =
                reader.GetInt32(
                    reader.GetOrdinal("PurchaseItemId")),

            PurchaseId =
                reader.GetInt32(
                    reader.GetOrdinal("PurchaseId")),

            MedicineId =
                reader.GetInt32(
                    reader.GetOrdinal("MedicineId")),

            MedicineName =
                reader.GetString(
                    reader.GetOrdinal("MedicineName")),

            Quantity =
                reader.GetInt32(
                    reader.GetOrdinal("Quantity")),

            PurchasePrice =
                reader.GetDecimal(
                    reader.GetOrdinal("PurchasePrice"))
        };
    }
}
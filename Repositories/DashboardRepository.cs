using Microsoft.Data.SqlClient;
using PharmacyManagement.Models;
using System.Data;

namespace PharmacyManagement.Repositories;

public class DashboardRepository : IDashboardRepository
{
    private readonly string _connectionString;

    public DashboardRepository(
        IConfiguration configuration)
    {
        _connectionString =
            configuration.GetConnectionString(
                "DefaultConnection")
            ?? throw new InvalidOperationException(
                "DefaultConnection is not configured.");
    }

    public async Task<DashboardViewModel> GetDashboardAsync(
        int currentUserId,
        bool isAdmin,
        bool canViewPurchases)
    {
        const string sql = """
            /* =====================================================
               1. DASHBOARD STATISTICS
               ===================================================== */

            SELECT
                (
                    SELECT COUNT(*)
                    FROM Medicines
                    WHERE IsActive = 1
                ) AS TotalMedicines,

                (
                    SELECT ISNULL(SUM(Quantity), 0)
                    FROM Medicines
                    WHERE IsActive = 1
                ) AS TotalStock,

                (
                    SELECT COUNT(*)
                    FROM Sales
                    WHERE SaleDate >= CAST(GETDATE() AS DATE)
                      AND SaleDate < DATEADD(
                            DAY,
                            1,
                            CAST(GETDATE() AS DATE)
                      )
                      AND (
                            @IsAdmin = 1
                            OR UserId = @UserId
                          )
                ) AS TodaySalesCount,

                (
                    SELECT ISNULL(SUM(TotalAmount), 0)
                    FROM Sales
                    WHERE SaleDate >= CAST(GETDATE() AS DATE)
                      AND SaleDate < DATEADD(
                            DAY,
                            1,
                            CAST(GETDATE() AS DATE)
                      )
                      AND (
                            @IsAdmin = 1
                            OR UserId = @UserId
                          )
                ) AS TodaySalesAmount,

                (
                    SELECT
                        CASE
                            WHEN @CanViewPurchases = 0
                                THEN 0
                            ELSE COUNT(*)
                        END
                    FROM Purchases
                    WHERE
                        @CanViewPurchases = 1
                        AND (
                            @IsAdmin = 1
                            OR UserId = @UserId
                        )
                ) AS TotalPurchases,

                (
                    SELECT
                        CASE
                            WHEN @CanViewPurchases = 0
                                THEN CAST(0 AS DECIMAL(18,2))
                            ELSE ISNULL(
                                SUM(TotalAmount),
                                0
                            )
                        END
                    FROM Purchases
                    WHERE
                        @CanViewPurchases = 1
                        AND (
                            @IsAdmin = 1
                            OR UserId = @UserId
                        )
                ) AS TotalPurchaseAmount;


            /* =====================================================
               2. LOW STOCK MEDICINES
               Shared pharmacy inventory
               ===================================================== */

            SELECT
                MedicineId,
                MedicineName,
                BatchNo,
                ExpiryDate,
                Quantity
            FROM Medicines
            WHERE IsActive = 1
              AND Quantity <= 10
            ORDER BY
                Quantity ASC,
                MedicineName;


            /* =====================================================
               3. EXPIRING WITHIN 30 DAYS
               Shared pharmacy inventory
               ===================================================== */

            SELECT
                MedicineId,
                MedicineName,
                BatchNo,
                ExpiryDate,
                Quantity
            FROM Medicines
            WHERE IsActive = 1
              AND ExpiryDate IS NOT NULL
              AND ExpiryDate >= CAST(GETDATE() AS DATE)
              AND ExpiryDate <= DATEADD(
                    DAY,
                    30,
                    CAST(GETDATE() AS DATE)
              )
            ORDER BY ExpiryDate ASC;


            /* =====================================================
               4. EXPIRED MEDICINES
               Shared pharmacy inventory
               ===================================================== */

            SELECT
                MedicineId,
                MedicineName,
                BatchNo,
                ExpiryDate,
                Quantity
            FROM Medicines
            WHERE IsActive = 1
              AND ExpiryDate IS NOT NULL
              AND ExpiryDate < CAST(GETDATE() AS DATE)
            ORDER BY ExpiryDate ASC;


            /* =====================================================
               5. RECENT SALES
               Admin -> all
               Other users -> own
               ===================================================== */

            SELECT TOP 5
                SaleId,
                InvoiceNo,
                CustomerName,
                SaleDate,
                TotalAmount
            FROM Sales
            WHERE
                @IsAdmin = 1
                OR UserId = @UserId
            ORDER BY SaleDate DESC;


            /* =====================================================
               6. RECENT PURCHASES
               Admin/Pharmacist only
               Admin -> all
               Pharmacist -> own
               Staff -> none
               ===================================================== */

            SELECT TOP 5
                PurchaseId,
                PurchaseDate,
                TotalAmount
            FROM Purchases
            WHERE
                @CanViewPurchases = 1
                AND (
                    @IsAdmin = 1
                    OR UserId = @UserId
                )
            ORDER BY PurchaseDate DESC;
            """;

        await using var connection =
            new SqlConnection(_connectionString);

        await connection.OpenAsync();

        await using var command =
            new SqlCommand(
                sql,
                connection);

        command.Parameters.Add(
            "@UserId",
            SqlDbType.Int).Value =
            currentUserId;

        command.Parameters.Add(
            "@IsAdmin",
            SqlDbType.Bit).Value =
            isAdmin;

        command.Parameters.Add(
            "@CanViewPurchases",
            SqlDbType.Bit).Value =
            canViewPurchases;

        await using var reader =
            await command.ExecuteReaderAsync();

        var dashboard =
            new DashboardViewModel();

        // =====================================================
        // 1. DASHBOARD STATISTICS
        // =====================================================

        if (await reader.ReadAsync())
        {
            dashboard.TotalMedicines =
                reader.GetInt32(
                    reader.GetOrdinal(
                        "TotalMedicines"));

            dashboard.TotalStock =
                reader.GetInt32(
                    reader.GetOrdinal(
                        "TotalStock"));

            dashboard.TodaySalesCount =
                reader.GetInt32(
                    reader.GetOrdinal(
                        "TodaySalesCount"));

            dashboard.TodaySalesAmount =
                reader.GetDecimal(
                    reader.GetOrdinal(
                        "TodaySalesAmount"));

            dashboard.TotalPurchases =
                reader.GetInt32(
                    reader.GetOrdinal(
                        "TotalPurchases"));

            dashboard.TotalPurchaseAmount =
                reader.GetDecimal(
                    reader.GetOrdinal(
                        "TotalPurchaseAmount"));
        }

        // =====================================================
        // 2. LOW STOCK
        // =====================================================

        if (await reader.NextResultAsync())
        {
            while (await reader.ReadAsync())
            {
                dashboard.LowStockMedicines.Add(
                    MapAlertItem(reader));
            }
        }

        // =====================================================
        // 3. EXPIRING SOON
        // =====================================================

        if (await reader.NextResultAsync())
        {
            while (await reader.ReadAsync())
            {
                dashboard.ExpiringSoonMedicines.Add(
                    MapAlertItem(reader));
            }
        }

        // =====================================================
        // 4. EXPIRED
        // =====================================================

        if (await reader.NextResultAsync())
        {
            while (await reader.ReadAsync())
            {
                dashboard.ExpiredMedicines.Add(
                    MapAlertItem(reader));
            }
        }

        // =====================================================
        // 5. RECENT SALES
        // =====================================================

        if (await reader.NextResultAsync())
        {
            while (await reader.ReadAsync())
            {
                dashboard.RecentSales.Add(
                    new DashboardRecentSale
                    {
                        SaleId =
                            reader.GetInt32(
                                reader.GetOrdinal(
                                    "SaleId")),

                        InvoiceNo =
                            reader.IsDBNull(
                                reader.GetOrdinal(
                                    "InvoiceNo"))
                                ? null
                                : reader.GetString(
                                    reader.GetOrdinal(
                                        "InvoiceNo")),

                        CustomerName =
                            reader.IsDBNull(
                                reader.GetOrdinal(
                                    "CustomerName"))
                                ? null
                                : reader.GetString(
                                    reader.GetOrdinal(
                                        "CustomerName")),

                        SaleDate =
                            reader.GetDateTime(
                                reader.GetOrdinal(
                                    "SaleDate")),

                        TotalAmount =
                            reader.GetDecimal(
                                reader.GetOrdinal(
                                    "TotalAmount"))
                    });
            }
        }

        // =====================================================
        // 6. RECENT PURCHASES
        // =====================================================

        if (await reader.NextResultAsync())
        {
            while (await reader.ReadAsync())
            {
                dashboard.RecentPurchases.Add(
                    new DashboardRecentPurchase
                    {
                        PurchaseId =
                            reader.GetInt32(
                                reader.GetOrdinal(
                                    "PurchaseId")),

                        PurchaseDate =
                            reader.GetDateTime(
                                reader.GetOrdinal(
                                    "PurchaseDate")),

                        TotalAmount =
                            reader.GetDecimal(
                                reader.GetOrdinal(
                                    "TotalAmount"))
                    });
            }
        }

        return dashboard;
    }

    // =========================================================
    // MAP MEDICINE ALERT
    // =========================================================

    private static DashboardAlertItem MapAlertItem(
        SqlDataReader reader)
    {
        return new DashboardAlertItem
        {
            MedicineId =
                reader.GetInt32(
                    reader.GetOrdinal(
                        "MedicineId")),

            MedicineName =
                reader.GetString(
                    reader.GetOrdinal(
                        "MedicineName")),

            BatchNo =
                reader.IsDBNull(
                    reader.GetOrdinal(
                        "BatchNo"))
                    ? null
                    : reader.GetString(
                        reader.GetOrdinal(
                            "BatchNo")),

            ExpiryDate =
                reader.IsDBNull(
                    reader.GetOrdinal(
                        "ExpiryDate"))
                    ? null
                    : reader.GetDateTime(
                        reader.GetOrdinal(
                            "ExpiryDate")),

            Quantity =
                reader.GetInt32(
                    reader.GetOrdinal(
                        "Quantity"))
        };
    }
}
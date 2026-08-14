using Microsoft.Data.SqlClient;
using PharmacyManagement.Models;
using System.Data;

namespace PharmacyManagement.Repositories;

public class ReportRepository : IReportRepository
{
    private readonly string _connectionString;

    public ReportRepository(
        IConfiguration configuration)
    {
        _connectionString =
            configuration.GetConnectionString(
                "DefaultConnection")
            ?? throw new InvalidOperationException(
                "DefaultConnection is not configured.");
    }

    // ============================
    // SALES REPORT
    // ============================

    public async Task<SalesReportViewModel>
        GetSalesReportAsync(
            DateTime? fromDate,
            DateTime? toDate,
            int currentUserId,
            bool isAdmin)
    {
        var report =
            new SalesReportViewModel
            {
                FromDate = fromDate,
                ToDate = toDate
            };

        var sales =
            new List<Sale>();

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
            WHERE
                (@IsAdmin = 1 OR UserId = @UserId)
            AND
                (@FromDate IS NULL
                 OR SaleDate >= @FromDate)
            AND
                (@ToDate IS NULL
                 OR SaleDate < DATEADD(DAY, 1, @ToDate))
            ORDER BY
                SaleDate DESC,
                SaleId DESC;
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

        command.Parameters.Add(
            "@FromDate",
            SqlDbType.DateTime2).Value =
                (object?)fromDate ??
                DBNull.Value;

        command.Parameters.Add(
            "@ToDate",
            SqlDbType.DateTime2).Value =
                (object?)toDate ??
                DBNull.Value;

        await using var reader =
            await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            sales.Add(
                new Sale
                {
                    SaleId =
                        reader.GetInt32(
                            reader.GetOrdinal(
                                "SaleId")),

                    UserId =
                        reader.GetInt32(
                            reader.GetOrdinal(
                                "UserId")),

                    CustomerName =
                        reader.IsDBNull(
                            reader.GetOrdinal(
                                "CustomerName"))
                            ? null
                            : reader.GetString(
                                reader.GetOrdinal(
                                    "CustomerName")),

                    CustomerMobile =
                        reader.IsDBNull(
                            reader.GetOrdinal(
                                "CustomerMobile"))
                            ? null
                            : reader.GetString(
                                reader.GetOrdinal(
                                    "CustomerMobile")),

                    SaleDate =
                        reader.GetDateTime(
                            reader.GetOrdinal(
                                "SaleDate")),

                    InvoiceNo =
                        reader.IsDBNull(
                            reader.GetOrdinal(
                                "InvoiceNo"))
                            ? null
                            : reader.GetString(
                                reader.GetOrdinal(
                                    "InvoiceNo")),

                    Discount =
                        reader.GetDecimal(
                            reader.GetOrdinal(
                                "Discount")),

                    TotalAmount =
                        reader.GetDecimal(
                            reader.GetOrdinal(
                                "TotalAmount")),

                    CreatedAt =
                        reader.GetDateTime(
                            reader.GetOrdinal(
                                "CreatedAt"))
                });
        }

        report.Sales = sales;

        report.TotalTransactions =
            sales.Count;

        report.TotalDiscount =
            sales.Sum(x => x.Discount);

        report.TotalSales =
            sales.Sum(x => x.TotalAmount);

        return report;
    }


    // ============================
    // PURCHASE REPORT
    // ============================

    public async Task<PurchaseReportViewModel>
        GetPurchaseReportAsync(
            DateTime? fromDate,
            DateTime? toDate,
            int currentUserId,
            bool isAdmin)
    {
        var report =
            new PurchaseReportViewModel
            {
                FromDate = fromDate,
                ToDate = toDate
            };

        var purchases =
            new List<Purchase>();

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
            WHERE
                (@IsAdmin = 1 OR p.UserId = @UserId)
            AND
                (@FromDate IS NULL
                 OR p.PurchaseDate >= @FromDate)
            AND
                (@ToDate IS NULL
                 OR p.PurchaseDate < DATEADD(DAY, 1, @ToDate))
            ORDER BY
                p.PurchaseDate DESC,
                p.PurchaseId DESC;
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

        command.Parameters.Add(
            "@FromDate",
            SqlDbType.DateTime2).Value =
                (object?)fromDate ??
                DBNull.Value;

        command.Parameters.Add(
            "@ToDate",
            SqlDbType.DateTime2).Value =
                (object?)toDate ??
                DBNull.Value;

        await using var reader =
            await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            purchases.Add(
                new Purchase
                {
                    PurchaseId =
                        reader.GetInt32(
                            reader.GetOrdinal(
                                "PurchaseId")),

                    UserId =
                        reader.GetInt32(
                            reader.GetOrdinal(
                                "UserId")),

                    SupplierId =
                        reader.GetInt32(
                            reader.GetOrdinal(
                                "SupplierId")),

                    SupplierName =
                        reader.GetString(
                            reader.GetOrdinal(
                                "SupplierName")),

                    PurchaseDate =
                        reader.GetDateTime(
                            reader.GetOrdinal(
                                "PurchaseDate")),

                    InvoiceNo =
                        reader.IsDBNull(
                            reader.GetOrdinal(
                                "InvoiceNo"))
                            ? null
                            : reader.GetString(
                                reader.GetOrdinal(
                                    "InvoiceNo")),

                    TotalAmount =
                        reader.GetDecimal(
                            reader.GetOrdinal(
                                "TotalAmount")),

                    CreatedAt =
                        reader.GetDateTime(
                            reader.GetOrdinal(
                                "CreatedAt"))
                });
        }

        report.Purchases = purchases;

        report.TotalTransactions =
            purchases.Count;

        report.TotalPurchases =
            purchases.Sum(
                x => x.TotalAmount);

        return report;
    }


    // ============================
    // STOCK REPORT
    // ============================

    public async Task<StockReportViewModel>
        GetStockReportAsync()
    {
        var report =
            new StockReportViewModel();

        var medicines =
            new List<Medicine>();

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
            ORDER BY m.MedicineName;
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
            medicines.Add(
                new Medicine
                {
                    MedicineId =
                        reader.GetInt32(
                            reader.GetOrdinal(
                                "MedicineId")),

                    MedicineName =
                        reader.GetString(
                            reader.GetOrdinal(
                                "MedicineName")),

                    Manufacturer =
                        reader.IsDBNull(
                            reader.GetOrdinal(
                                "Manufacturer"))
                            ? null
                            : reader.GetString(
                                reader.GetOrdinal(
                                    "Manufacturer")),

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

                    PurchasePrice =
                        reader.GetDecimal(
                            reader.GetOrdinal(
                                "PurchasePrice")),

                    SellingPrice =
                        reader.GetDecimal(
                            reader.GetOrdinal(
                                "SellingPrice")),

                    Quantity =
                        reader.GetInt32(
                            reader.GetOrdinal(
                                "Quantity")),

                    IsActive =
                        reader.GetBoolean(
                            reader.GetOrdinal(
                                "IsActive")),

                    CreatedAt =
                        reader.GetDateTime(
                            reader.GetOrdinal(
                                "CreatedAt")),

                    CategoryId =
                        reader.IsDBNull(
                            reader.GetOrdinal(
                                "CategoryId"))
                            ? null
                            : reader.GetInt32(
                                reader.GetOrdinal(
                                    "CategoryId")),

                    SupplierId =
                        reader.IsDBNull(
                            reader.GetOrdinal(
                                "SupplierId"))
                            ? null
                            : reader.GetInt32(
                                reader.GetOrdinal(
                                    "SupplierId")),

                    CategoryName =
                        reader.IsDBNull(
                            reader.GetOrdinal(
                                "CategoryName"))
                            ? null
                            : reader.GetString(
                                reader.GetOrdinal(
                                    "CategoryName")),

                    SupplierName =
                        reader.IsDBNull(
                            reader.GetOrdinal(
                                "SupplierName"))
                            ? null
                            : reader.GetString(
                                reader.GetOrdinal(
                                    "SupplierName"))
                });
        }

        report.Medicines = medicines;

        report.TotalMedicines =
            medicines.Count;

        report.TotalStockQuantity =
            medicines.Sum(x => x.Quantity);

        report.TotalStockValue =
            medicines.Sum(
                x => x.Quantity * x.PurchasePrice);

        report.LowStockCount =
            medicines.Count(
                x => x.Quantity > 0 &&
                     x.Quantity <= 10);

        report.OutOfStockCount =
            medicines.Count(
                x => x.Quantity == 0);

        var expiryThreshold =
            DateTime.Today.AddDays(30);

        report.ExpiringSoonCount =
            medicines.Count(
                x => x.ExpiryDate.HasValue &&
                     x.ExpiryDate.Value.Date >= DateTime.Today &&
                     x.ExpiryDate.Value.Date <= expiryThreshold);

        return report;
    }
}
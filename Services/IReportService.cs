using PharmacyManagement.Models;

namespace PharmacyManagement.Services;

public interface IReportService
{
    Task<SalesReportViewModel> GetSalesReportAsync(
        DateTime? fromDate,
        DateTime? toDate,
        int currentUserId,
        bool isAdmin);

    Task<PurchaseReportViewModel> GetPurchaseReportAsync(
        DateTime? fromDate,
        DateTime? toDate,
        int currentUserId,
        bool isAdmin);

    Task<StockReportViewModel> GetStockReportAsync();
}
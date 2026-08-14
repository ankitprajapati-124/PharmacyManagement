using PharmacyManagement.Models;

namespace PharmacyManagement.Repositories;

public interface IReportRepository
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
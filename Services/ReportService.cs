using PharmacyManagement.Models;
using PharmacyManagement.Repositories;

namespace PharmacyManagement.Services;

public class ReportService : IReportService
{
    private readonly IReportRepository _repository;

    public ReportService(
        IReportRepository repository)
    {
        _repository = repository;
    }

    public async Task<SalesReportViewModel>
        GetSalesReportAsync(
            DateTime? fromDate,
            DateTime? toDate,
            int currentUserId,
            bool isAdmin)
    {
        return await _repository.GetSalesReportAsync(
            fromDate,
            toDate,
            currentUserId,
            isAdmin);
    }

    public async Task<PurchaseReportViewModel>
        GetPurchaseReportAsync(
            DateTime? fromDate,
            DateTime? toDate,
            int currentUserId,
            bool isAdmin)
    {
        return await _repository.GetPurchaseReportAsync(
            fromDate,
            toDate,
            currentUserId,
            isAdmin);
    }

    public async Task<StockReportViewModel>
        GetStockReportAsync()
    {
        return await _repository.GetStockReportAsync();
    }
}
using PharmacyManagement.Models;
using PharmacyManagement.Repositories;

namespace PharmacyManagement.Services;

public class DashboardService : IDashboardService
{
    private readonly IDashboardRepository _repository;

    public DashboardService(
        IDashboardRepository repository)
    {
        _repository = repository;
    }

    public async Task<DashboardViewModel> GetDashboardAsync(
        int currentUserId,
        bool isAdmin,
        bool canViewPurchases)
    {
        return await _repository.GetDashboardAsync(
            currentUserId,
            isAdmin,
            canViewPurchases);
    }
}
using PharmacyManagement.Models;

namespace PharmacyManagement.Repositories;

public interface IDashboardRepository
{
    Task<DashboardViewModel> GetDashboardAsync(
        int currentUserId,
        bool isAdmin,
        bool canViewPurchases);
}
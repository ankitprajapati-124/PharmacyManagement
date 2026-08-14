using PharmacyManagement.Models;

namespace PharmacyManagement.Services;

public interface IDashboardService
{
    Task<DashboardViewModel> GetDashboardAsync(
        int currentUserId,
        bool isAdmin,
        bool canViewPurchases);
}
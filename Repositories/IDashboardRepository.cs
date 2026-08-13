using PharmacyManagement.Models;

namespace PharmacyManagement.Repositories;

public interface IDashboardRepository
{
    Task<DashboardViewModel> GetDashboardAsync();
}
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

    public async Task<DashboardViewModel> GetDashboardAsync()
    {
        return await _repository.GetDashboardAsync();
    }
}
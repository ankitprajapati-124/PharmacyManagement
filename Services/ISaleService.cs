using PharmacyManagement.Models;

namespace PharmacyManagement.Services;

public interface ISaleService
{
    Task<IReadOnlyList<Sale>> GetAllAsync();

    Task<Sale?> GetByIdAsync(int id);

    Task<int> AddAsync(Sale sale);

    Task<bool> DeleteAsync(int id);
}
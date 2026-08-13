using PharmacyManagement.Models;

namespace PharmacyManagement.Repositories;

public interface ISaleRepository
{
    Task<IReadOnlyList<Sale>> GetAllAsync();

    Task<Sale?> GetByIdAsync(int id);

    Task<int> AddAsync(Sale sale);

    Task<bool> DeleteAsync(int id);
}
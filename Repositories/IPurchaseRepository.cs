using PharmacyManagement.Models;

namespace PharmacyManagement.Repositories;

public interface IPurchaseRepository
{
    Task<IReadOnlyList<Purchase>> GetAllAsync();

    Task<Purchase?> GetByIdAsync(int id);

    Task<int> AddAsync(Purchase purchase);

    Task<bool> DeleteAsync(int id);
}
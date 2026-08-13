using PharmacyManagement.Models;

namespace PharmacyManagement.Services;

public interface IPurchaseService
{
    Task<IReadOnlyList<Purchase>> GetAllAsync();

    Task<Purchase?> GetByIdAsync(int id);

    Task<int> AddAsync(Purchase purchase);

    Task<bool> DeleteAsync(int id);
}
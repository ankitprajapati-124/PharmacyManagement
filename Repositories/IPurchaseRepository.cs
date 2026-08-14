using PharmacyManagement.Models;

namespace PharmacyManagement.Repositories;

public interface IPurchaseRepository
{
    Task<IReadOnlyList<Purchase>> GetAllAsync(
        int currentUserId,
        bool isAdmin);

    Task<Purchase?> GetByIdAsync(
        int id,
        int currentUserId,
        bool isAdmin);

    Task<int> AddAsync(
        Purchase purchase,
        int currentUserId);

    Task<bool> DeleteAsync(
        int id,
        int currentUserId,
        bool isAdmin);
}
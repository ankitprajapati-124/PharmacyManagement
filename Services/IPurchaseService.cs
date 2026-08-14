using PharmacyManagement.Models;

namespace PharmacyManagement.Services;

public interface IPurchaseService
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
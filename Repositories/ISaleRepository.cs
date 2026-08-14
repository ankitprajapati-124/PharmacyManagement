using PharmacyManagement.Models;

namespace PharmacyManagement.Repositories;

public interface ISaleRepository
{
    Task<IReadOnlyList<Sale>> GetAllAsync(
        int currentUserId,
        bool isAdmin);

    Task<Sale?> GetByIdAsync(
        int id,
        int currentUserId,
        bool isAdmin);

    Task<int> AddAsync(
        Sale sale,
        int currentUserId);

    Task<bool> DeleteAsync(
        int id,
        int currentUserId,
        bool isAdmin);
}
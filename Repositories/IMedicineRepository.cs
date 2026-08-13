using PharmacyManagement.Models;

namespace PharmacyManagement.Repositories;

public interface IMedicineRepository
{
    Task<IReadOnlyList<Medicine>> GetAllAsync(string? search = null);
    Task<Medicine?> GetByIdAsync(int id);
    Task<int> AddAsync(Medicine medicine);
    Task<bool> UpdateAsync(Medicine medicine);
    Task<bool> DeleteAsync(int id);
}

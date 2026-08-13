using PharmacyManagement.Models;

namespace PharmacyManagement.Services;

public interface ICategoryService
{
    Task<IEnumerable<Category>> GetAllAsync();

    Task<Category?> GetByIdAsync(int id);

    Task<int> AddAsync(Category category);

    Task<bool> UpdateAsync(Category category);

    Task<bool> DeleteAsync(int id);
}
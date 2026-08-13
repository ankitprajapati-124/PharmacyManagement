using PharmacyManagement.Models;
using PharmacyManagement.Repositories;

namespace PharmacyManagement.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _repository;

    public CategoryService(ICategoryRepository repository)
    {
        _repository = repository;
    }

    public Task<IEnumerable<Category>> GetAllAsync()
    {
        return _repository.GetAllAsync();
    }

    public Task<Category?> GetByIdAsync(int id)
    {
        return _repository.GetByIdAsync(id);
    }

    public Task<int> AddAsync(Category category)
    {
        return _repository.AddAsync(category);
    }

    public Task<bool> UpdateAsync(Category category)
    {
        return _repository.UpdateAsync(category);
    }

    public Task<bool> DeleteAsync(int id)
    {
        return _repository.DeleteAsync(id);
    }
}
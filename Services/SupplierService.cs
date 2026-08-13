using PharmacyManagement.Models;
using PharmacyManagement.Repositories;

namespace PharmacyManagement.Services;

public class SupplierService : ISupplierService
{
    private readonly ISupplierRepository _repository;

    public SupplierService(ISupplierRepository repository)
    {
        _repository = repository;
    }

    public Task<IEnumerable<Supplier>> GetAllAsync()
    {
        return _repository.GetAllAsync();
    }

    public Task<Supplier?> GetByIdAsync(int id)
    {
        return _repository.GetByIdAsync(id);
    }

    public Task<int> AddAsync(Supplier supplier)
    {
        return _repository.AddAsync(supplier);
    }

    public Task<bool> UpdateAsync(Supplier supplier)
    {
        return _repository.UpdateAsync(supplier);
    }

    public Task<bool> DeleteAsync(int id)
    {
        return _repository.DeleteAsync(id);
    }
}
using PharmacyManagement.Models;

namespace PharmacyManagement.Services;

public interface ISupplierService
{
    Task<IEnumerable<Supplier>> GetAllAsync();

    Task<Supplier?> GetByIdAsync(int id);

    Task<int> AddAsync(Supplier supplier);

    Task<bool> UpdateAsync(Supplier supplier);

    Task<bool> DeleteAsync(int id);
}
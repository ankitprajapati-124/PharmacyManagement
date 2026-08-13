using PharmacyManagement.Models;
using PharmacyManagement.Repositories;

namespace PharmacyManagement.Services;

public class MedicineService : IMedicineService
{
    private readonly IMedicineRepository _repository;

    public MedicineService(IMedicineRepository repository)
    {
        _repository = repository;
    }

    public Task<IReadOnlyList<Medicine>> GetAllAsync(string? search = null) =>
        _repository.GetAllAsync(search);

    public Task<Medicine?> GetByIdAsync(int id) =>
        _repository.GetByIdAsync(id);

    public Task<int> AddAsync(Medicine medicine) =>
        _repository.AddAsync(medicine);

    public Task<bool> UpdateAsync(Medicine medicine) =>
        _repository.UpdateAsync(medicine);

    public Task<bool> DeleteAsync(int id) =>
        _repository.DeleteAsync(id);
}

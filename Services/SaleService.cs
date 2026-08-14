using PharmacyManagement.Models;
using PharmacyManagement.Repositories;

namespace PharmacyManagement.Services;

public class SaleService : ISaleService
{
    private readonly ISaleRepository _repository;

    public SaleService(
        ISaleRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<Sale>> GetAllAsync(
        int currentUserId,
        bool isAdmin)
    {
        return await _repository.GetAllAsync(
            currentUserId,
            isAdmin);
    }

    public async Task<Sale?> GetByIdAsync(
        int id,
        int currentUserId,
        bool isAdmin)
    {
        return await _repository.GetByIdAsync(
            id,
            currentUserId,
            isAdmin);
    }

    public async Task<int> AddAsync(
        Sale sale,
        int currentUserId)
    {
        return await _repository.AddAsync(
            sale,
            currentUserId);
    }

    public async Task<bool> DeleteAsync(
        int id,
        int currentUserId,
        bool isAdmin)
    {
        return await _repository.DeleteAsync(
            id,
            currentUserId,
            isAdmin);
    }
}
using PharmacyManagement.Models;
using PharmacyManagement.Repositories;

namespace PharmacyManagement.Services;

public class PurchaseService : IPurchaseService
{
    private readonly IPurchaseRepository _repository;

    public PurchaseService(
        IPurchaseRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<Purchase>> GetAllAsync(
        int currentUserId,
        bool isAdmin)
    {
        return await _repository.GetAllAsync(
            currentUserId,
            isAdmin);
    }

    public async Task<Purchase?> GetByIdAsync(
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
        Purchase purchase,
        int currentUserId)
    {
        if (purchase.SupplierId <= 0)
            throw new ArgumentException(
                "Please select a supplier.");

        if (purchase.Items is null ||
            purchase.Items.Count == 0)
        {
            throw new ArgumentException(
                "At least one medicine is required.");
        }

        purchase.TotalAmount =
            purchase.Items.Sum(
                x => x.Quantity * x.PurchasePrice);

        return await _repository.AddAsync(
            purchase,
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
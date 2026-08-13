using PharmacyManagement.Models;
using PharmacyManagement.Repositories;

namespace PharmacyManagement.Services;

public class PurchaseService : IPurchaseService
{
    private readonly IPurchaseRepository _repository;

    public PurchaseService(IPurchaseRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<Purchase>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<Purchase?> GetByIdAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<int> AddAsync(Purchase purchase)
    {
        if (purchase.SupplierId <= 0)
            throw new ArgumentException(
                "Please select a supplier.");

        if (purchase.Items is null ||
            purchase.Items.Count == 0)
            throw new ArgumentException(
                "At least one medicine is required.");

        purchase.TotalAmount =
            purchase.Items.Sum(
                x => x.Quantity * x.PurchasePrice);

        return await _repository.AddAsync(purchase);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _repository.DeleteAsync(id);
    }
}
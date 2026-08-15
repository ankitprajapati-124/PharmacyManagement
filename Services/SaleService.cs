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
        // =========================================
        // BASIC VALIDATION
        // =========================================

        if (sale.Items is null ||
            sale.Items.Count == 0)
        {
            throw new ArgumentException(
                "At least one medicine is required.");
        }

        if (sale.Discount < 0)
        {
            throw new ArgumentException(
                "Discount cannot be negative.");
        }

        // =========================================
        // CALCULATE SUBTOTAL FROM SALE ITEMS
        // =========================================

        decimal subtotal =
            sale.Items.Sum(
                item =>
                    item.Quantity *
                    item.SellingPrice);

        // =========================================
        // DISCOUNT CANNOT EXCEED SUBTOTAL
        // =========================================

        if (sale.Discount > subtotal)
        {
            throw new ArgumentException(
                "Discount cannot be greater than the sale subtotal.");
        }

        // =========================================
        // CALCULATE FINAL TOTAL
        // =========================================

        sale.TotalAmount =
            subtotal - sale.Discount;

        // =========================================
        // SAVE SALE
        // =========================================

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
using PharmacyManagement.Models;
using PharmacyManagement.Repositories;

namespace PharmacyManagement.Services;

public class SaleService : ISaleService
{
    private readonly ISaleRepository _repository;

    public SaleService(ISaleRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<Sale>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<Sale?> GetByIdAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<int> AddAsync(Sale sale)
    {
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

        // Calculate subtotal
        var subtotal = sale.Items.Sum(
            x => x.Quantity * x.SellingPrice);

        // Calculate final total
        sale.TotalAmount =
            subtotal - sale.Discount;

        if (sale.TotalAmount < 0)
        {
            throw new ArgumentException(
                "Discount cannot be greater than the sale amount.");
        }

        return await _repository.AddAsync(sale);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _repository.DeleteAsync(id);
    }
}
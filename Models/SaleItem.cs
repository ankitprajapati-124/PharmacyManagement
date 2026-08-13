namespace PharmacyManagement.Models;

public class SaleItem
{
    public int SaleItemId { get; set; }

    public int SaleId { get; set; }

    public int MedicineId { get; set; }

    public string? MedicineName { get; set; }

    public int AvailableStock { get; set; }

    public int Quantity { get; set; }

    public decimal SellingPrice { get; set; }

    public decimal TotalPrice =>
        Quantity * SellingPrice;
}
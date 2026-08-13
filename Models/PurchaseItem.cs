namespace PharmacyManagement.Models;

public class PurchaseItem
{
    public int PurchaseItemId { get; set; }

    public int PurchaseId { get; set; }

    public int MedicineId { get; set; }

    public string? MedicineName { get; set; }

    public int Quantity { get; set; }

    public decimal PurchasePrice { get; set; }

    public decimal TotalPrice =>
        Quantity * PurchasePrice;
}
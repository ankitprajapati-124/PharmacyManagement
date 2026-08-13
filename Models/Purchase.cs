namespace PharmacyManagement.Models;

public class Purchase
{
    public int PurchaseId { get; set; }

    public int SupplierId { get; set; }

    public string? SupplierName { get; set; }

    public DateTime PurchaseDate { get; set; }

    public string? InvoiceNo { get; set; }

    public decimal TotalAmount { get; set; }

    public DateTime CreatedAt { get; set; }

    public List<PurchaseItem> Items { get; set; } = new();
}
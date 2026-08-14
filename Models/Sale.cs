namespace PharmacyManagement.Models;

public class Sale
{
    public int SaleId { get; set; }

    public int UserId { get; set; }

    public string? CustomerName { get; set; }

    public string? CustomerMobile { get; set; }

    public DateTime SaleDate { get; set; }

    public string? InvoiceNo { get; set; }

    public decimal Discount { get; set; }

    public decimal TotalAmount { get; set; }

    public DateTime CreatedAt { get; set; }

    public List<SaleItem> Items { get; set; } = new();
}
namespace PharmacyManagement.Models;

public class DashboardRecentSale
{
    public int SaleId { get; set; }

    public string? InvoiceNo { get; set; }

    public string? CustomerName { get; set; }

    public DateTime SaleDate { get; set; }

    public decimal TotalAmount { get; set; }
}
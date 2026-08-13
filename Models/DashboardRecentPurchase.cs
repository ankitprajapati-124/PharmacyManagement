namespace PharmacyManagement.Models;

public class DashboardRecentPurchase
{
    public int PurchaseId { get; set; }

    public DateTime PurchaseDate { get; set; }

    public decimal TotalAmount { get; set; }
}
namespace PharmacyManagement.Models;

public class DashboardViewModel
{
    public int TotalMedicines { get; set; }

    public int TotalStock { get; set; }

    public int TodaySalesCount { get; set; }

    public decimal TodaySalesAmount { get; set; }

    public int TotalPurchases { get; set; }

    public decimal TotalPurchaseAmount { get; set; }

    public List<DashboardAlertItem> LowStockMedicines { get; set; } = [];

    public List<DashboardAlertItem> ExpiringSoonMedicines { get; set; } = [];

    public List<DashboardAlertItem> ExpiredMedicines { get; set; } = [];

    public List<DashboardRecentSale> RecentSales { get; set; } = [];

    public List<DashboardRecentPurchase> RecentPurchases { get; set; } = [];
}
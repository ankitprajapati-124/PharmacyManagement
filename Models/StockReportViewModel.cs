namespace PharmacyManagement.Models;

public class StockReportViewModel
{
    public int TotalMedicines { get; set; }

    public int TotalStockQuantity { get; set; }

    public decimal TotalStockValue { get; set; }

    public int LowStockCount { get; set; }

    public int OutOfStockCount { get; set; }

    public int ExpiringSoonCount { get; set; }

    public IReadOnlyList<Medicine> Medicines { get; set; }
        = Array.Empty<Medicine>();
}
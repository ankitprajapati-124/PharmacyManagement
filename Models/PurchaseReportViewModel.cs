using System.ComponentModel.DataAnnotations;

namespace PharmacyManagement.Models;

public class PurchaseReportViewModel
{
    [DataType(DataType.Date)]
    [Display(Name = "From Date")]
    public DateTime? FromDate { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "To Date")]
    public DateTime? ToDate { get; set; }

    public int TotalTransactions { get; set; }

    public decimal TotalPurchases { get; set; }

    public IReadOnlyList<Purchase> Purchases { get; set; }
        = Array.Empty<Purchase>();
}
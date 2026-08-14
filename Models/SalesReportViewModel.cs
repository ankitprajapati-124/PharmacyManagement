using System.ComponentModel.DataAnnotations;

namespace PharmacyManagement.Models;

public class SalesReportViewModel
{
    [DataType(DataType.Date)]
    [Display(Name = "From Date")]
    public DateTime? FromDate { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "To Date")]
    public DateTime? ToDate { get; set; }

    public int TotalTransactions { get; set; }

    public decimal TotalDiscount { get; set; }

    public decimal TotalSales { get; set; }

    public IReadOnlyList<Sale> Sales { get; set; }
        = Array.Empty<Sale>();
}
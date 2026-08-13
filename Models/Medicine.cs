using System.ComponentModel.DataAnnotations;

namespace PharmacyManagement.Models;

public class Medicine
{
    public int MedicineId { get; set; }

    [Required(ErrorMessage = "Medicine name is required.")]
    [StringLength(150)]
    [Display(Name = "Medicine Name")]
    public string MedicineName { get; set; } = string.Empty;

    [StringLength(150)]
    public string? Manufacturer { get; set; }

    [StringLength(50)]
    [Display(Name = "Batch No.")]
    public string? BatchNo { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Expiry Date")]
    public DateTime? ExpiryDate { get; set; }

    [Range(0, 999999999)]
    [Display(Name = "Purchase Price")]
    public decimal PurchasePrice { get; set; }

    [Range(0, 999999999)]
    [Display(Name = "Selling Price")]
    public decimal SellingPrice { get; set; }

    [Range(0, int.MaxValue)]
    public int Quantity { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public int? CategoryId { get; set; }

    public int? SupplierId { get; set; }

    public string? CategoryName { get; set; }

    public string? SupplierName { get; set; }
}

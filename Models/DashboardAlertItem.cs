namespace PharmacyManagement.Models;

public class DashboardAlertItem
{
    public int MedicineId { get; set; }

    public string MedicineName { get; set; } = string.Empty;

    public string? BatchNo { get; set; }

    public DateTime? ExpiryDate { get; set; }

    public int Quantity { get; set; }
}
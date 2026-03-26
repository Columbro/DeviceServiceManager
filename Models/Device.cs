
namespace DeviceServiceManager.Models
{
    /// <summary>
    /// Represents a physical device (e.g., barcode scanner, printer) covered by a maintenance contract.
    /// </summary>
    public class Device
    {
        public int Id { get; set; }
        public string SerialNumber { get; set; } = string.Empty;
        public string Manufacturer { get; set; } = string.Empty;
        public string Designation { get; set; } = string.Empty;
        public string? Type { get; set; }
        public int? MaintenanceContractId { get; set; }
    }
}

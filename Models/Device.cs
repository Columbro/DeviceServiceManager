
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
        public string Status { get; set; } = "aktiv";

        public Device Clone()
        {
            return new Device
            {
                Id = this.Id,
                SerialNumber = this.SerialNumber,
                Manufacturer = this.Manufacturer,
                Designation = this.Designation,
                Type = this.Type,
                MaintenanceContractId = this.MaintenanceContractId,
                Status = this.Status
            };
        }
    }
}

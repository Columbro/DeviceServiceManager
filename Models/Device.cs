
namespace DeviceServiceManager.Models
{
    public class Device
    {
        public int Id { get; set; }
        public string SerialNumber { get; set; } = string.Empty;
        public string Designation { get; set; } = string.Empty;
        public string? Type { get; set; }
        public DateTime? InstallationDate { get; set; }
        public int? MaintenanceContractId { get; set; }
    }
}

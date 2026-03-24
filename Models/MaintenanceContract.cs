
namespace DeviceServiceManager.Models
{
    public class MaintenanceContract
    {
        public int Id { get; set; }
        public string ContractNumber { get; set; } = string.Empty;
        public int CustomerId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = "aktiv";
        public string? Remarks { get; set; }
    }
}

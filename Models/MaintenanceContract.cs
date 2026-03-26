
namespace DeviceServiceManager.Models
{
    /// <summary>
    /// Represents a maintenance contract that belongs to a customer and covers multiple devices.
    /// </summary>
    public class MaintenanceContract
    {
        public int Id { get; set; }
        public string ContractNumber { get; set; } = string.Empty;
        public int CustomerId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = "aktiv";
        public string? Remarks { get; set; }

        // --- Navigation Properties ---

        /// <summary>
        /// The customer object associated with this contract.
        /// Useful for displaying the customer's name in the contract line.
        /// </summary>
        public Customer? Customer { get; set; }

        /// <summary>
        /// A list of devices covered by this specific contract.
        /// </summary>
        public List<Device> CoveredDevices { get; set; } = new List<Device>();
    }
}


using System.Collections.ObjectModel;

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

        public Customer? Customer { get; set; }
        public ObservableCollection<Device> CoveredDevices { get; set; } = new ();

        /// <summary>
        /// Creates a deep copy of the contract to prevent phantom UI updates.
        /// Ensure ALL properties (especially the Id) are mapped correctly!
        /// </summary>
        /// <returns>A cloned MaintenanceContract object.</returns>
        public MaintenanceContract Clone()
        {
            var clone = new MaintenanceContract
            {
                Id = this.Id,
                ContractNumber = this.ContractNumber,
                CustomerId = this.CustomerId,
                StartDate = this.StartDate,
                EndDate = this.EndDate,
                Status = this.Status,
                Remarks = this.Remarks,
                Customer = this.Customer?.Clone()
            };

            foreach (var device in this.CoveredDevices)
            {
                clone.CoveredDevices.Add(device.Clone());
            }

            return clone;
        }
    }
}
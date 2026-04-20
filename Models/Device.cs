
using DeviceServiceManager.Core;

namespace DeviceServiceManager.Models
{
    /// <summary>
    /// Represents a physical device (e.g., barcode scanner, printer) covered by a maintenance contract.
    /// </summary>
    public class Device : ObservableObject
    {
        public int Id { get; set; }
        public string SerialNumber { get; set; } = string.Empty;
        public string Manufacturer { get; set; } = string.Empty;
        public string? Type { get; set; }
        public int? MaintenanceContractId { get; set; }
        private string _status = "aktiv";
        public string Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged();
            }
        }

        public Device Clone()
        {
            return new Device
            {
                Id = this.Id,
                SerialNumber = this.SerialNumber,
                Manufacturer = this.Manufacturer,
                Type = this.Type,
                MaintenanceContractId = this.MaintenanceContractId,
                Status = this.Status
            };
        }
    }
}

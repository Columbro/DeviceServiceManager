using DeviceServiceManager.Core;
using DeviceServiceManager.Models;
using DeviceServiceManager.Services;
using System.Collections.ObjectModel;

namespace DeviceServiceManager.ViewModels
{
    public class DashboardViewModel : ObservableObject
    {
        private readonly CustomerService _customerService;
        private readonly ContractService _contractService;

        private int _totalCustomers;
        private int _activeContracts;
        private int _expiringContractsCount;
        private int _defectiveDevicesCount;

        // --- KPI properties for the tiles ---

        public int TotalCustomers
        {
            get => _totalCustomers;
            set { _totalCustomers = value; OnPropertyChanged(); }
        }

        public int ActiveContracts
        {
            get => _activeContracts;
            set { _activeContracts = value; OnPropertyChanged(); }
        }

        public int ExpiringContractsCount
        {
            get => _expiringContractsCount;
            set { _expiringContractsCount = value; OnPropertyChanged(); }
        }

        public int DefectiveDevicesCount
        {
            get => _defectiveDevicesCount;
            set { _defectiveDevicesCount = value; OnPropertyChanged(); }
        }

        // --- List of expiring contracts ---
        public ObservableCollection<MaintenanceContract> ExpiringContractsList { get; } = new();

        public DashboardViewModel()
        {
            _customerService = new CustomerService();
            _contractService = new ContractService();

            // Load the statistics at startup
            _ = LoadStatisticsAsync();
        }

        private async Task LoadStatisticsAsync()
        {
            try
            {
                // Retrieve data from the database
                var customers = await _customerService.GetAllCustomersAsync();
                var contracts = await _contractService.GetAllContractsAsync();

                // 1. KPI: Total customers
                TotalCustomers = customers.Count;

                // 2. KPI: Active contracts
                ActiveContracts = contracts.Count(c => c.Status.Equals("aktiv", StringComparison.OrdinalIgnoreCase));

                // 3. KPI: Defective devices (We search all contracts for inactive devices)
                DefectiveDevicesCount = contracts
                    .SelectMany(c => c.CoveredDevices)
                    .Count(d => d.Status.Equals("inaktiv", StringComparison.OrdinalIgnoreCase));

                // 4. KPI & List: Expiring Contracts (Active, but with an expiration date within the next 90 days)
                DateTime thresholdDate = DateTime.Today.AddDays(90);

                var expiring = contracts
                    .Where(c => c.Status.Equals("aktiv", StringComparison.OrdinalIgnoreCase) && c.EndDate <= thresholdDate)
                    .OrderBy(c => c.EndDate)
                    .ToList();

                ExpiringContractsCount = expiring.Count;

                ExpiringContractsList.Clear();
                foreach (var contract in expiring)
                {
                    ExpiringContractsList.Add(contract);
                }
            }
            catch (Exception ex)
            {
                // Quietly ignore errors in the background, since the dashboard is for informational purposes only
                System.Diagnostics.Debug.WriteLine($"Fehler beim Laden des Dashboards: {ex.Message}");
            }
        }
    }
}
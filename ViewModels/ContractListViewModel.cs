using DeviceServiceManager.Core;
using DeviceServiceManager.Models;
using DeviceServiceManager.Services;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace DeviceServiceManager.ViewModels
{
    /// <summary>
    /// Represents the logic and data for the maintenance contract view.
    /// Manages contracts, device associations, and customer selection.
    /// </summary>
    public class ContractListViewModel : ObservableObject
    {
        private readonly ContractService _contractService;
        private readonly CustomerService _customerService;

        private MaintenanceContract? _selectedContract;
        private bool _isFormVisible;
        private string _searchText = string.Empty;
        private List<MaintenanceContract> _allContractsCache = new();

        /// <summary>
        /// Collection of contracts displayed in the Master DataGrid.
        /// </summary>
        public ObservableCollection<MaintenanceContract> Contracts { get; }

        /// <summary>
        /// Collection of available customers for the searchable Dropdown (ComboBox).
        /// </summary>
        public ObservableCollection<Customer> AvailableCustomers { get; }

        /// <summary>
        /// Gets or sets the currently selected contract in the UI.
        /// </summary>
        public MaintenanceContract? SelectedContract
        {
            get => _selectedContract;
            set
            {
                _selectedContract = value;
                OnPropertyChanged();
                if (_selectedContract != null)
                {
                    IsFormVisible = true;
                }
            }
        }

        public bool IsFormVisible
        {
            get => _isFormVisible;
            set
            {
                _isFormVisible = value;
                OnPropertyChanged();
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                ApplySearchFilter();
            }
        }

        // --- Commands ---
        public ICommand CreateNewContractCommand { get; }
        public ICommand SaveContractCommand { get; }
        public ICommand CancelCommand { get; }

        public ContractListViewModel()
        {
            _contractService = new ContractService();
            _customerService = new CustomerService(); // Needed to load customers for the dropdown

            Contracts = new ObservableCollection<MaintenanceContract>();
            AvailableCustomers = new ObservableCollection<Customer>();

            IsFormVisible = false;

            CreateNewContractCommand = new RelayCommand(ExecuteCreateNewContract);
            SaveContractCommand = new RelayCommand(async _ => await ExecuteSaveContractAsync());
            CancelCommand = new RelayCommand(ExecuteCancel);

            // Load data asynchronously on startup
            _ = LoadInitialDataAsync();
        }

        /// <summary>
        /// Loads both the contracts and the customer list from the database.
        /// </summary>
        private async Task LoadInitialDataAsync()
        {
            try
            {
                // 1. Load Customers for the Dropdown
                var customers = await _customerService.GetAllCustomersAsync();
                AvailableCustomers.Clear();
                foreach (var c in customers)
                {
                    AvailableCustomers.Add(c);
                }

                // 2. Load Contracts for the DataGrid
                await LoadContractsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Laden der Daten:\n{ex.Message}", "Datenbankfehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadContractsAsync()
        {
            _allContractsCache = await _contractService.GetAllContractsAsync();
            ApplySearchFilter();
        }

        private void ApplySearchFilter()
        {
            Contracts.Clear();
            foreach (var contract in _allContractsCache)
            {
                if (string.IsNullOrWhiteSpace(SearchText) ||
                    contract.ContractNumber.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    (contract.Customer != null && contract.Customer.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase)))
                {
                    Contracts.Add(contract);
                }
            }
        }

        private void ExecuteCreateNewContract(object? parameter)
        {
            SelectedContract = new MaintenanceContract
            {
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddYears(1), // Default: 1 Year duration
                Status = "aktiv"
            };
            IsFormVisible = true;
        }

        private async Task ExecuteSaveContractAsync()
        {
            if (SelectedContract == null) return;

            // Validation
            if (string.IsNullOrWhiteSpace(SelectedContract.ContractNumber) || SelectedContract.CustomerId <= 0)
            {
                MessageBox.Show("Bitte geben Sie eine Vertragsnummer ein und wählen Sie einen Kunden aus!", "Fehlende Daten", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Upsert Logic (Insert or Update)
                await _contractService.SaveContractAsync(SelectedContract);

                MessageBox.Show("Wartungsvertrag erfolgreich gespeichert!", "Erfolg", MessageBoxButton.OK, MessageBoxImage.Information);

                await LoadContractsAsync();
                SelectedContract = null;
                IsFormVisible = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Speichern:\n{ex.Message}", "Datenbankfehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteCancel(object? parameter)
        {
            SelectedContract = null;
            IsFormVisible = false;
        }
    }
}
using DeviceServiceManager.Core;
using DeviceServiceManager.Models;
using DeviceServiceManager.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DeviceServiceManager.ViewModels
{
    /// <summary>
    /// Represents the presentation logic and state management for the Maintenance Contracts view.
    /// Handles data binding, filtering, and CRUD operations via the service layer.
    /// </summary>
    public class ContractListViewModel : ObservableObject
    {
        private readonly ContractService _contractService;
        private readonly CustomerService _customerService;
        private readonly IDialogService _dialogService;

        private MaintenanceContract? _selectedContract;
        private MaintenanceContract? _editableContract;
        private bool _isFormVisible;
        private string _searchText = string.Empty;
        private List<MaintenanceContract> _allContractsCache = new();

        private ObservableCollection<MaintenanceContract> _contracts = new();

        /// <summary>
        /// Gets or sets the collection of contracts bound to the Master DataGrid.
        /// </summary>
        public ObservableCollection<MaintenanceContract> Contracts
        {
            get => _contracts;
            set
            {
                _contracts = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets the collection of available customers for the searchable UI ComboBox.
        /// </summary>
        public ObservableCollection<Customer> AvailableCustomers { get; }

        /// <summary>
        /// Gets or sets the currently selected contract from the DataGrid.
        /// Setting this property automatically triggers the creation of a deep clone for editing.
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
                    // Create a deep copy to prevent the UI from updating instantly before saving
                    EditableContract = _selectedContract.Clone();
                    IsFormVisible = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets the cloned contract object bound to the Detail Form.
        /// </summary>
        public MaintenanceContract? EditableContract
        {
            get => _editableContract;
            set
            {
                _editableContract = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Controls the visibility state of the detail form.
        /// Used by the XAML triggers to animate the form slide-in/out.
        /// </summary>
        public bool IsFormVisible
        {
            get => _isFormVisible;
            set
            {
                _isFormVisible = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the search term used for in-memory filtering of the contracts.
        /// </summary>
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

        /// <summary>Command to initialize the creation of a new maintenance contract.</summary>
        public ICommand CreateNewContractCommand { get; }

        /// <summary>Command to validate and persist the currently edited contract.</summary>
        public ICommand SaveContractCommand { get; }

        /// <summary>Command to discard changes and close the detail form.</summary>
        public ICommand CancelCommand { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContractListViewModel"/> class.
        /// Configures services, collections, and commands.
        /// </summary>
        public ContractListViewModel()
        {
            _contractService = new ContractService();
            _customerService = new CustomerService();
            _dialogService = new WpfDialogService();

            AvailableCustomers = new ObservableCollection<Customer>();
            IsFormVisible = false;

            CreateNewContractCommand = new RelayCommand(ExecuteCreateNewContract);
            SaveContractCommand = new RelayCommand(async _ => await ExecuteSaveContractAsync());
            CancelCommand = new RelayCommand(ExecuteCancel);

            // Execute the initial data load as a fire-and-forget background task
            _ = LoadInitialDataAsync();
        }

        /// <summary>
        /// Fetches the customer lookup data and the contract list from the database.
        /// </summary>
        private async Task LoadInitialDataAsync()
        {
            try
            {
                // 1. Load customers to populate the ComboBox
                var customers = await _customerService.GetAllCustomersAsync();
                AvailableCustomers.Clear();
                foreach (var c in customers)
                {
                    AvailableCustomers.Add(c);
                }

                // 2. Load the actual contracts
                await LoadContractsAsync();
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Fehler beim Laden der Daten:\n{ex.Message}", "Datenbankfehler");
            }
        }

        /// <summary>
        /// Retrieves all contracts from the service and applies the current search filter.
        /// </summary>
        private async Task LoadContractsAsync()
        {
            _allContractsCache = await _contractService.GetAllContractsAsync();
            ApplySearchFilter();
        }

        /// <summary>
        /// Performs an in-memory LINQ filtering on the cached contract list to ensure 
        /// high UI performance without redundant database queries.
        /// </summary>
        private void ApplySearchFilter()
        {
            var filteredList = string.IsNullOrWhiteSpace(SearchText)
                ? _allContractsCache
                : _allContractsCache.Where(c =>
                    c.ContractNumber.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    (c.Customer != null && c.Customer.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase)) ||
                    c.Status.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
                ).ToList();

            // Single update to the UI collection
            Contracts = new ObservableCollection<MaintenanceContract>(filteredList);
        }

        /// <summary>
        /// Prepares the detail form for a new contract entry.
        /// </summary>
        private void ExecuteCreateNewContract(object? parameter)
        {
            SelectedContract = null;

            EditableContract = new MaintenanceContract
            {
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddYears(4), // Default contract duration are 4 years
                Status = "aktiv"
            };

            IsFormVisible = true;
        }

        /// <summary>
        /// Validates the form input and invokes the upsert (Insert/Update) operation on the service layer.
        /// </summary>
        private async Task ExecuteSaveContractAsync()
        {
            if (EditableContract == null) return;

            // Business Validation: A contract must be linked to a customer
            if (EditableContract.CustomerId <= 0)
            {
                _dialogService.ShowWarning("Bitte wählen Sie einen Kunden aus der Dropdown-Liste aus!", "Fehlende Zuweisung");
                return;
            }

            try
            {
                if (EditableContract.Id == 0)
                {
                    // Execute INSERT
                    await _contractService.SaveContractAsync(EditableContract);
                    _dialogService.ShowMessage($"Wartungsvertrag erfolgreich angelegt!\nDie generierte Nummer lautet: {EditableContract.ContractNumber}", "Erfolg");
                }
                else
                {
                    // Execute UPDATE
                    await _contractService.SaveContractAsync(EditableContract);
                    _dialogService.ShowMessage("Vertragsänderungen erfolgreich gespeichert!", "Update erfolgreich");
                }

                // Refresh the list to display the newly saved data
                await LoadContractsAsync();

                // Cleanup UI state
                EditableContract = null;
                SelectedContract = null;
                IsFormVisible = false;
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Fehler beim Speichern:\n{ex.Message}", "Datenbankfehler");
            }
        }

        /// <summary>
        /// Discards all unsaved changes and collapses the detail form.
        /// </summary>
        private void ExecuteCancel(object? parameter)
        {
            SelectedContract = null;
            EditableContract = null;
            IsFormVisible = false;
        }
    }
}
using DeviceServiceManager.Core;
using DeviceServiceManager.Models;
using DeviceServiceManager.Repositories;
using DeviceServiceManager.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace DeviceServiceManager.ViewModels
{
    /// <summary>
    /// Represents the logic and data for the customer management view.
    /// Implements the Master-Detail pattern (List + Form).
    /// </summary>
    public class CustomerViewModel : ObservableObject
    {
        private readonly CustomerService _customerService;
        private readonly IDialogService _dialogService;

        private Customer? _selectedCustomer;
        private Customer? _editableCustomer;

        private string _searchText = string.Empty;
        private bool _isFormVisible;
        private bool _isDeliveryAddressDifferent;
        private List<Customer> _allCustomersCache = new();
        private ObservableCollection<Customer> _customers = new();
        private readonly CountryRepository _countryRepository;

        /// <summary>
        /// Gets the collection of customers displayed in the list (Master).
        /// ObservableCollection ensures the UI updates when items are added/removed.
        /// </summary>

        public ObservableCollection<Customer> Customers
        {
            get => _customers;
            set
            {
                _customers = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the currently selected customer in the list.
        /// The detail form binds to property.
        /// </summary>
        public Customer? SelectedCustomer
        {
            get => _selectedCustomer;
            set
            {
                _selectedCustomer = value;
                OnPropertyChanged();

                if (_selectedCustomer != null)
                {
                    EditableCustomer = _selectedCustomer.Clone();

                    IsDeliveryAddressDifferent = EditableCustomer.BillingAddress?.Street != EditableCustomer.DeliveryAddress?.Street;
                    IsFormVisible = true;
                }
            }
        }

        public Customer? EditableCustomer
        {
            get => _editableCustomer;
            set
            {
                _editableCustomer = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the detail form is visible.
        /// Used by XAML view to dynamically adjust layout and column spans.
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
        /// Gets or sets the text used to filter the customer list.
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

        /// <summary>
        /// Controls the visibility of the delivery address form.
        /// Bound to the Checkbox in the UI.
        /// </summary>
        public bool IsDeliveryAddressDifferent
        {
            get => _isDeliveryAddressDifferent;
            set
            {
                _isDeliveryAddressDifferent = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// A predefined list of countries for the UI ComboBox to ensure data consistency.
        /// </summary>
        public ObservableCollection<Country> AvailableCountries { get; } = new();

        // --- Commands ---

        public ICommand CreateNewCustomerCommand { get; }
        public ICommand SaveCustomerCommand { get; }
        public ICommand CancelCommand { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomerViewModel"/> class.
        /// Sets up the initial state and configures the ICommands.
        /// </summary>
        public CustomerViewModel()
        {
            _customerService = new CustomerService();
            _dialogService = new WpfDialogService();
            _countryRepository = new CountryRepository();

            IsFormVisible = false;

            // Initialize Commands
            CreateNewCustomerCommand = new RelayCommand(ExecuteCreateNewCustomer);
            SaveCustomerCommand = new RelayCommand(async _ => await ExecuteSaveCustomerAsync());
            CancelCommand = new RelayCommand(ExecuteCancel);

            _ = LoadCustomersAsync();
        }

        // --- Execution Methods ---

        /// Prepares the UI for the creation of a new customer.
        /// </summary>
        /// <param name="parameter">Optional command parameter.</param>
        private void ExecuteCreateNewCustomer(object? parameter)
        {
            SelectedCustomer = null;

            EditableCustomer = new Customer
            {
                BillingAddress = new Address (),
                DeliveryAddress = new Address ()
            };

            IsDeliveryAddressDifferent = false;
            IsFormVisible = true;
        }

        /// <summary>
        /// Validates user input and saves the customer data.
        /// </summary>
        /// <param name="parameter">Optional command parameter.</param>
        private async Task ExecuteSaveCustomerAsync()
        {
            if (EditableCustomer == null) return;

            // 1. Validation
            if (string.IsNullOrWhiteSpace(EditableCustomer.Name))
            {
                _dialogService.ShowWarning("Bitte geben Sie einen Firmennamen ein!", "Fehlende Daten");
                return;
            }

            // Check whether the required fields for the billing address have been filled in
            if (EditableCustomer.BillingAddress == null ||
                string.IsNullOrWhiteSpace(EditableCustomer.BillingAddress.Street) ||
                string.IsNullOrWhiteSpace(EditableCustomer.BillingAddress.ZipCode) ||
                string.IsNullOrWhiteSpace(EditableCustomer.BillingAddress.City))
            {
                _dialogService.ShowWarning("Bitte füllen Sie die Pflichtfelder der Rechnungsadresse aus!", "Fehlende Daten"); 
                return;
            }

            try
            {
                // 2. Clone the shipping address 
                if (!IsDeliveryAddressDifferent)
                {
                    // If checkbox is NOT checked, we clone the billing address to the delivery address
                    EditableCustomer.DeliveryAddress = EditableCustomer.BillingAddress.Clone();
                    EditableCustomer.DeliveryAddress.Id = _selectedCustomer?.DeliveryAddressId ?? 0;
                }
                else
                {
                    // If checkbox IS checked, validate the delivery address!
                    if (string.IsNullOrWhiteSpace(EditableCustomer.DeliveryAddress!.Street) ||
                        string.IsNullOrWhiteSpace(EditableCustomer.DeliveryAddress.ZipCode) ||
                        string.IsNullOrWhiteSpace(EditableCustomer.DeliveryAddress.City))
                    {
                        _dialogService.ShowWarning("Bitte füllen Sie die Pflichtfelder der abweichenden Lieferadresse aus!", "Fehlende Daten");
                        return;
                    }
                }

                // 3. Call the service to write everything to the database
                if (EditableCustomer.Id == 0)
                {
                    await _customerService.CreateCustomerAsync(EditableCustomer);
                    _dialogService.ShowMessage($"Kunde erfolgreich angelegt!\nDie generierte Kundennummer lautet: {EditableCustomer.CustomerNumber}", "Erfolg");
                }
                else
                {
                    await _customerService.UpdateCustomerAsync(EditableCustomer);
                    _dialogService.ShowMessage("Kundenänderungen erfolgreich gespeichert!", "Update erfolgreich");
                }


                await LoadCustomersAsync();

                EditableCustomer = null;
                IsFormVisible = false;
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Fehler beim Speichern:\n{ex.Message}", "Datenbankfehler");
            }
        }

        /// <summary>
        /// Discards the current selection and hides the form.
        /// </summary>
        /// <param name="parameter">Optional command parameter.</param>
        private void ExecuteCancel(object? parameter)
        {
            // Discard and hide form
            SelectedCustomer = null;
            EditableCustomer = null;
            IsFormVisible = false;
        }


        /// <summary>
        /// Loads all customers from the database and updates the UI.
        /// </summary>
        public async Task LoadCustomersAsync()
        {
            try
            {
                var countriesFromDb = await _countryRepository.GetAllAsync();
                AvailableCountries.Clear();
                foreach (var country in countriesFromDb)
                {
                    AvailableCountries.Add(country);
                }

                _allCustomersCache = await _customerService.GetAllCustomersAsync();
                ApplySearchFilter();
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Fehler beim Laden der Daten:\n{ex.Message}", "Datenbankfehler");
            }
        }

        /// <summary>
        /// Filters the cached customer list based on the search text.
        /// </summary>
        private void ApplySearchFilter()
        {
            var filteredList = string.IsNullOrWhiteSpace(SearchText)
                ? _allCustomersCache
                : _allCustomersCache.Where(c =>
                    c.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    c.CustomerNumber.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    (c.ContactPerson != null && c.ContactPerson.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                ).ToList();

            Customers = new ObservableCollection<Customer>(filteredList);
        }
    }
}

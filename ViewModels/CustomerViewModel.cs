using DeviceServiceManager.Core;
using DeviceServiceManager.Models;
using DeviceServiceManager.Services; 
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace DeviceServiceManager.ViewModels
{
    /// <summary>
    /// Represents the logic and data for the customer management view.
    /// Implements the Master-Detail pattern (List + Form).
    /// </summary>
    public class CustomerViewModel : ObservableObject
    {

        private Customer? _selectedCustomer;
        private string _searchText = string.Empty;
        private bool _isFormVisible;
        private readonly CustomerService _customerService;
        private List<Customer> _allCustomersCache = new();
        private bool _isDeliveryAddressDifferent;

        /// <summary>
        /// Gets the collection of customers displayed in the list (Master).
        /// ObservableCollection ensures the UI updates when items are added/removed.
        /// </summary>
        public ObservableCollection<Customer> Customers { get; }

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

                // Show the detail form automatically when a customer is selected
                if (_selectedCustomer != null)
                {
                    IsDeliveryAddressDifferent = _selectedCustomer.BillingAddress?.Street != 
                        _selectedCustomer.DeliveryAddress?.Street;
                    _isFormVisible = true;
                }
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
        public List<string> AvailableCountries { get; } = new()
        {
            "Deutschland",
            "Österreich",
            "Schweiz",
            "Niederlande",
            "Belgien",
            "Frankreich"
        };

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
            Customers = new ObservableCollection<Customer>();
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
            // Initialize a clean object with empty addresses to prevent NullReferenceExceptions in the UI binding
            SelectedCustomer = new Customer
            {
                BillingAddress = new Address{Country = "Deutschland"},
                DeliveryAddress = new Address { Country = "Deutschland"}
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
            if (SelectedCustomer == null) return;

            // 1. Validation
            if (string.IsNullOrWhiteSpace(SelectedCustomer.Name))
            {
                MessageBox.Show("Bitte geben Sie einen Firmennamen ein!", "Fehlende Daten", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Check whether the required fields for the billing address have been filled in
            if (SelectedCustomer.BillingAddress == null ||
                string.IsNullOrWhiteSpace(SelectedCustomer.BillingAddress.Street) ||
                string.IsNullOrWhiteSpace(SelectedCustomer.BillingAddress.ZipCode) ||
                string.IsNullOrWhiteSpace(SelectedCustomer.BillingAddress.City))
            {
                MessageBox.Show("Bitte füllen Sie die Pflichtfelder der Rechnungsadresse (Straße, PLZ, Ort) aus!", "Fehlende Daten", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // 2. Clone the shipping address 
                if (!IsDeliveryAddressDifferent)
                {
                    // If checkbox is NOT checked, we clone the billing address to the delivery address
                    SelectedCustomer.DeliveryAddress!.Street = SelectedCustomer.BillingAddress!.Street;
                    SelectedCustomer.DeliveryAddress.HouseNumber = SelectedCustomer.BillingAddress.HouseNumber;
                    SelectedCustomer.DeliveryAddress.ZipCode = SelectedCustomer.BillingAddress.ZipCode;
                    SelectedCustomer.DeliveryAddress.City = SelectedCustomer.BillingAddress.City;
                    SelectedCustomer.DeliveryAddress.Country = SelectedCustomer.BillingAddress.Country;
                }
                else
                {
                    // If checkbox IS checked, validate the delivery address!
                    if (string.IsNullOrWhiteSpace(SelectedCustomer.DeliveryAddress!.Street) ||
                        string.IsNullOrWhiteSpace(SelectedCustomer.DeliveryAddress.ZipCode) ||
                        string.IsNullOrWhiteSpace(SelectedCustomer.DeliveryAddress.City))
                    {
                        MessageBox.Show("Bitte füllen Sie die Pflichtfelder der abweichenden Lieferadresse aus!", "Fehlende Daten", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }

                // 3. Call the service to write everything to the database
                if (SelectedCustomer.Id == 0)
                {   
                    await _customerService.CreateCustomerAsync(SelectedCustomer);
                    MessageBox.Show($"Kunde erfolgreich angelegt!\nDie generierte Kundennummer lautet: {SelectedCustomer.CustomerNumber}", "Erfolg", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {  
                    await _customerService.UpdateCustomerAsync(SelectedCustomer);
                    MessageBox.Show("Kundenänderungen erfolgreich gespeichert!", "Update erfolgreich", MessageBoxButton.OK, MessageBoxImage.Information);
                }


                await LoadCustomersAsync();

                SelectedCustomer = null;
                IsFormVisible = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Speichern in die Datenbank:\n{ex.Message}", "Datenbankfehler", MessageBoxButton.OK, MessageBoxImage.Error);
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
            IsFormVisible = false;
        }


        /// <summary>
        /// Loads all customers from the database and updates the UI.
        /// </summary>
        public async Task LoadCustomersAsync()
        {
            try
            {
                _allCustomersCache = await _customerService.GetAllCustomersAsync();
                ApplySearchFilter();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Laden der Kundenliste:\n{ex.Message}", "Datenbankfehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Filters the cached customer list based on the search text.
        /// </summary>
        private void ApplySearchFilter()
        {
            Customers.Clear();

            foreach (var customer in _allCustomersCache)
            {
                // If search is empty, show all. Otherwise check if Name, CustomerNumber or ContactPerson contains the text.
                if (string.IsNullOrWhiteSpace(SearchText) ||
                    customer.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    customer.CustomerNumber.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    (customer.ContactPerson != null && customer.ContactPerson.Contains(SearchText, StringComparison.OrdinalIgnoreCase)))
                {
                    Customers.Add(customer);
                }
            }
        }
    }
}

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
                // We call the search logic here later
            }
        }

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
                BillingAddress = new Address(),
                DeliveryAddress = new Address()
            };
            IsFormVisible = true;
        }

        /// <summary>
        /// Validates user input and saves the customer data.
        /// </summary>
        /// <param name="parameter">Optional command parameter.</param>
        private async Task ExecuteSaveCustomerAsync()
        {
            if (SelectedCustomer == null) return;

            // 1. Validierung (Kundennummer haben wir entfernt, also prüfen wir nur noch den Firmennamen)
            if (string.IsNullOrWhiteSpace(SelectedCustomer.Name))
            {
                MessageBox.Show("Bitte geben Sie einen Firmennamen ein!", "Fehlende Daten", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Prüfen, ob die Pflichtfelder der Rechnungsadresse ausgefüllt sind
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
                // 2. Lieferadresse klonen (Der Trick!)
                // Wir erzeugen ein neues Adress-Objekt mit den exakten Werten der Rechnungsadresse
                SelectedCustomer.DeliveryAddress = new Address
                {
                    Street = SelectedCustomer.BillingAddress.Street,
                    HouseNumber = SelectedCustomer.BillingAddress.HouseNumber,
                    ZipCode = SelectedCustomer.BillingAddress.ZipCode,
                    City = SelectedCustomer.BillingAddress.City,
                    Country = SelectedCustomer.BillingAddress.Country
                };

                // 3. Den Service aufrufen, um alles in die Datenbank zu schreiben
                await _customerService.CreateCustomerAsync(SelectedCustomer);

                // 4. Erfolgsmeldung und Aufräumen
                MessageBox.Show($"Kunde erfolgreich angelegt!\nDie generierte Kundennummer lautet: {SelectedCustomer.CustomerNumber}",
                                "Erfolg", MessageBoxButton.OK, MessageBoxImage.Information);

                // Später: Kundenliste aktualisieren
                // await LoadCustomersAsync();

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
    }
}

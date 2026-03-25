using DeviceServiceManager.Core;
using DeviceServiceManager.Models;
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

        private Customer? _selectedCustomer;
        private string _searchText = string.Empty;
        private bool _isFormVisible;

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
            Customers = new ObservableCollection<Customer>();
            IsFormVisible = false;

            // Initialize Commands
            CreateNewCustomerCommand = new RelayCommand(ExecuteCreateNewCustomer);
            SaveCustomerCommand = new RelayCommand(ExecuteSaveCustomer);
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
        private void ExecuteSaveCustomer(object? parameter)
        {
            // Basic validation: Check for null or empty required strings
            if (SelectedCustomer == null || string.IsNullOrWhiteSpace(SelectedCustomer.CustomerNumber)
                || string.IsNullOrWhiteSpace(SelectedCustomer.Name))
            {
                System.Windows.MessageBox.Show("Bitte füllen Sie alle Pflichtfelder aus.",
                                               "Fehlende Daten", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return;
            }

            // TODO: Call CustomerService to execute DB Insert or Update

            System.Windows.MessageBox.Show("Kunde wird gespeichert.", "Erfolg", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);

            // Hide form after successful save
            IsFormVisible = false;
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

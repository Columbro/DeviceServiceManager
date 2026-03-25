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

                if (_selectedCustomer != null)
                {
                    _isFormVisible = true;
                }
            }
        }

        /// <summary>
        /// Gets of sets the visibility of the formular.
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
        private void ExecuteCreateNewCustomer(object? parameter)
        {
            // Creates an empty customer object (including an empty address) that populates the form
            SelectedCustomer = new Customer
            {
                BillingAddress = new Address(),
                DeliveryAddress = new Address()
            };
            IsFormVisible = true;
        }

        private void ExecuteSaveCustomer(object? parameter)
        {
            // Validation: If required fields are left blank, the process is terminated here and a message is displayed
            if (SelectedCustomer == null || string.IsNullOrWhiteSpace(SelectedCustomer.CustomerNumber) 
                || string.IsNullOrWhiteSpace(SelectedCustomer.Name))
            {
                System.Windows.MessageBox.Show("Bitte füllen Sie alle Pflichtfelder aus.",
                                               "Fehlende Daten", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return;
            }

            // Später: DB Insert oder Update
            System.Windows.MessageBox.Show("Kunde wird gespeichert...", "Erfolg", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);

            // Nach dem Speichern Formular wieder schließen
            IsFormVisible = false;
        }

        private void ExecuteCancel(object? parameter)
        {
            // Discard and hide form
            SelectedCustomer = null;
            IsFormVisible = false;
        }
    }
}

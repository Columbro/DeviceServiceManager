using DeviceServiceManager.Core;
using DeviceServiceManager.Models;
using DeviceServiceManager.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows;

namespace DeviceServiceManager.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        private readonly AddressService _addressService;

        // ObservableCollection notifies the UI when items are added or removed
        public ObservableCollection<Address> Addresses { get; set; }

        // Commands for the UI Buttons
        public ICommand LoadAddressesCommand { get; }
        public ICommand AddTestAddressCommand { get; }

        public MainViewModel()
        {
            _addressService = new AddressService();
            Addresses = new ObservableCollection<Address>();

            // Initialize Commands
            LoadAddressesCommand = new RelayCommand(async _ => await LoadAddressesAsync());
            AddTestAddressCommand = new RelayCommand(async _ => await AddTestAddressAsync());
        }

        private async Task LoadAddressesAsync()
        {
            try
            {
                var loadedAddresses = await _addressService.GetAllAddressesAsync();

                Addresses.Clear();
                foreach (var address in loadedAddresses)
                {
                    Addresses.Add(address);
                }
            }
            catch (Exception ex)
            {
                // Here we would typically show a MessageBox or log the error
                MessageBox.Show($"Datenbank-Fehler:\n{ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task AddTestAddressAsync()
        {
            var newAddress = new Address
            {
                Street = "Musterstraße",
                HouseNumber = "42a",
                ZipCode = "12345",
                City = "Teststadt",
                Country = "Deutschland"
            };

            try
            {
                await _addressService.CreateAddressAsync(newAddress);
                // Reload the list to show the new address
                await LoadAddressesAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Datenbank-Fehler:\n{ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}

using DeviceServiceManager.Models;
using DeviceServiceManager.Repositories;

namespace DeviceServiceManager.Services
{
    public class AddressService
    {
        private readonly AddressRepository _repository;

        public AddressService()
        {
            _repository = new AddressRepository();
        }

        /// <summary>
        /// Gets all addresses from the repository.
        /// </summary>
        public async Task<List<Address>> GetAllAddressesAsync()
        {
            // Business logic could be added here later (e.g., logging, filtering)
            return await _repository.GetAllAsync();
        }

        /// <summary>
        /// Validates and adds a new address.
        /// </summary>
        public async Task<int> CreateAddressAsync(Address address)
        {
            // Basic validation example
            if (string.IsNullOrWhiteSpace(address.Street) || string.IsNullOrWhiteSpace(address.City))
            {
                throw new ArgumentException("Straße und Stadt sind notwendig.");
            }

            return await _repository.AddAsync(address);
        }
    }
}

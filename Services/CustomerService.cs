using DeviceServiceManager.Models;
using DeviceServiceManager.Repositories;
using System;
using System.Threading.Tasks;

namespace DeviceServiceManager.Services
{
    /// <summary>
    /// Encapsulates the business logic for customer management.
    /// Orchestrates interactions between different repositories.
    /// </summary>
    public class CustomerService
    {
        private readonly CustomerRepository _customerRepository;
        private readonly AddressRepository _addressRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomerService"/> class.
        /// </summary>
        public CustomerService()
        {
            _customerRepository = new CustomerRepository();
            _addressRepository = new AddressRepository();
        }

        /// <summary>
        /// Generates the next available customer number.
        /// Starts at 1000 if no customers exist in the database.
        /// </summary>
        /// <returns>A unique customer number as a string.</returns>
        private async Task<string> GenerateNextCustomerNumberAsync()
        {
            int currentMax = await _customerRepository.GetMaxCustomerNumberAsync();
            int nextNumber = currentMax + 1;

            return nextNumber.ToString();
        }

        /// <summary>
        /// Validates, prepares, and saves a new customer along with their addresses to the database.
        /// </summary>
        /// <param name="customer">The customer object containing billing and delivery addresses.</param>
        public async Task CreateCustomerAsync(Customer customer)
        {
            if (customer.BillingAddress == null || customer.DeliveryAddress == null)
            {
                throw new ArgumentException("Rechnungs- und Lieferadresse müssen angegeben werden.");
            }

            // 1. Generate the automated Customer Number
            customer.CustomerNumber = await GenerateNextCustomerNumberAsync();

            // 2. Save Addresses to DB and retrieve their newly generated IDs
            int billingId = await _addressRepository.AddAsync(customer.BillingAddress);
            int deliveryId = await _addressRepository.AddAsync(customer.DeliveryAddress);

            // 3. Assign the generated Address IDs to the Customer object
            customer.BillingAddressId = billingId;
            customer.DeliveryAddressId = deliveryId;

            // 4. Save the Customer to the DB
            customer.Id = await _customerRepository.AddAsync(customer);
        }
    }
}

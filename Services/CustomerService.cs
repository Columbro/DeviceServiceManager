using DeviceServiceManager.Models;
using DeviceServiceManager.Repositories;
using DeviceServiceManager.Data;
using MySqlConnector;

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
        /// Validates, prepares, and saves a new customer along with their addresses to the database.
        /// Prevents orphaned addresses and race conditions.
        /// </summary>
        /// <param name="customer">The customer object containing billing and delivery addresses.</param>
        public async Task CreateCustomerAsync(Customer customer)
        {
            if (customer.BillingAddress == null || customer.DeliveryAddress == null)
            {
                throw new ArgumentException("Billing and Delivery addresses must be provided.");
            }

            string connectionString = DatabaseConfig.GetConnectionString();

            using (var connection = new MySqlConnection(connectionString))
            {
                await connection.OpenAsync();

                // START TRANSACTION
                using (var transaction = await connection.BeginTransactionAsync())
                {
                    try
                    {
                        // 1. Race Condition Prevention: Generate number inside the transaction (locked)
                        int currentMax = await _customerRepository.GetMaxCustomerNumberAsync(connection, transaction);
                        customer.CustomerNumber = (currentMax + 1).ToString();

                        // 2. Save Addresses 
                        int billingId = await _addressRepository.AddAsync(customer.BillingAddress, connection, transaction);
                        int deliveryId = await _addressRepository.AddAsync(customer.DeliveryAddress, connection, transaction);

                        customer.BillingAddressId = billingId;
                        customer.DeliveryAddressId = deliveryId;

                        // 3. Save the Customer
                        customer.Id = await _customerRepository.AddAsync(customer, connection, transaction);

                        // COMMIT TRANSACTION: If we reach this line, everything worked! Save it permanently.
                        await transaction.CommitAsync();
                    }
                    catch (Exception ex)
                    {
                        // ROLLBACK: Something failed. Delete the addresses we just created and abort!
                        await transaction.RollbackAsync();

                        // Pass the error up to the ViewModel to show the MessageBox
                        throw new Exception($"Transaction failed and was rolled back. Reason: {ex.Message}", ex);
                    }
                }
            }
        }

        /// <summary>
        /// Retrieves a complete list of all customers from the repository.
        /// </summary>
        public async Task<List<Customer>> GetAllCustomersAsync()
        {
            return await _customerRepository.GetAllAsync();
        }

        /// <summary>
        /// Updates an existing customer and their addresses using a database transaction.
        /// </summary>
        public async Task UpdateCustomerAsync(Customer customer)
        {
            if (customer.BillingAddress == null || customer.DeliveryAddress == null)
            {
                throw new ArgumentException("Billing and Delivery addresses must be provided.");
            }

            string connectionString = DatabaseConfig.GetConnectionString();

            using (var connection = new MySqlConnection(connectionString))
            {
                await connection.OpenAsync();

                // START TRANSACTION
                using (var transaction = await connection.BeginTransactionAsync())
                {
                    try
                    {
                        // 1. Update the addresses first
                        await _addressRepository.UpdateAsync(customer.BillingAddress, connection, transaction);
                        await _addressRepository.UpdateAsync(customer.DeliveryAddress, connection, transaction);

                        // 2. Update the customer details
                        await _customerRepository.UpdateAsync(customer, connection, transaction);

                        // COMMIT
                        await transaction.CommitAsync();
                    }
                    catch (Exception ex)
                    {
                        // ROLLBACK on failure
                        await transaction.RollbackAsync();
                        throw new Exception($"Update failed and was rolled back. Reason: {ex.Message}", ex);
                    }
                }
            }
        }
    }
}

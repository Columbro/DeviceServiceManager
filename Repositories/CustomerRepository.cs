using System;
using DeviceServiceManager.Data;
using DeviceServiceManager.Models;
using MySqlConnector;
using System.Threading.Tasks;

namespace DeviceServiceManager.Repositories
{
    /// <summary>
    /// Handles direct databse operations for the Customer entity.
    /// </summary>
    public class CustomerRepository
    {
        private readonly string _connectionString;

        /// <summary>
        /// Initializes a new instace of the <see cref="CustomerRepository"/> class.
        /// </summary>
        public CustomerRepository()
        {
            _connectionString = DatabaseConfig.GetConnectionString();
        }

        /// <summary>
        /// Retrieves the highest existing customer number from the database.
        /// </summary>
        /// <returns>The highest customer number as an integer, or 999 if no customers exist.</returns>
        public async Task<int> GetMaxCustomerNumberAsync()
        {
            // CAST ensures that we sort mathematically, not alphabetically (string)
            string query = "SELECT MAX(CAST(kundennummer AS UNSIGNED)) FROM kunden";

            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new MySqlCommand(query, connection))
                {
                    var result = await command.ExecuteScalarAsync();

                    // If DB is empty, DBNull is returned
                    if (result == DBNull.Value || result == null)
                    {
                        return 999; // So the next generated number will be 1000
                    }

                    return Convert.ToInt32(result);
                }
            }
        }


        /// <summary>
        /// Inserts a new customer into the database.
        /// </summary>
        /// <param name="customer">The customer object to insert.</param>
        /// <returns>The auto-generated database ID of the new customer.</returns>
        public async Task<int> AddAsync(Customer customer)
        {
            string query = @"INSERT INTO kunden 
                            (kundennummer, name, ansprechpartner, email, telefon, rechnungsadresse_id, lieferadresse_id) 
                            VALUES 
                            (@CustomerNumber, @Name, @ContactPerson, @Email, @Phone, @BillingId, @DeliveryId);
                            SELECT LAST_INSERT_ID();";

            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@CustomerNumber", customer.CustomerNumber);
                    command.Parameters.AddWithValue("@Name", customer.Name);

                    // Handle nullable fields correctly for SQL
                    command.Parameters.AddWithValue("@ContactPerson", (object?)customer.ContactPerson ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Email", (object?)customer.Email ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Phone", (object?)customer.Phone ?? DBNull.Value);

                    command.Parameters.AddWithValue("@BillingId", customer.BillingAddressId);
                    command.Parameters.AddWithValue("@DeliveryId", customer.DeliveryAddressId);

                    var result = await command.ExecuteScalarAsync();
                    return Convert.ToInt32(result);
                }
            }
        }
    }
}

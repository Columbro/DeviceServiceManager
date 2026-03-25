using DeviceServiceManager.Data;
using DeviceServiceManager.Models;
using MySqlConnector;

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
        public async Task<int> GetMaxCustomerNumberAsync(MySqlConnection connection, MySqlTransaction transaction)
        {
            // CAST ensures that we sort mathematically, not alphabetically (string)
            // FOR UPDATE locks the row/table to prevent Race Conditions until the transaction finishes!
            string query = "SELECT MAX(CAST(kundennummer AS UNSIGNED)) FROM kunden FOR UPDATE";

            using (var command = new MySqlCommand(query, connection, transaction))
            {
                var result = await command.ExecuteScalarAsync();

                if (result == DBNull.Value ||result == null)
                {
                    return 999;
                }
                return Convert.ToInt32(result);
            }
        }


        /// <summary>
        /// Inserts a new customer into the database.
        /// </summary>
        /// <param name="customer">The customer object to insert.</param>
        /// <returns>The auto-generated database ID of the new customer.</returns>
        public async Task<int> AddAsync(Customer customer, MySqlConnection connection, MySqlTransaction transaction)
        {
            string query = @"INSERT INTO kunden 
                            (kundennummer, name, ansprechpartner, email, telefon, rechnungsadresse_id, lieferadresse_id) 
                            VALUES 
                            (@CustomerNumber, @Name, @ContactPerson, @Email, @Phone, @BillingId, @DeliveryId);
                            SELECT LAST_INSERT_ID();";

            try
            {
                using (var command = new MySqlCommand(query, connection, transaction))
                {
                    command.Parameters.Add("@CustomerNumber", MySqlDbType.VarChar, 20).Value = customer.CustomerNumber;
                    command.Parameters.Add("@Name", MySqlDbType.VarChar, 150).Value = customer.Name;

                    command.Parameters.Add("@ContactPerson", MySqlDbType.VarChar, 100).Value = (object?)customer.ContactPerson ?? DBNull.Value;
                    command.Parameters.Add("@Email", MySqlDbType.VarChar, 150).Value = (object?)customer.Email ?? DBNull.Value;
                    command.Parameters.Add("@Phone", MySqlDbType.VarChar, 50).Value = (object?)customer.Phone ?? DBNull.Value;

                    command.Parameters.Add("@BillingId", MySqlDbType.Int32).Value = customer.BillingAddressId;
                    command.Parameters.Add("@DeliveryId", MySqlDbType.Int32).Value = customer.DeliveryAddressId;

                    var result = await command.ExecuteScalarAsync();
                    return Convert.ToInt32(result);
                }
            }
            catch (MySqlException ex)
            {
                // If the UNIQUE constraint of Kundennummer is violated, MySQL throws Error 1062
                if (ex.Number == 1062)
                {
                    throw new Exception("A customer with this number already exists. Race condition prevented.", ex);
                }
                throw new Exception("Database error occurred while inserting the customer.", ex);
            }
        }
    }
}

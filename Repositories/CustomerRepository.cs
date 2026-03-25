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

        /// <summary>
        /// Retrieves all customers including their billing and delivery addresses from the database.
        /// Uses INNER JOINs to fetch all related data in a single query (Eager Loading).
        /// </summary>
        /// <returns>A list of fully populated Customer objects.</returns>
        public async Task<List<Customer>> GetAllAsync()
        {
            var customers = new List<Customer>();

            // The connection string needs to be fetched from the config
            string connectionString = DatabaseConfig.GetConnectionString();

            // SQL Query with ALIASES (k, r, l) to separate the columns clearly
            string query = @"
                SELECT 
                    k.id, k.kundennummer, k.name, k.ansprechpartner, k.email, k.telefon, k.erstellt_am,
                    k.rechnungsadresse_id, k.lieferadresse_id,
                    r.id AS r_id, r.strasse AS r_strasse, r.hausnummer AS r_hausnummer, r.plz AS r_plz, r.ort AS r_ort, r.land AS r_land,
                    l.id AS l_id, l.strasse AS l_strasse, l.hausnummer AS l_hausnummer, l.plz AS l_plz, l.ort AS l_ort, l.land AS l_land
                FROM kunden k
                INNER JOIN adressen r ON k.rechnungsadresse_id = r.id
                INNER JOIN adressen l ON k.lieferadresse_id = l.id
                ORDER BY k.kundennummer ASC;";

            using (var connection = new MySqlConnection(connectionString))
            {
                await connection.OpenAsync();
                using (var command = new MySqlCommand(query, connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var customer = new Customer
                        {
                            Id = reader.GetInt32("id"),
                            CustomerNumber = reader.GetString("kundennummer"),
                            Name = reader.GetString("name"),

                            // Handling nullable fields
                            ContactPerson = reader.IsDBNull(reader.GetOrdinal("ansprechpartner")) ? null : reader.GetString("ansprechpartner"),
                            Email = reader.IsDBNull(reader.GetOrdinal("email")) ? null : reader.GetString("email"),
                            Phone = reader.IsDBNull(reader.GetOrdinal("telefon")) ? null : reader.GetString("telefon"),

                            CreatedAt = reader.GetDateTime("erstellt_am"),
                            BillingAddressId = reader.GetInt32("rechnungsadresse_id"),
                            DeliveryAddressId = reader.GetInt32("lieferadresse_id"),

                            // Map the Billing Address
                            BillingAddress = new Address
                            {
                                Id = reader.GetInt32("r_id"),
                                Street = reader.GetString("r_strasse"),
                                HouseNumber = reader.GetString("r_hausnummer"),
                                ZipCode = reader.GetString("r_plz"),
                                City = reader.GetString("r_ort"),
                                Country = reader.GetString("r_land")
                            },

                            // Map the Delivery Address
                            DeliveryAddress = new Address
                            {
                                Id = reader.GetInt32("l_id"),
                                Street = reader.GetString("l_strasse"),
                                HouseNumber = reader.GetString("l_hausnummer"),
                                ZipCode = reader.GetString("l_plz"),
                                City = reader.GetString("l_ort"),
                                Country = reader.GetString("l_land")
                            }
                        };

                        customers.Add(customer);
                    }
                }
            }

            return customers;
        }
    }
}

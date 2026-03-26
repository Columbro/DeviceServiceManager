using DeviceServiceManager.Data;
using DeviceServiceManager.Models;
using MySqlConnector;

namespace DeviceServiceManager.Repositories
{
    /// <summary>
    /// Handles direct database operations for the Address entity.
    /// </summary>
    public class AddressRepository
    {
        private readonly string _connectionString;

        /// <summary>
        /// Initializes a new instance of the <see cref="AddressRepository"/> class.
        /// </summary>
        public AddressRepository()
        {
            _connectionString = DatabaseConfig.GetConnectionString();
        }

        /// <summary>
        /// Inserts a new address into the database.
        /// </summary>
        /// <param name="address">The address object to insert.</param>
        /// <returns>The auto-generated database ID of the new address.</returns>
        public async Task<int> AddAsync(Address address, MySqlConnection connection, MySqlTransaction transaction)
        {
            string query = @"INSERT INTO adressen (strasse, hausnummer, plz, ort, land) 
                             VALUES (@Street, @HouseNumber, @ZipCode, @City, @Country);
                             SELECT LAST_INSERT_ID();";

            try
            {
                using (var command = new MySqlCommand(query, connection, transaction))
                {
                    // EXPLICIT TYPING instead of AddWithValue (Best Practice)
                    command.Parameters.Add("@Street", MySqlDbType.VarChar, 150).Value = address.Street;
                    command.Parameters.Add("@HouseNumber", MySqlDbType.VarChar, 10).Value = address.HouseNumber;
                    command.Parameters.Add("@ZipCode", MySqlDbType.VarChar, 10).Value = address.ZipCode;
                    command.Parameters.Add("@City", MySqlDbType.VarChar, 100).Value = address.City;

                    string country = string.IsNullOrWhiteSpace(address.Country) ? "Deutschland" : address.Country;
                    command.Parameters.Add("@Country", MySqlDbType.VarChar, 50).Value = country;

                    var result = await command.ExecuteScalarAsync();
                    return Convert.ToInt32(result);
                }
            }
            catch (MySqlException ex)
            {
                // Wrap and throw to trigger the Rollback in the Service
                throw new Exception("Database error occurred while inserting the address.", ex);
            }
        }

        /// <summary>
        /// Updates an existing address in the database.
        /// </summary>
        public async Task UpdateAsync(Address address, MySqlConnection connection, MySqlTransaction transaction)
        {
            string query = @"UPDATE adressen 
                             SET strasse = @Street, hausnummer = @HouseNumber, 
                                 plz = @ZipCode, ort = @City, land = @Country
                             WHERE id = @Id;";

            try
            {
                using (var command = new MySqlCommand(query, connection, transaction))
                {
                    command.Parameters.Add("@Id", MySqlDbType.Int32).Value = address.Id;
                    command.Parameters.Add("@Street", MySqlDbType.VarChar, 150).Value = address.Street;
                    command.Parameters.Add("@HouseNumber", MySqlDbType.VarChar, 10).Value = address.HouseNumber;
                    command.Parameters.Add("@ZipCode", MySqlDbType.VarChar, 10).Value = address.ZipCode;
                    command.Parameters.Add("@City", MySqlDbType.VarChar, 100).Value = address.City;

                    string country = string.IsNullOrWhiteSpace(address.Country) ? "Deutschland" : address.Country;
                    command.Parameters.Add("@Country", MySqlDbType.VarChar, 50).Value = country;

                    await command.ExecuteNonQueryAsync();
                }
            }
            catch (MySqlException ex)
            {
                throw new Exception("Database error occurred while updating the address.", ex);
            }
        }
    }
}
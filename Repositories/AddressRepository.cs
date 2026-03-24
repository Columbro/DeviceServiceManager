using DeviceServiceManager.Data;
using DeviceServiceManager.Models;
using MySqlConnector;

namespace DeviceServiceManager.Repositories
{
    public class AddressRepository
    {
        private readonly string _connectionString;

        public AddressRepository()
        {
            _connectionString = DatabaseConfig.GetConnectionString();
        }

        /// <summary>
        /// Retrieves all addresses from the database asynchronously.
        /// <returns> The list of all addresses</returns>
        /// </summary>
        public async Task<List<Address>> GetAllAsync()
        {
            var addresses = new List<Address>();
            string query = "SELECT id, strasse, hausnummer, plz, ort, land, erstellt_am FROM adressen";

            // Using statement ensures the connection is closed and disposed automatically
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var command = new MySqlCommand(query, connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        addresses.Add(new Address
                        {
                            Id = reader.GetInt32("id"),
                            Street = reader.GetString("strasse"),
                            HouseNumber = reader.GetString("hausnummer"),
                            ZipCode = reader.GetString("plz"),
                            City = reader.GetString("ort"),
                            Country = reader.GetString("land"),
                            CreatedAt = reader.GetDateTime("erstellt_am")
                        });
                    }
                }
            }

            return addresses;
        }

        /// <summary>
        /// Inserts a new address into the database and returns the generated ID.
        /// </summary>
        public async Task<int> AddAsync(Address address)
        {
            // We use parameters (@Street, etc.) to prevent SQL Injection attacks
            string query = @"INSERT INTO adressen (strasse, hausnummer, plz, ort, land) 
                             VALUES (@Street, @HouseNumber, @ZipCode, @City, @Country);
                             SELECT LAST_INSERT_ID();";

            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Street", address.Street);
                    command.Parameters.AddWithValue("@HouseNumber", address.HouseNumber);
                    command.Parameters.AddWithValue("@ZipCode", address.ZipCode);
                    command.Parameters.AddWithValue("@City", address.City);
                    command.Parameters.AddWithValue("@Country", address.Country);

                    // ExecuteScalar returns the first column of the first row (the new ID)
                    var result = await command.ExecuteScalarAsync();
                    return Convert.ToInt32(result);
                }
            }
        }
    }
}

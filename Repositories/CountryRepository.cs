using DeviceServiceManager.Data;
using DeviceServiceManager.Models;
using MySqlConnector;

namespace DeviceServiceManager.Repositories
{
    public class CountryRepository
    {
        private readonly string _connectionString;

        public CountryRepository()
        {
            _connectionString = DatabaseConfig.GetConnectionString();
        }

        public async Task<List<Country>> GetAllAsync()
        {
            var countries = new List<Country>();
            string query = "SELECT id, name FROM laender ORDER BY name ASC;";

            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new MySqlCommand(query, connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        countries.Add(new Country
                        {
                            Id = reader.GetInt32("id"),
                            Name = reader.GetString("name")
                        });
                    }
                }
            }
            return countries;
        }
    }
}
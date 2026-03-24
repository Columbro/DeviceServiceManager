using Microsoft.Extensions.Configuration;

namespace DeviceServiceManager.Data
{
    public static class DatabaseConfig
    {
        // Holds the loaded configuration
        private static IConfigurationRoot? _configuration;

        /// <summary>
        /// Retrieves the database connection string from the appsettings.json file.
        /// </summary>
        /// <returns>The connection string or throws an exception if not found.</returns>
        public static string GetConnectionString()
        {
            if (_configuration == null)
            {
                // Build the configuration by pointing to the current directory and loading the JSON file
                var builder = new ConfigurationBuilder()
                    .SetBasePath(AppContext.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

                _configuration = builder.Build();
            }

            // Retrieve the connection string named "DefaultConnection"
            string? connectionString = _configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("Connection string 'DefaultConnection' was not found in appsettings.json.");
            }

            return connectionString;
        }
    }
}

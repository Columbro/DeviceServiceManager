using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceServiceManager.Core
{
    public static class DatabaseConfig
    {
        // Details for the local or company server
        // Server=localhost; User ID=root; Password=yourpassword; Database=DeviceServiceManager;
        public static string ConnectionString { get; } =
            "Server=localhost;User ID=root;Password=;Database=DeviceServiceManager;Allow User Variables=True";
    }
}

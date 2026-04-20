using DeviceServiceManager.Data;
using DeviceServiceManager.Models;
using MySqlConnector;

namespace DeviceServiceManager.Repositories
{
    /// <summary>
    /// Handles direct database operations for Devices (Geräte).
    /// </summary>
    public class DeviceRepository
    {
        private readonly string _connectionString;

        public DeviceRepository()
        {
            _connectionString = DatabaseConfig.GetConnectionString();
        }

        /// <summary>
        /// Inserts a new device into the database and links it to a contract.
        /// Executed within the current transaction scope.
        /// </summary>
        public async Task<int> AddAsync(Device device, MySqlConnection connection, MySqlTransaction transaction)
        {
            string query = @"INSERT INTO geraete 
                            (seriennummer, hersteller, typ, wartungsvertrag_id, status) 
                            VALUES 
                            (@SerialNumber, @Manufacturer, @Type, @ContractId, @Status);
                            SELECT LAST_INSERT_ID();";

            try
            {
                using (var command = new MySqlCommand(query, connection, transaction))
                {
                    command.Parameters.Add("@SerialNumber", MySqlDbType.VarChar, 100).Value = device.SerialNumber;
                    command.Parameters.Add("@Manufacturer", MySqlDbType.VarChar, 100).Value = device.Manufacturer;
                    command.Parameters.Add("@Type", MySqlDbType.VarChar, 100).Value = (object?)device.Type ?? DBNull.Value;
                    command.Parameters.Add("@ContractId", MySqlDbType.Int32).Value = device.MaintenanceContractId;
                    command.Parameters.Add("@Status", MySqlDbType.VarChar, 20).Value = device.Status;

                    var result = await command.ExecuteScalarAsync();
                    return Convert.ToInt32(result);
                }
            }
            catch (MySqlException ex)
            {
                if (ex.Number == 1062) // UNIQUE Constraint
                {
                    throw new Exception($"Die Seriennummer '{device.SerialNumber}' existiert bereits im System.", ex);
                }
                throw new Exception($"Datenbankfehler beim Speichern des Geräts (S/N: {device.SerialNumber}): {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Deletes ALL devices associated with a specific contract.
        /// Used for the Bulk-Replace-Pattern during contract updates.
        /// </summary>
        public async Task DeleteByContractIdAsync(int contractId, MySqlConnection connection, MySqlTransaction transaction)
        {
            string query = "DELETE FROM geraete WHERE wartungsvertrag_id = @ContractId;";

            try
            {
                using (var command = new MySqlCommand(query, connection, transaction))
                {
                    command.Parameters.Add("@ContractId", MySqlDbType.Int32).Value = contractId;
                    await command.ExecuteNonQueryAsync();
                }
            }
            catch (MySqlException ex)
            {
                throw new Exception("Database error occurred while clearing old devices.", ex);
            }
        }
    }
}
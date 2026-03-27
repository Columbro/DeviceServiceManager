using DeviceServiceManager.Data;
using DeviceServiceManager.Models;
using MySqlConnector;

namespace DeviceServiceManager.Repositories
{
    /// <summary>
    /// Handles direct database operations for Maintenance Contracts (Wartungsverträge).
    /// </summary>
    public class ContractRepository
    {
        private readonly string _connectionString;

        public ContractRepository()
        {
            _connectionString = DatabaseConfig.GetConnectionString();
        }

        /// <summary>
        /// Retrieves all maintenance contracts, including the associated customer data via INNER JOIN.
        /// </summary>
        /// <returns>A list of fully populated MaintenanceContract objects.</returns>
        public async Task<List<MaintenanceContract>> GetAllAsync()
        {
            var contracts = new List<MaintenanceContract>();

            // SQL Query: We join 'wartungsvertraege' (w) with 'kunden' (k) to get the customer's name
            string query = @"
                SELECT 
                    w.id, w.vertragsnummer, w.kunde_id, w.start_datum, w.end_datum, w.status, w.bemerkung,
                    k.kundennummer, k.name AS kunden_name
                FROM wartungsvertraege w
                INNER JOIN kunden k ON w.kunde_id = k.id
                ORDER BY w.end_datum ASC;"; // Ordering by end date is crucial for 'expiring soon' overviews

            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new MySqlCommand(query, connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var contract = new MaintenanceContract
                        {
                            Id = reader.GetInt32("id"),
                            ContractNumber = reader.GetString("vertragsnummer"),
                            CustomerId = reader.GetInt32("kunde_id"),
                            StartDate = reader.GetDateTime("start_datum"),
                            EndDate = reader.GetDateTime("end_datum"),
                            Status = reader.GetString("status"),
                            Remarks = reader.IsDBNull(reader.GetOrdinal("bemerkung")) ? null : reader.GetString("bemerkung"),

                            // We map the Customer object immediately so the UI can display the Name
                            Customer = new Customer
                            {
                                Id = reader.GetInt32("kunde_id"),
                                CustomerNumber = reader.GetString("kundennummer"),
                                Name = reader.GetString("kunden_name")
                            }
                        };

                        contracts.Add(contract);
                    }
                }
            }

            return contracts;
        }

        /// <summary>
        /// Inserts a new maintenance contract into the database.
        /// </summary>
        public async Task<int> AddAsync(MaintenanceContract contract, MySqlConnection connection, MySqlTransaction transaction)
        {
            string query = @"INSERT INTO wartungsvertraege 
                            (vertragsnummer, kunde_id, start_datum, end_datum, status, bemerkung) 
                            VALUES 
                            (@ContractNumber, @CustomerId, @StartDate, @EndDate, @Status, @Remarks);
                            SELECT LAST_INSERT_ID();";

            try
            {
                using (var command = new MySqlCommand(query, connection, transaction))
                {
                    command.Parameters.Add("@ContractNumber", MySqlDbType.VarChar, 50).Value = contract.ContractNumber;
                    command.Parameters.Add("@CustomerId", MySqlDbType.Int32).Value = contract.CustomerId;
                    command.Parameters.Add("@StartDate", MySqlDbType.Date).Value = contract.StartDate;
                    command.Parameters.Add("@EndDate", MySqlDbType.Date).Value = contract.EndDate;
                    command.Parameters.Add("@Status", MySqlDbType.VarChar, 20).Value = contract.Status;

                    command.Parameters.Add("@Remarks", MySqlDbType.Text).Value = (object?)contract.Remarks ?? DBNull.Value;

                    var result = await command.ExecuteScalarAsync();
                    return Convert.ToInt32(result);
                }
            }
            catch (MySqlException ex)
            {
                if (ex.Number == 1062) // UNIQUE constraint violation
                {
                    throw new Exception("Eine Vertragsnummer mit diesem Wert existiert bereits.", ex);
                }
                throw new Exception("Database error occurred while inserting the contract.", ex);
            }
        }

        /// <summary>
        /// Updates an existing maintenance contract in the database.
        /// </summary>
        public async Task UpdateAsync(MaintenanceContract contract, MySqlConnection connection, MySqlTransaction transaction)
        {
            string query = @"UPDATE wartungsvertraege 
                             SET kunde_id = @CustomerId, start_datum = @StartDate, 
                                 end_datum = @EndDate, status = @Status, bemerkung = @Remarks
                             WHERE id = @Id;";

            try
            {
                using (var command = new MySqlCommand(query, connection, transaction))
                {
                    command.Parameters.Add("@Id", MySqlDbType.Int32).Value = contract.Id;
                    command.Parameters.Add("@CustomerId", MySqlDbType.Int32).Value = contract.CustomerId;
                    command.Parameters.Add("@StartDate", MySqlDbType.Date).Value = contract.StartDate;
                    command.Parameters.Add("@EndDate", MySqlDbType.Date).Value = contract.EndDate;
                    command.Parameters.Add("@Status", MySqlDbType.VarChar, 20).Value = contract.Status;

                    command.Parameters.Add("@Remarks", MySqlDbType.Text).Value = (object?)contract.Remarks ?? DBNull.Value;

                    await command.ExecuteNonQueryAsync();
                }
            }
            catch (MySqlException ex)
            {
                throw new Exception("Database error occurred while updating the contract.", ex);
            }
        }
    }
}
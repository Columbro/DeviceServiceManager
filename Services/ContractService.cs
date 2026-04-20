using DeviceServiceManager.Data;
using DeviceServiceManager.Models;
using DeviceServiceManager.Repositories;
using MySqlConnector;

namespace DeviceServiceManager.Services
{
    /// <summary>
    /// Encapsulates the business logic for maintenance contracts.
    /// Handles transactions and orchestrates repository calls.
    /// </summary>
    public class ContractService
    {
        private readonly ContractRepository _contractRepository;
        private readonly DeviceRepository _deviceRepository;

        public ContractService()
        {
            _contractRepository = new ContractRepository();
            _deviceRepository = new DeviceRepository();
        }

        /// <summary>
        /// Retrieves all maintenance contracts with their associated customer data.
        /// </summary>
        public async Task<List<MaintenanceContract>> GetAllContractsAsync()
        {
            return await _contractRepository.GetAllAsync();
        }

        /// <summary>
        /// Upserts (Inserts or Updates) a maintenance contract using a database transaction.
        /// </summary>
        public async Task SaveContractAsync(MaintenanceContract contract)
        {
            if (contract.CustomerId <= 0)
            {
                throw new ArgumentException("Der Vertrag muss zwingend einem Kunden zugeordnet werden.");
            }

            string connectionString = DatabaseConfig.GetConnectionString();

            using (var connection = new MySqlConnection(connectionString))
            {
                await connection.OpenAsync();

                using (var transaction = await connection.BeginTransactionAsync())
                {
                    try
                    {
                        if (contract.Id == 0)
                        {
                            // INSERT contract
                            contract.Id = await _contractRepository.AddAsync(contract, connection, transaction);
                        }
                        else
                        {
                            // UPDATE contract
                            await _contractRepository.UpdateAsync(contract, connection, transaction);

                            // BULK REPLACE: Delete all old devices from the database
                            await _deviceRepository.DeleteByContractIdAsync(contract.Id, connection, transaction);
                        }

                        // Save the current list of devices from the form to the database
                        foreach (var device in contract.CoveredDevices)
                        {
                            // Require the foreign key to be set to the current contract ID
                            device.MaintenanceContractId = contract.Id;

                            // Insert a new device (The database generates a new auto-increment ID)
                            await _deviceRepository.AddAsync(device, connection, transaction);
                        }

                        // COMMIT TRANSACTION
                        await transaction.CommitAsync();
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        throw new Exception($"Speichern fehlgeschlagen. Grund: {ex.Message}", ex);
                    }
                }
            }
        }
    }
}
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

        public ContractService()
        {
            _contractRepository = new ContractRepository();
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
                            // INSERT
                            contract.Id = await _contractRepository.AddAsync(contract, connection, transaction);
                        }
                        else
                        {
                            // UPDATE
                            await _contractRepository.UpdateAsync(contract, connection, transaction);
                        }

                        // TODO (Später): Hier werden wir in einer Schleife alle zugeordneten Geräte (Devices) in die DB speichern!

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
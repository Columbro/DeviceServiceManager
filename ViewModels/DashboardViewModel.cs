
using DeviceServiceManager.Core;

namespace DeviceServiceManager.ViewModels
{
    /// <summary>
    /// Represents the logic and data for the dashboard overview.
    /// Displays key metrics like expiring contracts and active devices.
    /// </summary>
    public class DashboardViewModel : ObservableObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DashboardViewModel"/> class.
        /// </summary>
        public DashboardViewModel() 
        {
            // Here we will later load statistics from the database
        }
    }
}

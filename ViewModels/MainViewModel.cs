using DeviceServiceManager.Core;
using System.Windows.Input;

namespace DeviceServiceManager.ViewModels
{
    /// <summary>
    /// Represents the main navigation and state container for the application window.
    /// </summary>
    public class MainViewModel : ObservableObject
    {
        private ObservableObject _currentViewModel = null!;

        private readonly DashboardViewModel _dashboardViewModel;
        private readonly CustomerViewModel _customerViewModel;
        private readonly ContractListViewModel _contractListViewModel;

        /// <summary>
        /// Gets or sets the currently active ViewModel.
        /// The UI binds to this property to display the corresponding view.
        /// </summary>
        public ObservableObject CurrentViewModel
        {
            get => _currentViewModel;
            set
            {
                _currentViewModel = value;
                OnPropertyChanged();
            }
        }

        // --- Navigation Commands ---

        /// <summary>
        /// Command to navigate to the Dashboard View
        /// </summary>
        public ICommand NavigateDashboardCommand { get; }

        /// <summary>
        /// Command to navigate to the Customer View.
        /// </summary>
        public ICommand NavigateCustomersCommand { get; }

        /// <summary>
        /// Command to navigate to the Contract List View.
        /// </summary>
        public ICommand NavigateContractsCommand { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MainViewModel"/> class.
        /// Sets up the default view and navigation commands.
        /// </summary>
        public MainViewModel()
        {
            _dashboardViewModel = new DashboardViewModel();
            _customerViewModel = new CustomerViewModel();
            _contractListViewModel = new ContractListViewModel();

            // --- Navigation Commands ---
            NavigateDashboardCommand = new RelayCommand(_ =>
                CurrentViewModel = _dashboardViewModel
            );

            NavigateCustomersCommand = new RelayCommand(_ =>
                CurrentViewModel = _customerViewModel
            );

            NavigateContractsCommand = new RelayCommand(_ =>
                CurrentViewModel = _contractListViewModel
            );

            // Default View
            CurrentViewModel = _dashboardViewModel;
        }
    }
}

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

        /// <summary>
        /// Command to navigate to the Dashboard View
        /// </summary>
        public ICommand NavigateDashboardCommand { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MainViewModel"/> class.
        /// Sets up the default view and navigation commands.
        /// </summary>
        public MainViewModel()
        {
            _dashboardViewModel = new DashboardViewModel();

            NavigateDashboardCommand = new RelayCommand(_ =>
                CurrentViewModel = _dashboardViewModel
            );

            CurrentViewModel = _dashboardViewModel;
        }
    }
}

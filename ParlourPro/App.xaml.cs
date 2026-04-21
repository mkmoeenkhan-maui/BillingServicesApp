using ParlourPro.Services;

namespace ParlourPro
{
    public partial class App : Application
    {
        private readonly DatabaseService _dbService;
        public App(DatabaseService dbService)
        {
            InitializeComponent();
            _dbService = dbService;

            MainPage = new AppShell();
            // Connectivity change listener
            Connectivity.ConnectivityChanged += OnConnectivityChanged;
        }

        private async void OnConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
        {
            if (e.NetworkAccess == NetworkAccess.Internet)
            {
                // Pending items to Sync
                var pendingItems = await _dbService.GetUnsyncedItemsAsync();
                foreach (var item in pendingItems)
                {
                    await _dbService.SyncItemToDrive(item);
                }
            }
        }
    }
}

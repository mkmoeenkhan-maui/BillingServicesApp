using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ParlourPro.Models;
using ParlourPro.Services;
using System.Collections.ObjectModel;

namespace ParlourPro.ViewModels
{
    public partial class ServiceMasterViewModel : ObservableObject
    {
        // Replaced FirebaseService with DatabaseService
        private readonly DatabaseService _databaseService;

        [ObservableProperty] string newServiceName;
        [ObservableProperty] string newServiceRate;
        [ObservableProperty] bool isBusy;

        public ObservableCollection<ServiceItem> MasterServices { get; } = new();

        public ServiceMasterViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;
            LoadServicesCommand.Execute(null); // Data Load on Page Init
        }

        [RelayCommand]
        async Task LoadServices()
        {
            IsBusy = true;
            try
            {
                // Fetch from local SQLite
                var services = await _databaseService.GetServicesAsync();
                MasterServices.Clear();
                foreach (var s in services)
                {
                    MasterServices.Add(s);
                }
            }
            finally { IsBusy = false; }
        }

        [RelayCommand]
        async Task AddService()
        {
            if (string.IsNullOrWhiteSpace(NewServiceName) || !double.TryParse(NewServiceRate, out double rate))
            {
                await Shell.Current.DisplayAlert("Error", "Please enter valid name and price", "OK");
                return;
            }

            // DUPLICATE CHECK: Check if service name already exists in the list (case-insensitive)
            bool exists = MasterServices.Any(x => x.Name.Trim().ToLower() == NewServiceName.Trim().ToLower());

            if (!exists)
            {
                IsBusy = true;
                var service = new ServiceItem { Name = NewServiceName, Price = rate };

                // Save to SQLite and trigger background Drive sync
                await _databaseService.SaveServiceAsync(service);

                // UI reset aur refresh
                NewServiceName = string.Empty;
                NewServiceRate = string.Empty;
                await LoadServices();

                IsBusy = false;
            }
            
        }

        [RelayCommand]
        async Task DeleteService(ServiceItem item)
        {
            if (item == null) return;

            bool confirm = await Shell.Current.DisplayAlert("Delete", $"Do you want to delete {item.Name}?", "Yes", "No");
            if (confirm)
            {
                IsBusy = true;
                try
                {
                    // 1. Delete from SQLite
                    await _databaseService.DeleteServiceAsync(item);

                    // 2. Remove from the UI list
                    MasterServices.Remove(item);
                }
                catch (Exception ex)
                {
                    await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
                }
                finally { IsBusy = false; }
            }
        }
    }
}

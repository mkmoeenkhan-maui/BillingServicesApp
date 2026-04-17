using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ParlourPro.Models;
using ParlourPro.Services;
using System.Collections.ObjectModel;

namespace ParlourPro.ViewModels
{
    public partial class ServiceMasterViewModel : ObservableObject
    {
        private readonly FirebaseService _firebaseService;

        [ObservableProperty] string newServiceName;
        [ObservableProperty] string newServiceRate;
        [ObservableProperty] bool isBusy;

        public ObservableCollection<ServiceMaster> MasterServices { get; } = new();

        public ServiceMasterViewModel(FirebaseService firebaseService)
        {
            _firebaseService = firebaseService;
            LoadServicesCommand.Execute(null); // Page khulte hi data load karo
        }

        [RelayCommand]
        async Task LoadServices()
        {
            IsBusy = true;
            try
            {
                var services = await _firebaseService.GetServices();
                MasterServices.Clear();
                foreach (var s in services) MasterServices.Add(s);
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

            IsBusy = true;
            var service = new ServiceMaster { Name = NewServiceName, Price = rate };

            await _firebaseService.AddService(service);

            // UI reset aur refresh
            NewServiceName = string.Empty;
            NewServiceRate = string.Empty;
            await LoadServices();

            IsBusy = false;
        }
    }
}

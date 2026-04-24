using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ParlourPro.Services;
using ParlourPro.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;

namespace ParlourPro.ViewModels
{
    public partial class DashboardViewModel : ObservableObject
    {
        private readonly DatabaseService _databaseService;

        [ObservableProperty] string businessName = "Service & Academy Pro";
        [ObservableProperty] string todayEarning = "₹0.00";
        [ObservableProperty] string monthlyEarning = "₹0.00";

        public DashboardViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;
            _ = LoadEarnings();
        }
        // Navigation Commands
        [RelayCommand]
        [SupportedOSPlatform("windows10.0.17763.0")]
        async Task GoToBilling()
        {
            // Ye aapke existing ServiceBillingPage par le jayega
            await Shell.Current.GoToAsync(nameof(ServiceBillingPage));
        }

        [RelayCommand]
        [SupportedOSPlatform("windows10.0.17763.0")]
        async Task GoToHistory()
        {
            // Ye aapke existing ServiceMasterPage par le jayega
            await Shell.Current.GoToAsync(nameof(HistoryPage));
        }

        [RelayCommand]
        [SupportedOSPlatform("windows10.0.17763.0")]
        async Task ManageServices()
        {
            // Ye aapke existing ServiceMasterPage par le jayega
            await Shell.Current.GoToAsync(nameof(ServiceMasterPage));
        }

        [RelayCommand]
        [SupportedOSPlatform("windows10.0.17763.0")]
        async Task AddStudent()
        {
            // Future Academy Page ke liye
            await Shell.Current.DisplayAlert("Coming Soon", "Academy Management feature is being added.", "OK");
        }

        public async Task LoadEarnings()
        {
            //var today = await _databaseService.GetCollectionAsync(DateTime.Today, DateTime.Now);
            //TodayEarning = $"₹{today:N2}";

            var today = await _databaseService.GetCollectionAsync(DateTime.Today, DateTime.Now);
            var firstDayOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var monthly = await _databaseService.GetCollectionAsync(firstDayOfMonth, DateTime.Now);

            TodayEarning = $"₹{today:F2}";
            MonthlyEarning = $"₹{monthly:F2}";
        }
    }
}

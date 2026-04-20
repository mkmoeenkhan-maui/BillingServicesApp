using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ParlourPro.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParlourPro.ViewModels
{
    public partial class DashboardViewModel : ObservableObject
    {
        [ObservableProperty] string businessName = "Service & Academy Pro";
        [ObservableProperty] string todayEarning = "₹0.00";

        // Navigation Commands
        [RelayCommand]
        async Task GoToBilling()
        {
            // Ye aapke existing ServiceBillingPage par le jayega
            await Shell.Current.GoToAsync(nameof(ServiceBillingPage));
        }

        [RelayCommand]
        async Task ManageServices()
        {
            // Ye aapke existing ServiceMasterPage par le jayega
            await Shell.Current.GoToAsync(nameof(ServiceMasterPage));
        }

        [RelayCommand]
        async Task AddStudent()
        {
            // Future Academy Page ke liye
            await Shell.Current.DisplayAlert("Coming Soon", "Academy Management feature is being added.", "OK");
        }
    }
}

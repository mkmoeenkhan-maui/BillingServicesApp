using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Newtonsoft.Json;
using ParlourPro.Models;
using ParlourPro.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParlourPro.ViewModels
{
    // Simple message class (can be placed at the end of the file or in a Messages folder)
    public record RefreshHistoryMessage(bool Value);

    [QueryProperty(nameof(Bill), "Bill")]
    public partial class BillDetailsViewModel : ObservableObject
    {
        private readonly DatabaseService _databaseService;

        [ObservableProperty]
        ServiceEntry bill;

        public BillDetailsViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;
            // Constructor is kept simple as QueryProperty handles data injection            
        }

        [RelayCommand]
        public async Task CancelBill(ServiceEntry bill)
        {
            if (bill == null || bill.Status == "Cancelled") return;

            bool answer = await Shell.Current.DisplayAlert("Confirm", "Do you want to cancel this bill?", "Yes", "No");

            if (answer)
            {
                try
                {
                    // Update status in DB
                    var success = await _databaseService.UpdateBillStatusAsync(bill, "Cancelled");

                    ////// Broadcast message to update UI observers
                    ////WeakReferenceMessenger.Default.Send(new RefreshHistoryMessage(true));

                    await Shell.Current.DisplayAlert("Success", "Bill marked as Cancelled", "OK");

                    if (success > 0)
                    {
                        // Direct call
                        HistoryViewModel.RefreshUIAction?.Invoke();                        
                    }
                    await Shell.Current.GoToAsync("..");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
                }
            }
        }

        // Triggers on 'Bill' property set 
        partial void OnBillChanged(ServiceEntry value)
        {
            if (value != null && !string.IsNullOrEmpty(value.ServicesJson))
            {
                try
                {
                    // Deserialize only when data arrives
                    value.SelectedServices = JsonConvert.DeserializeObject<List<ServiceItem>>(value.ServicesJson);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"JSON Error: {ex.Message}");
                }
            }
        }        
    }
}

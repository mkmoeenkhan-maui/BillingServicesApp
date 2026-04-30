using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ParlourPro.Models;
using ParlourPro.Services;
using ParlourPro.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ParlourPro.ViewModels.BillDetailsViewModel;

namespace ParlourPro.ViewModels
{
    public partial class HistoryViewModel : ObservableObject
    {
        private readonly DatabaseService _databaseService;


        // The collection that the UI binds to
        public ObservableCollection<ServiceEntry> BillHistory { get; } = new();

        [ObservableProperty]
        bool isBusy;
        // Define current filter state
        [ObservableProperty]
        string selectedFilter = "All";

        // Add this at the top with other properties
        [ObservableProperty]
        string currentSearchText = string.Empty;


        public static Action RefreshUIAction { get; set; }

        public HistoryViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;

            // Register global refresh action
            RefreshUIAction = () =>
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    // Wait 300ms for navigation animation to finish
                    await Task.Delay(300);
                    await LoadInitialHistory();
                });
            };

            // Initial data load
            _ = LoadInitialHistory();
        }

        async Task LoadInitialHistory() => await Search("");

        [RelayCommand]
        public async Task FilterStatus(string status)
        {
            if (SelectedFilter == status) return; // Do nothing if same filter clicked

            //SelectedFilter = status;
            //// Trigger search with current text but new status
            //await Search(CurrentSearchText);            

            SelectedFilter = status;

            // Run this in a background task to keep the UI responsive
            await Task.Run(async () =>
            {
                // Fetch data from SQLite
                await Search(CurrentSearchText);
            });
        }

        [RelayCommand]
        public async Task Search(string text)
        {
            IsBusy = true;
            CurrentSearchText = text; // Keep track of the search string
            try
            {
                // Offload database work to background thread to avoid "Davey" lag
                var results = await Task.Run(async () =>
                    await _databaseService.GetBillHistoryAsync(text, SelectedFilter));

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    BillHistory.Clear();
                    foreach (var bill in results)
                    {
                        BillHistory.Add(bill);
                    }
                });
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        async Task ViewBillDetails(ServiceEntry bill)
        {
            if (bill == null) return;

            // Passing the full bill object to the details page
            var navigationParameter = new Dictionary<string, object> { { "Bill", bill } };
            await Shell.Current.GoToAsync(nameof(BillDetailsPage), navigationParameter);
        }

    }
}

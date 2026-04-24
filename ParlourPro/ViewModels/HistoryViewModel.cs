using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ParlourPro.Models;
using ParlourPro.Services;
using ParlourPro.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParlourPro.ViewModels
{
    public partial class HistoryViewModel : ObservableObject
    {
        private readonly DatabaseService _databaseService;

        // The collection that the UI binds to
        public ObservableCollection<ServiceEntry> BillHistory { get; } = new();

        [ObservableProperty]
        bool isBusy;

        public HistoryViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;
            // Load full history when page opens
            _ = LoadInitialHistory();
        }
        async Task LoadInitialHistory() => await Search("");

        [RelayCommand]
        public async Task Search(string text)
        {
            IsBusy = true;
            try
            {
                // Fetch filtered data from SQLite
                var results = await _databaseService.GetBillHistoryAsync(text);

                BillHistory.Clear();
                foreach (var bill in results)
                {
                    BillHistory.Add(bill);
                }
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

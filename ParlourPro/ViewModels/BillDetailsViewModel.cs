using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;
using ParlourPro.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParlourPro.ViewModels
{
    [QueryProperty(nameof(Bill), "Bill")]
    public partial class BillDetailsViewModel : ObservableObject
    {
        [ObservableProperty]
        ServiceEntry bill;

        public BillDetailsViewModel()
        {
            // Constructor is kept simple as QueryProperty handles data injection            
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

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ParlourPro.Models;
using ParlourPro.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ParlourPro.ViewModels
{
    public partial class ServiceBillingViewModel : ObservableObject
    {
        private readonly IParlourService _parlourService;
        //private readonly FirebaseService _firebaseService; // Added Firebase service
        private readonly DatabaseService _databaseService;

        [ObservableProperty] string currentBillNumber;
        [ObservableProperty] string customerName;
        [ObservableProperty] string mobile;
        [ObservableProperty] string currentServiceName;
        [ObservableProperty] double currentPrice;
        [ObservableProperty] double finalTotal;
        [ObservableProperty] ServiceItem selectedService;
        [ObservableProperty]
        bool isBusy; // This generates 'IsBusy' property

        [ObservableProperty]
        double discount = 0;

        [ObservableProperty]
        double gstRate = 18; // Default 18% rakhte hain, par editable hoga

        [ObservableProperty]
        bool isGstEnabled = false;

        

        public ObservableCollection<ServiceItem> CurrentBillItems { get; } = new();
        //public List<string> AvailableServices { get; } = new() { "Makeup", "Facial", "Bleach", "Hair Cut" };
        // Change to a simple ObservableCollection
        public ObservableCollection<ServiceItem> MasterServices { get; } = new();


        partial void OnSelectedServiceChanged(ServiceItem value)
        {
            if (value != null)
            {
                CurrentPrice = value.Price; // Auto-fill price!
                CurrentServiceName = value.Name;
            }
        }

        // Jab bhi Discount, GST ya Toggle change ho, Total update karo
        partial void OnDiscountChanged(double value) => RefreshTotal();
        partial void OnGstRateChanged(double value) => RefreshTotal();
        partial void OnIsGstEnabledChanged(bool value) => RefreshTotal();


        public ServiceBillingViewModel(IParlourService parlourService, DatabaseService databaseService)
        {
            _parlourService = parlourService;
            _databaseService = databaseService;

            // Load services from cloud when ViewModel initializes
            _ = LoadServicesFromCloud();
        }

        [RelayCommand]
        void AddToCart()
        {            
            // Check if service name is selected
            if (string.IsNullOrEmpty(CurrentServiceName) || CurrentPrice <= 0) return;

            CurrentBillItems.Add(new ServiceItem { Name = CurrentServiceName, Price = CurrentPrice });

            // Yahan sirf sum nahi, RefreshTotal call karein taaki Discount/GST apply ho
            RefreshTotal();

            // Clear inputs and Reset Picker selection
            CurrentPrice = 0;
            CurrentServiceName = string.Empty;
            SelectedService = null; // Taaki picker reset ho jaye
        }

        // 2. Delete Item from Cart
        [RelayCommand]
        void DeleteCartItem(ServiceItem item)
        {
            if (item != null && CurrentBillItems.Contains(item))
            {
                CurrentBillItems.Remove(item);
                RefreshTotal(); // Recalculate the grand total
            }
        }

        // 2. Edit Item Price in Cart (Simple Popup)
        [RelayCommand]
        async Task EditCartItem(ServiceItem item)
        {
            string newPrice = await Shell.Current.DisplayPromptAsync("Edit Price", $"Update price for {item.Name}", "Save", "Cancel", initialValue: item.Price.ToString(), keyboard: Keyboard.Numeric);

            if (double.TryParse(newPrice, out double updatedPrice))
            {
                item.Price = updatedPrice;
                // Hack: Trigger UI refresh by removing and re-adding (MAUI CollectionView limitation)
                int index = CurrentBillItems.IndexOf(item);
                CurrentBillItems.RemoveAt(index);
                CurrentBillItems.Insert(index, item);

                RefreshTotal();
            }
        }

        //[RelayCommand]
        //async Task SaveAndPrint(VisualElement receiptLayout)
        //{
        //    if (receiptLayout == null) return;

        //    IsBusy = true;
        //    try
        //    {
        //        // Generate new Bill No before saving
        //        CurrentBillNumber = GenerateUniqueBillNo();

        //        // 1. Prepare data for Firebase
        //        var bill = new ServiceEntry
        //        {
        //            BillId = CurrentBillNumber,
        //            CustomerName = CustomerName,
        //            Mobile = Mobile,
        //            GrandTotal = FinalTotal,
        //            SelectedServices = CurrentBillItems.ToList(),
        //            EntryDate = DateTime.Now
        //        };

        //        // Show receipt temporarily for screenshot
        //        receiptLayout.IsVisible = true;
        //        await Task.Delay(200); // Critical: Gives MAUI time to draw items

        //        // 2. Save to Firebase Cloud
        //        bool isSaved = await _firebaseService.SaveBill(bill);

        //        if (isSaved)
        //        {
        //            // 3. Capture the specific View passed as parameter
        //            // This captures the layout as an image
        //            IScreenshotResult screenshot = await receiptLayout.CaptureAsync();

        //            // 4. Save to temporary storage
        //            string filePath = Path.Combine(FileSystem.CacheDirectory, "invoice.png");
        //            using (Stream fileStream = File.Create(filePath))
        //            {
        //                await screenshot.CopyToAsync(fileStream);
        //            }

        //            //5. Send whatsapp message first
        //            // Standard format: whatsapp://send?phone=919876543210&text=Hello
        //            string message = $"Hello {CustomerName}! ✨\n\n" +
        //                 $"Thank you for visiting Parlour Pro. Your bill for today is ready.\n" +
        //                 $"Total Amount: ₹{FinalTotal}\n\n" +
        //                 $"We are sharing your digital invoice below. Hope to see you again soon! 🙏";

        //            //string message = $"Hello {CustomerName}, your bill of {FinalTotal} is ready!";
        //            string url = $"whatsapp://send?phone=91{Mobile}&text={Uri.EscapeDataString(message)}";

        //            await Launcher.Default.OpenAsync(url);

        //            // 6. Open Share Sheet
        //            await Share.Default.RequestAsync(new ShareFileRequest
        //            {
        //                Title = "Share Receipt",
        //                File = new ShareFile(filePath)
        //            });

        //            //// 6. Share in "One Go" (Image + Text as Caption)
        //            //// If WhatsApp is missing, the OS will show other apps automatically
        //            //await Share.Default.RequestAsync(new ShareFileRequest
        //            //{
        //            //    Title = "Parlour Pro Invoice",
        //            //    File = new ShareFile(filePath),
        //            //    Text = message // This acts as the WhatsApp caption or Email body
        //            //});                    

        //            //await Shell.Current.DisplayAlert("Success", "Bill saved and ready to share!", "OK");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
        //    }
        //    finally
        //    {
        //        IsBusy = false;
        //        receiptLayout.IsVisible = false;
        //    }
        //    //var bill = new ServiceEntry
        //    //{
        //    //    CustomerName = CustomerName,
        //    //    Mobile = Mobile,
        //    //    SelectedServices = CurrentBillItems.ToList(),
        //    //    GrandTotal = FinalTotal
        //    //};

        //    //bool success = await _parlourService.SaveBill(bill);
        //    //if (success)
        //    //{
        //    //    //// WhatsApp message logic
        //    //    //string msg = $"Thanks {CustomerName}! Total: ₹{FinalTotal}";
        //    //    //await Launcher.Default.OpenAsync($"whatsapp://send?phone=91{Mobile}&text={msg}");

        //    //    // Reset Form
        //    //    CurrentBillItems.Clear();
        //    //    CustomerName = Mobile = "";
        //    //    FinalTotal = 0;
        //    //}
        //}


        // 4. Unique Bill Number Logic (Milliseconds + Random)

        [RelayCommand]
        async Task SaveAndPrint(VisualElement receiptLayout)
        {
            if (receiptLayout == null || CurrentBillItems.Count == 0) return;

            IsBusy = true;
            try
            {
                // Step 1: Generate unique bill number
                CurrentBillNumber = GenerateUniqueBillNo();

                // Step 2: Prepare single entry for database (Contains all items)
                var billEntry = new ServiceEntry
                {
                    BillId = CurrentBillNumber,
                    CustomerName = CustomerName,
                    Mobile = Mobile,
                    GrandTotal = FinalTotal,
                    SelectedServices = CurrentBillItems.ToList(),
                    EntryDate = DateTime.Now,
                    IsSynced = false
                };

                // Step 3: Save locally to SQLite
                var res = await _databaseService.SaveBillAsync(billEntry);

                // Step 4: Handle Screenshot with safety delay
                receiptLayout.IsVisible = true;
                await Task.Delay(500); // Increased delay to prevent null reference on CaptureAsync

                IScreenshotResult screenshot = await receiptLayout.CaptureAsync();
                if (screenshot == null) throw new Exception("Failed to capture receipt screenshot.");

                string filePath = Path.Combine(FileSystem.CacheDirectory, "invoice.png");
                using (Stream fileStream = File.Create(filePath))
                {
                    await screenshot.CopyToAsync(fileStream);
                }

                // Step 5: WhatsApp Logic
                string message = $"Hello {CustomerName}! ✨\n\n" +
                                 $"Thank you for visiting. Your bill {CurrentBillNumber} is ready.\n" +
                                 $"Total Amount: ₹{FinalTotal}\n\n" +
                                 $"Sharing digital invoice below. 🙏";

                string url = $"whatsapp://send?phone=91{Mobile}&text={Uri.EscapeDataString(message)}";
                await Launcher.Default.OpenAsync(url);

                // Step 6: Share Receipt Image
                await Share.Default.RequestAsync(new ShareFileRequest
                {
                    Title = "Share Receipt",
                    File = new ShareFile(filePath)
                });

                // Reset for next customer
                CurrentBillItems.Clear();
                CustomerName = string.Empty;
                Mobile = string.Empty;
                RefreshTotal();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", "Save failed: " + ex.Message, "OK");
            }
            finally
            {
                IsBusy = false;
                receiptLayout.IsVisible = false;
            }
        }

        private string GenerateUniqueBillNo()
        {
            // Format: INV + 20240522 (Date) + 123045 (Time) + 999 (Millis) + 10 (Random)
            string timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            int randomSuffix = new Random().Next(10, 99);
            return $"INV-{timestamp}-{randomSuffix}";
        }

        public void RefreshTotal()
        {
            double subTotal = CurrentBillItems.Sum(x => x.Price);

            // 1. Pehle Discount minus karein
            double afterDiscount = subTotal - Discount;
            if (afterDiscount < 0) afterDiscount = 0;

            // 2. GST calculate karein (Dynamic Rate se)
            if (IsGstEnabled)
            {
                double gstAmount = (afterDiscount * GstRate) / 100;
                FinalTotal = afterDiscount + gstAmount;
            }
            else
            {
                FinalTotal = afterDiscount;
            }
        }

        private async Task LoadServicesFromCloud()
        {
            try
            {
                var services = await _databaseService.GetServicesAsync();
                MasterServices.Clear();
                foreach (var service in services)
                {
                    MasterServices.Add(service);
                }
            }
            catch (Exception ex)
            {
                // Handle or log potential connection errors
                System.Diagnostics.Debug.WriteLine($"Error loading services: {ex.Message}");
            }
        }
    }
}

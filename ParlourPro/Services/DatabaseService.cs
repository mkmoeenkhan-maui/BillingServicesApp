using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Newtonsoft.Json;
using ParlourPro.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParlourPro.Services
{
    public class DatabaseService
    {
        // SQLite connection instance
        private SQLiteAsyncConnection _database;

        // Google Drive API service instance
        private DriveService _driveService;

        // Initializes the SQLite database and creates tables if they don't exist
        async Task Init()
        {
            if (_database is not null) return;

            _database = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);

            // Create ServiceItem table
            await _database.CreateTableAsync<ServiceItem>();
            await _database.CreateTableAsync<ServiceEntry>(); // For Bills/Transactions

            // Future: Add Student or Academy tables here
            // await _database.CreateTableAsync<Student>();
        }

        // Method to save the entire bill (ServiceEntry)
        public async Task<int> SaveBillAsync(ServiceEntry bill)
        {
            await Init();

            // Save bill to local SQLite
            int result = await _database.InsertAsync(bill);

            // Attempt background sync to Google Drive
            if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
            {
                _ = Task.Run(async () => await SyncBillToDrive(bill));
            }

            return result;
        }

        // Sync Logic for Bill (Modified for ServiceEntry)
        private async Task SyncBillToDrive(ServiceEntry bill)
        {
            try
            {
                await SetupDriveService();

                var fileMetadata = new Google.Apis.Drive.v3.Data.File()
                {
                    Name = $"Bill_{bill.BillId}_{bill.EntryDate:yyyyMMdd}.txt",
                    MimeType = "text/plain"
                };

                string content = $"Bill ID: {bill.BillId}\nCustomer: {bill.CustomerName}\nTotal: ₹{bill.GrandTotal}";
                byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(content);
                using var stream = new MemoryStream(byteArray);

                var request = _driveService.Files.Create(fileMetadata, stream, "text/plain");
                var status = await request.UploadAsync();

                if (status.Status == Google.Apis.Upload.UploadStatus.Completed)
                {
                    bill.IsSynced = true;
                    await _database.UpdateAsync(bill);
                }
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
        }

        // Fetches all service records from the local database
        public async Task<List<ServiceItem>> GetServicesAsync()
        {
            await Init();
            return await _database.Table<ServiceItem>().ToListAsync();
        }

        // Saves a new service and attempts to sync it with Google Drive
        public async Task<int> SaveServiceAsync(ServiceItem item)
        {
            await Init();

            // Step 1: Set metadata and save to local SQLite database (Offline-First)
            item.CreatedAt = DateTime.Now;
            item.IsSynced = false;
            int result = await _database.InsertAsync(item);

            // Step 2: Check for internet connectivity and trigger background sync
            if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
            {
                // Fire and forget: Sync in the background without blocking the UI thread
                _ = Task.Run(async () => await SyncItemToDrive(item));
            }

            return result;
        }

        // Uploads the service record details to Google Drive as a text file
        public async Task SyncItemToDrive(ServiceItem item)
        {
            try
            {
                // Ensure Drive service is authenticated
                await SetupDriveService();

                // Prepare file metadata for Google Drive
                var fileMetadata = new Google.Apis.Drive.v3.Data.File()
                {
                    Name = $"BizBill_{item.Id}_{item.CreatedAt:yyyyMMdd_HHmm}.txt",
                    MimeType = "text/plain"
                };

                // Format the content for the backup file
                string content = $"BizBill Backup\n" +
                                 $"------------------\n" +
                                 $"Service Name: {item.Name}\n" +
                                 $"Price: {item.Price}\n" +
                                 $"Date: {item.CreatedAt:f}";

                byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(content);
                using var stream = new MemoryStream(byteArray);

                // Create the upload request
                var request = _driveService.Files.Create(fileMetadata, stream, "text/plain");
                var uploadStatus = await request.UploadAsync();

                // If upload is successful, update the local record status
                if (uploadStatus.Status == Google.Apis.Upload.UploadStatus.Completed)
                {
                    item.IsSynced = true;
                    await _database.UpdateAsync(item);
                }
            }
            catch (Exception ex)
            {
                // Log error to console (You can replace this with a logger)
                Console.WriteLine($"[Sync Error]: {ex.Message}");
            }
        }

        // Handles OAuth2 authentication for Google Drive API
        private async Task SetupDriveService()
        {
            if (_driveService != null) return;

            // Load client secrets from a JSON file (stored as MauiAsset)
            using var stream = await FileSystem.OpenAppPackageFileAsync("client_secret.json");

            // Authorize user and get credentials
            var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                GoogleClientSecrets.FromStream(stream).Secrets,
                new[] { DriveService.Scope.DriveFile },
                "user",
                CancellationToken.None);

            // Initialize the Drive Service with the credentials
            _driveService = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "BizBill"
            });
        }

        // NEW: Fetches all records that have not been synced to Google Drive yet
        public async Task<List<ServiceItem>> GetUnsyncedItemsAsync()
        {
            await Init();
            // Returns only items where IsSynced is false
            return await _database.Table<ServiceItem>().Where(x => !x.IsSynced).ToListAsync();
        }

        // Manually trigger sync for all pending (unsynced) items
        public async Task SyncPendingItems()
        {
            await Init();

            // Fetch all records where IsSynced is false
            var pending = await _database.Table<ServiceItem>().Where(x => !x.IsSynced).ToListAsync();

            foreach (var item in pending)
            {
                await SyncItemToDrive(item);
            }
        }

        // Method to delete a master service item
        public async Task<int> DeleteServiceAsync(ServiceItem item)
        {
            await Init();
            // Deletes the item from SQLite using its Primary Key (Id)
            return await _database.DeleteAsync(item);
        }

        // Get total for a specific date range
        public async Task<double> GetCollectionAsync(DateTime startDate, DateTime endDate)
        {
            await Init();
            var bills = await _database.Table<ServiceEntry>()
                                       .Where(x => x.EntryDate >= startDate && x.EntryDate <= endDate)
                                       .ToListAsync();
            return bills.Sum(x => x.GrandTotal);
        }

        // Fetches bill history with a default 2-month filter and optional search text
        public async Task<List<ServiceEntry>> GetBillHistoryAsync(string searchText = "")
        {
            await Init();

            // Default: Show last 2 months of records
            DateTime dateLimit = DateTime.Now.AddMonths(-2);

            var query = _database.Table<ServiceEntry>()
                                 .Where(x => x.EntryDate >= dateLimit);

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                string search = searchText.ToLower();
                // Filter by Customer Name or Bill ID
                query = query.Where(x => x.CustomerName.ToLower().Contains(search) ||
                                         x.BillId.ToLower().Contains(search));
            }

            // Sort by newest first
            var results = await query.OrderByDescending(x => x.EntryDate).ToListAsync();

            // Deserialize JSON items back to the list for UI display
            foreach (var bill in results)
            {
                if (!string.IsNullOrEmpty(bill.ServicesJson))
                    bill.SelectedServices = JsonConvert.DeserializeObject<List<ServiceItem>>(bill.ServicesJson);
            }

            return results;
        }
    }
}

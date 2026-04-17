using Firebase.Database;
using ParlourPro.Models;
using Firebase.Database;
using Firebase.Database.Query;

namespace ParlourPro.Services
{
    public class FirebaseService
    {
        // Firebase Console se URL yahan dalein
        private readonly FirebaseClient _client = new FirebaseClient("https://parlourpro-2988b-default-rtdb.asia-southeast1.firebasedatabase.app/",
            new FirebaseOptions
            {
                AuthTokenAsyncFactory = () => Task.FromResult("xPY8AJjEF3p4wlE87OOt1v9whZiyI3jjudjMtXDR")
            });

        // Nayi Service save karne ke liye
        public async Task AddService(Models.ServiceMaster service)
        {
            await _client.Child("Services").PostAsync(service);
        }

        // Update existing service
        public async Task UpdateService(string id, ServiceMaster service) =>
            await _client.Child("Services").Child(id).PutAsync(service);

        // Delete service
        public async Task DeleteService(string id) =>
            await _client.Child("Services").Child(id).DeleteAsync();
        
        // Saari Services load karne ke liye
        public async Task<List<ServiceMaster>> GetServices()
        {
            var services = await _client.Child("Services").OnceAsync<ServiceMaster>();
            return services.Select(x => x.Object).ToList();
        }

        public async Task<bool> SaveBill(ServiceEntry bill)
        {
            try
            {
                // Creates a new entry under "Bills" node
                await _client.Child("Bills").PostAsync(bill);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}

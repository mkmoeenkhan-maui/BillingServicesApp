using ParlourPro.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParlourPro.Services
{
    public class ParlourService : IParlourService
    {
        private List<ServiceEntry> _tempDb = new(); // Abhi ke liye local memory, baad mein Firebase

        public async Task<bool> SaveBill(ServiceEntry bill)
        {
            _tempDb.Add(bill);
            await Task.Delay(500); // Network simulation
            return true;
        }

        public async Task<List<ServiceEntry>> GetHistory() => _tempDb.OrderByDescending(x => x.EntryDate).ToList();
    }
}

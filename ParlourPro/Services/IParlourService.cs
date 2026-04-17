using ParlourPro.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParlourPro.Services
{
    public interface IParlourService
    {
        Task<bool> SaveBill(ServiceEntry bill);
        Task<List<ServiceEntry>> GetHistory();

        //// Aaj ki total kamai calculate karne ke liye
        //Task<double> GetTodayTotalEarnings();
    }
}

using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParlourPro.Models
{
    ////Firebase old class
    //public class ServiceEntry
    //{
    //    public string Id { get; set; } = Guid.NewGuid().ToString();
    //    public string CustomerName { get; set; }
    //    public string BillId { get; set; }
    //    public string Mobile { get; set; }
    //    public List<ServiceItem> SelectedServices { get; set; } = new();
    //    public double GrandTotal { get; set; }
    //    public DateTime EntryDate { get; set; } = DateTime.Now;

    //}

    public class ServiceEntry
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string BillId { get; set; }
        public string CustomerName { get; set; }
        public string Mobile { get; set; }
        public double GrandTotal { get; set; }
        public DateTime EntryDate { get; set; }

        // This property is used for Google Drive sync tracking
        public bool IsSynced { get; set; }

        // SQLite doesn't support List directly. 
        // We use [Ignore] so it doesn't crash the DB, 
        // but it can still be used for the UI/WhatsApp.
        [Ignore]
        public List<ServiceItem> SelectedServices { get; set; }

        // Actual string stored in SQLite
        public string ServicesJson { get; set; }

        // Added to handle bill status: Active, Cancelled
        public string Status { get; set; } = "Active";
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParlourPro.Models
{
    public class ServiceEntry
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string CustomerName { get; set; }
        public string BillId { get; set; }
        public string Mobile { get; set; }
        public List<ServiceItem> SelectedServices { get; set; } = new();
        public double GrandTotal { get; set; }
        public DateTime EntryDate { get; set; } = DateTime.Now;
    }

    public class ServiceItem
    {
        public string Name { get; set; }
        public double Price { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParlourPro.Models
{
    public class ServiceMaster
    {
        public string Id { get; set; } // Firebase key ke liye
        public string Name { get; set; }
        public double Price { get; set; }
    }
}

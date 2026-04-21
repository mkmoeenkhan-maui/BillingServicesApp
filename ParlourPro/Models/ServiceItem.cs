using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParlourPro.Models
{
    public class ServiceItem
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Name { get; set; }
        public double Price { get; set; }
        public bool IsSynced { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

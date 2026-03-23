using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceServiceManager.Models
{
    public class Address
    {
        public int Id { get; set; }
        public string Street { get; set; } = string.Empty;
        public string HouseNumber { get; set; } = string.Empty;
        public string ZipCode {  get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Country { get; set; } = "Deutschland";
        public DateTime CreatedAt { get; set; }
    }
}

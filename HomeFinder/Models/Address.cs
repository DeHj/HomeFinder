using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HomeFinder.Models
{
    public class Address
    {
        public int AddressId { get; set; }

        public int UserId { get; set; } 
        
        public string Street { get; set; }
        public string House { get; set; }
        public int Building { get; set; }
    }
}

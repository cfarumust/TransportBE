using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TransportBE.Models.DTOs
{
    public class Shipper
    {
        public decimal nShipperId { get; set; }
        public string sName{ get; set; }        
        public string sAddress { get; set; }
        public decimal nVehicleId { get; set; }
        public string sEmail { get; set; }
        public string sPhone { get; set; }        
        public string sUserName { get; set; }
        public string sPassword { get; set; }
       
    }
}

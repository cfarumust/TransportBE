using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TransportBE.Models.DTOs
{
    public class Client
    {
        public string sName { get; set; }
        public string sAddress        {get;set;}
        public string   sPhone        {get;set;}
        public string   sEmail        {get;set;}
        public string sUsername { get; set; } 
        public string sPassword { get; set; }
        public string nClientId { get; set; }

    }
}

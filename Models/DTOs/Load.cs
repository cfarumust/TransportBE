using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TransportBE.Models.DTOs
{
    public class Load
    {       
       public string SADDRESSPICKUP  {get; set;}
       public string SADDRESSDROP    {get; set;}
       public decimal NBOXCOUNT       {get; set;}
       public decimal NBOXID          {get; set;}
       public decimal NORDERID        {get; set;}
        public decimal NPICKUPLAT { get; set; }
        public decimal NPICKUPLONG { get; set; }
        public decimal NDROPLAT { get; set; }
        public decimal NDROPLONG { get; set; }
        public decimal NLEGID { get; set; }
        public decimal NISMULTIVEHICLELOAD{ get; set; }
        public string SGRIDIDPICKUP { get; set; }

        public string SGRIDIDDROP { get; set; }

        public string SSTATUSID { get; set; }
    }
}

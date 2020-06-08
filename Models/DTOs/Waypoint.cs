using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TransportBE.Models.DTOs
{
    public class Waypoint
    {
         public decimal   NWAYPOINTID    { get; set; }
         public decimal   NORDERID { get; set; }
         public decimal   NLAT { get; set; }
         public decimal   NLONG { get; set; }
         public decimal   NLOADID { get; set; }
         public decimal NLEGID { get; set; }
    }
}

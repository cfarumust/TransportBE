using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TransportBE.Models.DTOs
{
    public class Vector
    {

       
        public Vector(decimal x, decimal y)
        {
            this.x = x/(Math.Abs(x));
            this.y = y/(Math.Abs(y));
        }

        public decimal x { get; set; }
        public decimal y { get; set; }

    }
}

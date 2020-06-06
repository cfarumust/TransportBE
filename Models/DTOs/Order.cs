﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TransportBE.Models.DTOs
{
    public class Order
    {
        public decimal NORDERID { get; set; }
        //public DateTime DTORDEREDON { get; set; }                
        public DateTime DTPICKUPDATE { get; set; }
        public DateTime DTDROPDATE { get; set; }
        public decimal NCLIENTID { get; set; }
        public string sAddressPickUp { get; set; }
        public string sAddressDrop { get; set; }
        public decimal NBOXID       { get; set; }
        public decimal NBOXCOUNT { get; set; }
        public decimal NDISTANCE    { get; set; }
        public decimal NPICKUPLAT   { get; set; }
        public decimal NPICKUPLONG  { get; set; }
        public decimal NDROPLAT     { get; set; }
        public decimal NDROPLONG    { get; set; }

    }
}
using System;
using System.Collections.Generic;
using TransportBE.Models.DTOs;

namespace TransportBE.Services
{
    public interface ITransportRepository
    {     
        void PostOrder(Order entity);        
        Shipper GetShipperByUserName(String SUSERNAME);
        void ShipperRegister(Shipper entity);
        Shipper LoginShipper(String SUSERNAME, String SPASSWORD);
        Client LoginClient(string SUSERNAME, string SPASSWORD);
        List<Order> GetOrdersByClient(decimal nClientId);
        List<Load> GetLoadsByOrderId(decimal nOrderId);
        List<Load> GetAvailableLoads();
        void AssignLoad(Load entity);
    }
}

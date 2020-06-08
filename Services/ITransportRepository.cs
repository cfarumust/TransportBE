using System;
using System.Collections.Generic;
using TransportBE.Models.DTOs;

namespace TransportBE.Services
{
    public interface ITransportRepository
    {
        int PostOrder(Order entity);
       
        void ShipperRegister(Shipper entity);
        Shipper LoginShipper(String SUSERNAME, String SPASSWORD);
        Client LoginClient(string SUSERNAME, string SPASSWORD);
        List<Order> GetOrdersByClient(decimal nClientId);
        List<Load> GetLoadsByOrderId(decimal nOrderId);
        List<Load> GetAvailableLoads();
        int AssignLoad(Load entity);
        List<Waypoint> GetOrderRouteWithWayPoints(decimal nOrderId);
        Client CheckClientUsernameExists(string SUSERNAME);

        Shipper CheckShipperUsernameExists(string SUSERNAME);
        void ClientRegister(Client entity);
    }
}

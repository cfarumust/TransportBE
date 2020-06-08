using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TransportBE.Models.DTOs;
using TransportBE.Services;

namespace TransportBE.Controllers
{
    [Authorize]
    [Route("[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly ITransportRepository _transportRepository;
        

        public OrderController(ITransportRepository transportRepository) {
            _transportRepository = transportRepository;
        }
   


        [HttpGet]
        [Route("client/{nClientId}")]
        public ActionResult<Order> GetOrdersByClient(decimal nClientId)
        {
            var product = _transportRepository.GetOrdersByClient(nClientId);
            return Ok(product);
        }

        [HttpGet]
        [Route("route/{nOrderId}")]
        public ActionResult<Waypoint> GetOrderRouteWithWayPoints(decimal nOrderId)
        {
            var product = _transportRepository.GetOrderRouteWithWayPoints(nOrderId);
            return Ok(product);
        }

        // POST: api/Order
        [HttpPost] 
        public ActionResult PostOrder(Order entity)
        {
           int fail = _transportRepository.PostOrder(entity);
            
            if (fail == 0) 
            {
                var response = Ok(new { entity, success = true, info = "Order placed" });
                return response;
            }
            else 
            {
                var response = Ok(new { entity, success = false, info = "Service not available in this area. Please contact Support." });
                return response;
            }
            
        }

        
    }
}

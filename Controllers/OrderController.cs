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
        [Route("{nClientId}")]
        public ActionResult<Order> GetOrdersByClient(decimal nClientId)
        {
            var product = _transportRepository.GetOrdersByClient(nClientId);
            return Ok(product);
        }

        // POST: api/Order
        [HttpPost] 
        public ActionResult PostOrder(Order entity)
        {
           _transportRepository.PostOrder(entity);
            return Ok(entity);
        }

        
    }
}

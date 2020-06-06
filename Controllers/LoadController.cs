using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TransportBE.Models.DTOs;
using TransportBE.Services;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TransportBE.Controllers
{
    [Authorize]
    [Route("[controller]")]
    [ApiController]
    public class LoadController : ControllerBase
    {
        private readonly ITransportRepository _transportRepository;

        public LoadController(ITransportRepository transportRepository)
        {
            _transportRepository = transportRepository;
        }

        [HttpGet]
        [Route("client/{nOrderId}")]
        public ActionResult<Load> GetLoadsByOrderId(decimal nOrderId)
        {
            var product = _transportRepository.GetLoadsByOrderId(nOrderId);
            return Ok(product);
        }


        [HttpGet]
        [Route("shipper/loads-available")]
        public ActionResult<Load> GetAvailableLoads()
        {
            var product = _transportRepository.GetAvailableLoads();
            return Ok(product);
        }

        [HttpPut]
        [Route("shipper/assign_to_load")]
        public void AssignLoad(Load entity)
        {
             _transportRepository.AssignLoad(entity);            
        }
    }
}

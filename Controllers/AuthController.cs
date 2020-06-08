//using System;
//using System.Threading.Tasks;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using TransportBE.Models;
//using TransportBE.Services;
//using System.IdentityModel.Tokens.Jwt;
//using System.Text;
//using Microsoft.IdentityModel.Tokens;
//using System.Security.Claims;
//using Microsoft.Extensions.Configuration;
//using System.Net;

//namespace TransportBE.Controllers
//{
//    //[Authorize]
//    [Route("[controller]")]
//    [ApiController]
//    public class AuthController : ControllerBase
//    {
//        private readonly Services.ITransportRepository _transportRepository;
//        private readonly IConfiguration _config;

//        public AuthController(ITransportRepository transportRepository,  IConfiguration config)
//        {
//            _transportRepository = transportRepository;
//            _config = config;
//        }
//        // GET: api/Shipper
       

//        // GET: api/Shipper/5
//        [HttpGet]
//        [Route("/shipper/{SUSERNAME}")]
//        public ActionResult<Models.ShipperRegister> GetShipperByUserName(String SUSERNAME)
//        {
//            var product = _transportRepository.GetShipperByUserName(SUSERNAME);
//            return Ok(product);
//        }

//        
//        [AllowAnonymous]
//        [HttpPost("register")]
//        public async Task<IActionResult> ShipperRegister([FromBody]ShipperRegister model)
//        {
//            Boolean _UserExists;
//            if (!ModelState.IsValid)
//                return BadRequest(ModelState);

//            model.SUSERNAME = model.SUSERNAME.ToLower();

//            if (_transportRepository.GetShipperByUserName(model.SUSERNAME) != null) {
//                _UserExists = true;
//                //HttpStatusCode codeNotDefined = (HttpStatusCode)422;
//                //return Content(codeNotDefined, "message to be sent in response body");
//                return BadRequest("Username is already taken");
            
//            }
//            _transportRepository.ShipperRegister(model);

//            return StatusCode(201);
//        }

//        [HttpPost("login")]
//        public async Task<IActionResult> Login([FromBody] AuthenticateModel loginShipper)
//        {
//            var userFromRepo = _transportRepository.LoginShipper(loginShipper.Username.ToLower(), loginShipper.Password);
//            if (userFromRepo == null) //User login failed
//                return Unauthorized();

//            //generate token
//            var tokenHandler = new JwtSecurityTokenHandler();
//            var key = Encoding.ASCII.GetBytes(_config.GetSection("JwtConfig").GetSection("secret").Value);
//            var tokenDescriptor = new SecurityTokenDescriptor
//            {
//                Subject = new ClaimsIdentity(new Claim[]{
//                    new Claim(ClaimTypes.NameIdentifier,userFromRepo.SUSERNAME.ToString()),
//                    new Claim(ClaimTypes.Name, userFromRepo.SUSERNAME)
//                }),
//                Expires = DateTime.Now.AddDays(1),
//                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha512Signature)
//            };

//            var token = tokenHandler.CreateToken(tokenDescriptor);
//            var tokenString = tokenHandler.WriteToken(token);

//            return Ok(new { tokenString });
//        }

//        // PUT: api/Shipper/5
//        [HttpPut("{id}")]
//        public void Put(int id, [FromBody] string value)
//        {
//        }

//        // DELETE: api/ApiWithActions/5
//        [HttpDelete("{id}")]
//        public void Delete(int id)
//        {
//        }
//    }
//}

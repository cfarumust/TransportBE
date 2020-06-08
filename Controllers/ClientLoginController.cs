using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using TransportBE.Models.DTOs;
using TransportBE.Services;
// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TransportBE.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ClientLoginController : Controller
    {
        private readonly Services.ITransportRepository _transportRepository;
        private readonly IConfiguration _config;

        public ClientLoginController(ITransportRepository transportRepository, IConfiguration config)
        {
            _transportRepository = transportRepository;
            _config = config;
        }
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login([FromBody] AuthenticateModel loginClient)
        {
            IActionResult response = Unauthorized();
            var userFromRepo = _transportRepository.LoginClient(loginClient.Username.ToLower(), loginClient.Password);

            AuthenticateModel authuser = new AuthenticateModel();
            authuser.Username = loginClient.Username.ToLower();
            authuser.Password = loginClient.Password;

            if (userFromRepo == null) //User login failed
            {
                return response;
            }
            else
            {
                var tokenString = GenerateJSONWebToken(authuser);
                response = Ok(new { token = tokenString, success = true, nclientid = userFromRepo.nClientId });
                return response;
            }


        }

        private string GenerateJSONWebToken(AuthenticateModel userInfo)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JwtSettings:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(_config["JwtSettings:Issuer"],
              _config["JwtSettings:Issuer"],
              null,
              expires: DateTime.Now.AddDays(7),
              signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Client([FromBody] Client model)
        {
            Boolean _UserExists;
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            model.sUsername = model.sUsername.ToLower();

            if (_transportRepository.CheckClientUsernameExists(model.sUsername) != null)
            {
                _UserExists = true;
                //HttpStatusCode codeNotDefined = (HttpStatusCode)422;
                //return Content(codeNotDefined, "message to be sent in response body");
                return BadRequest("Username is already taken");

            }
            else
            {
                _transportRepository.ClientRegister(model);

                 return Ok();
            }
            
        }

    }
}

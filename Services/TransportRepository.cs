using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Threading.Tasks;
using TransportBE.Models.DTOs;

using TransportBE.Helpers;
using System.Data.SqlClient;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.CodeAnalysis.CSharp;
using TransportBE.Controllers;
using TransportBE.Models.DataOperators;
using System.Web.WebPages;


namespace TransportBE.Services
{
    public class TransportRepository : ITransportRepository
    {
        private readonly CommandText _commandText;
        private readonly string _connStr;
        private readonly AppSettings _appSettings;
       
        private LoadProcessor _loadprocessor;
        private void ExecuteCommand(string connStr, Action<SqlConnection> task)
        {
            using (var conn = new SqlConnection(connStr))
            {
                conn.Open();
                
                task(conn);
            }
        }
        private T ExecuteCommand<T>(string connStr, Func<SqlConnection, T> task)
        {
            using (var conn = new SqlConnection(connStr))
            {
                conn.Open();

                return task(conn);
            }
        }
        public TransportRepository(Microsoft.Extensions.Configuration.IConfiguration configuration, CommandText commandText)
        {

            _commandText = commandText;
            _connStr = configuration.GetConnectionString("qsys");
        }


        

        public  void PostOrder(Order entity)
        {
            List<Load> loadList;
            using (var connection = new SqlConnection(_connStr)) 
            {
                connection.Open();
                Order currentOrder = new Order
                {
                    DTPICKUPDATE = entity.DTPICKUPDATE,
                    DTDROPDATE = entity.DTDROPDATE,
                    NCLIENTID = entity.NCLIENTID,
                    sAddressPickUp = entity.sAddressPickUp,
                    sAddressDrop = entity.sAddressDrop,
                    NBOXID = entity.NBOXID,
                    NBOXCOUNT = entity.NBOXCOUNT,
                    NDISTANCE = entity.NDISTANCE,  
                    NPICKUPLAT  =entity.NPICKUPLAT, 
                    NPICKUPLONG =entity.NPICKUPLONG,
                    NDROPLAT    =entity.NDROPLAT,   
                    NDROPLONG   =entity.NDROPLONG
                };

                var insertedId = connection.Query<int>(_commandText.PostOrder, currentOrder).First();

                int id = (int)insertedId;
                loadList = LoadProcessor.GenerateLoadsFromOrder(id, currentOrder);

                foreach (Load genLoad in loadList)
                {
                    ExecuteCommand(_connStr, conn =>
                    {
                        var query = conn.Query<Load>(_commandText.PostLoad, genLoad);
                    });
                }                
            }           
        }

       

        public Shipper GetShipperByUserName(string SUSERNAME)
        {
            var pUser = ExecuteCommand<Shipper>(_connStr, conn =>
                 conn.Query<Shipper>(_commandText.GetShipperByUserName, new { @SUSERNAME = SUSERNAME }).SingleOrDefault());
            return pUser;
        }

        public Shipper LoginShipper(string SUSERNAME, string SPASSWORD)
        {
            var pUser = ExecuteCommand<Shipper>(_connStr, conn =>
                 conn.Query<Shipper>(_commandText.LoginShipper, new { @SUSERNAME = SUSERNAME, @SPASSWORD= SPASSWORD}).SingleOrDefault());
            return pUser;
        }
        
        public void ShipperRegister(Shipper entity)
        {
            ExecuteCommand(_connStr, conn =>
            {
                var query = conn.Query<Shipper>(_commandText.RegisterShipper,
                    new
                    {
                        SNAME = entity.sName,
                        SADDRESS = entity.sAddress,
                        NVEHICLEID = entity.nVehicleId,
                        SEMAIL = entity.sEmail,
                        SPHONE = entity.sPhone,                       
                        SUSERNAME = entity.sUserName,
                        SPASSWORD = entity.sPassword
                    });
            });
        }

        public Client LoginClient(string SUSERNAME, string SPASSWORD)
        {
            var pUser = ExecuteCommand<Client>(_connStr, conn =>
                 conn.Query<Client>(_commandText.LoginClient, new { @SUSERNAME = SUSERNAME, @SPASSWORD = SPASSWORD }).SingleOrDefault());
            return pUser;
        }

        public List<Order> GetOrdersByClient (decimal nClientId) 
        {
            
            var query = ExecuteCommand(_connStr,
                   conn => conn.Query<Order>(_commandText.GetOrdersByClient, new { @NCLIENTID = nClientId})).ToList();
            return query;
            
        }

        public List<Load> GetLoadsByOrderId(decimal nOrderId)
        {

            var query = ExecuteCommand(_connStr,
                   conn => conn.Query<Load>(_commandText.GetLoadsByOrderId, new { @NORDERID = nOrderId })).ToList();
            return query;

        }
        public void AssignLoad(string map) 
        {
            string[] input = map.Split(",");
             

            int nShipperId = int.Parse(input[0]);
            int nLoadId = int.Parse(input[1]);

            var query = ExecuteCommand(_connStr,
                   conn => conn.Query<Load>(_commandText.AssignLoad, new {@NSHIPPERID = nShipperId, @NLOADID = nLoadId }));

            
        }
        public List<Load> GetAvailableLoads()
        {

            var query = ExecuteCommand(_connStr,
                   conn => conn.Query<Load>(_commandText.GetAvailableLoads)).ToList();
            return query;

        }



        /*  public Shipper Authenticate(string username, string password)
          {
              var user = ExecuteCommand<Shipper>(_connStr, conn =>
                   conn.Query<Shipper>(_commandText.GetShipperByUserName, new { @SUSERNAME = SUSERNAME }).SingleOrDefault());

              // return null if user not found
              if (user == null)
                  return null;

              // authentication successful so generate jwt token
              var tokenHandler = new JwtSecurityTokenHandler();
              var key = System.Text.Encoding.ASCII.GetBytes(_appSettings.Secret);
              var tokenDescriptor = new SecurityTokenDescriptor
              {
                  Subject = new ClaimsIdentity(new Claim[]
                  {
                      new Claim(ClaimTypes.Name, user.Id.ToString())
                  }),
                  Expires = DateTime.UtcNow.AddDays(7),
                  SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
              };
              var token = tokenHandler.CreateToken(tokenDescriptor);
              user.Token = tokenHandler.WriteToken(token);

              return user;
          }*/
    }
}

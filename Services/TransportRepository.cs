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
using GoogleMaps.LocationServices;


namespace TransportBE.Services
{
    public class TransportRepository : ITransportRepository
    {
        private const string gMapsApiKey = "AIzaSyBeCFketdO1G0mIkVJRbpos_JvYROwxV_k";

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


        

        public  int PostOrder(Order entity)
        {
            var locationService = new GoogleLocationService(gMapsApiKey);
            List<Load> loadList;
            using (var connection = new SqlConnection(_connStr))
            {
                connection.Open();
                Order currentOrder = new Order
                {
                    DTPICKUPDATE = entity.DTPICKUPDATE,
                    DTDROPDATE = entity.DTDROPDATE,
                    NCLIENTID = entity.NCLIENTID,
                    SADDRESSPICKUP = entity.SADDRESSPICKUP,
                    SADDRESSDROP = entity.SADDRESSDROP,
                    NBOXID = entity.NBOXID,
                    NBOXCOUNT = entity.NBOXCOUNT,
                    NDISTANCE = entity.NDISTANCE,
                    NPICKUPLAT = entity.NPICKUPLAT,
                    NPICKUPLONG = entity.NPICKUPLONG,
                    NDROPLAT = entity.NDROPLAT,
                    NDROPLONG = entity.NDROPLONG
                };

                var insertedId = connection.Query<int>(_commandText.PostOrder, currentOrder).First();

                int id = (int)insertedId;
                loadList = LoadProcessor.GenerateLoadsFromOrder(id, currentOrder);
                if(loadList.Count == 0)
                {
                    double pickUpLatitude = (double)currentOrder.NPICKUPLAT;
                    double pickUpLongitude = (double)currentOrder.NPICKUPLONG;
                    double dropLatitude = (double)currentOrder.NDROPLAT;
                    double dropLongitude = (double)currentOrder.NDROPLONG;
                    
                    

                    var PickUpLocation = locationService.GetAddressFromLatLang(pickUpLatitude, pickUpLongitude);
                    string source = PickUpLocation.City.ToString();

                    var DropLocation = locationService.GetAddressFromLatLang(dropLatitude, dropLongitude);
                    string destination = DropLocation.City.ToString();

                    //InsertUnknownCity(source);
                    //InsertUnknownCity(destination);
                    //SetOrderStatusToGridUnknown(id);
                    
                    return 1;
                }
                List<decimal> insertedLegs = new List<decimal>();

                int LoadId = 99999999;
                decimal leg = 99;
                foreach (Load genLoad in loadList)
                {
                    ExecuteCommand(_connStr, conn =>
                    {
                        var insertedLoadId = connection.Query<int>(_commandText.PostLoad, genLoad).First();
                        LoadId = insertedId;
                        

                    });
                    

                    var Location = locationService.GetLatLongFromAddress(genLoad.SADDRESSPICKUP );
                    double latitude = Location.Latitude;
                    double longitude = Location.Longitude;

                    if (!insertedLegs.Contains(genLoad.NLEGID)) 
                    {
                        insertedLegs.Add(genLoad.NLEGID);
                        Waypoint waypoint = new Waypoint
                        {
                            NORDERID = id,
                            NLEGID = genLoad.NLEGID,
                            NLAT = (decimal)latitude,
                            NLONG = (decimal)longitude,
                            NLOADID = LoadId
                        };
                        leg = genLoad.NLEGID;
                        ExecuteCommand(_connStr, conn =>
                        {
                            var query = connection.Query<Waypoint>(_commandText.PostWaypoint, waypoint);
                        });
                    }
                    

                }
                var dropLocation = locationService.GetLatLongFromAddress(currentOrder.SADDRESSDROP );
                double droplatitude = dropLocation.Latitude;
                double droplongitude = dropLocation.Longitude;
                Waypoint dropwaypoint = new Waypoint
                {
                    NORDERID = id,
                    NLEGID = (leg + 1),
                    NLAT = (decimal)droplatitude,
                    NLONG = (decimal)droplongitude,
                    NLOADID = LoadId
                };
                ExecuteCommand(_connStr, conn =>
                {
                    var query = connection.Query<Waypoint>(_commandText.PostWaypoint, dropwaypoint);
                });
            }
            return 0;
        }

        public Client CheckClientUsernameExists(string SUSERNAME)
        {
            var pUser = ExecuteCommand<Client>(_connStr, conn =>
                 conn.Query<Client>(_commandText.CheckClientUsernameExists, new { @SUSERNAME = SUSERNAME }).SingleOrDefault());
            return pUser;
        }

        public Shipper CheckShipperUsernameExists(string SUSERNAME)
        {
            var pUser = ExecuteCommand<Shipper>(_connStr, conn =>
                 conn.Query<Shipper>(_commandText.CheckShipperUsernameExists, new { @SUSERNAME = SUSERNAME }).SingleOrDefault());
            return pUser;
        }

        public Shipper LoginShipper(string SUSERNAME, string SPASSWORD)
        {
            var pUser = ExecuteCommand<Shipper>(_connStr, conn =>
                 conn.Query<Shipper>(_commandText.LoginShipper, new { @SUSERNAME = SUSERNAME, @SPASSWORD= SPASSWORD}).SingleOrDefault());
            return pUser;
        }

        public void ClientRegister(Client entity)
        {
            ExecuteCommand(_connStr, conn =>
            {
                var query = conn.Query<Client>(_commandText.ClientRegister,
                    new
                    {
                        SNAME = entity.sName,
                        SADDRESS = entity.sAddress,                       
                        SEMAIL = entity.sEmail,
                        SPHONE = entity.sPhone,
                        SUSERNAME = entity.sUsername,
                        SPASSWORD = entity.sPassword
                    });
            });
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
                        //NVEHICLEID = entity.nVehicleId,
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

        public List<Load> GetLoadsByShipperId(decimal nShipperId)
        {

            var query = ExecuteCommand(_connStr,
                   conn => conn.Query<Load>(_commandText.GetLoadsByShipperId, new { @NSHIPPERID = nShipperId })).ToList();
            return query;

        }

        
        public int SetLoadStatusToDelivered(Load entity)
        {

            var query = ExecuteCommand(_connStr,
                   conn => conn.Query<int>(_commandText.SetLoadStatusToDelivered, new { @NSHIPPERID = entity.NSHIPPERID, @NLOADID = entity.NLOADID }).SingleOrDefault());
            int retValue = (int)query;
            if (retValue == 0)
            {
                return 1;
            }
            else
            {
                return 0;
            }

        }

        public int AssignLoad(Load entity) 
        {                         

            var query = ExecuteCommand(_connStr,
                   conn => conn.Query<int>(_commandText.AssignLoad, new {@NSHIPPERID = entity.NSHIPPERID, @NLOADID = entity.NLOADID }).SingleOrDefault());
            int retValue = (int)query;
            if (retValue == 0) 
            {
                return 1;
            }
            else 
            {
                return 0;
            }
            
        }

        public List<Load> GetAvailableLoads()
        {

            var query = ExecuteCommand(_connStr,
                   conn => conn.Query<Load>(_commandText.GetAvailableLoads)).ToList();
            return query;

        }
        

        public List<Waypoint> GetOrderRouteWithWayPoints(decimal nOrderId)
        {

            var query = ExecuteCommand(_connStr,
                   conn => conn.Query<Waypoint>(_commandText.GetOrderRouteWithWayPoints, new { @NORDERID = nOrderId })).ToList();
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

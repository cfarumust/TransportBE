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
using TransportBE.Services;
using Microsoft.IdentityModel.Protocols;

namespace TransportBE.Models.DataOperators
{
    public class GridProcessor
    {        

        internal const string connStr = "Server=ATS007;Database=transport;User Id=qsys; Password=qsys;";
        internal static string GetGridIdOfCitySql => "select TOP 1 SGRIDID from CITIES where SCITYNAME = @sCityName and SGRIDID != '99'";
        internal static string GetCitiesInGridCellSql => "select SCITYNAME, SGRIDID from CITIES where SGRIDID = @sGridId";


        private static void ExecuteCommand(string connStr, Action<SqlConnection> task)
        {
            using (var conn = new SqlConnection(connStr))
            {
                conn.Open();

                task(conn);
            }
        }
        private static T ExecuteCommand<T>(string connStr, Func<SqlConnection, T> task)
        {
            
            using (var conn = new SqlConnection(connStr))
            {
               
                conn.Open();
                return task(conn);
            }
        }

        internal static string GetGridIdOfCity(string cityName)
        {
            var gridId = ExecuteCommand<string>(connStr, conn =>
                 conn.Query<string>(GetGridIdOfCitySql, new { @sCityName = cityName }).SingleOrDefault());
            if (gridId != null) 
            {
                return gridId.ToString();
            }                
            return "NOK";
            
            
        }

        internal static  List<City> GetCitiesInGridCell(string gridId)
        {
            List<City> cityList = new List<City>();
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(connStr);

            using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
            {
                SqlCommand command = new SqlCommand(GetCitiesInGridCellSql, connection);
                command.Parameters.Add("@sGridId", System.Data.SqlDbType.NVarChar);
                command.Parameters["@sGridId"].Value = gridId;

                connection.Open();

                SqlDataReader reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        City city = new City();

                        if (!reader.IsDBNull(0))
                        {
                            city.sCityName = reader.GetString(0);
                        }
                        if (!reader.IsDBNull(1))
                        {
                            city.sGridId = reader.GetString(1);
                        }
                        cityList.Add(city);
                    }
                }
            }
            return cityList;
        }

    }
}

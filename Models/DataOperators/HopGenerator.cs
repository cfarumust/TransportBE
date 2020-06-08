using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TransportBE.Models.DTOs;
using GoogleMaps.LocationServices;
using Microsoft.IdentityModel.Tokens;
using TransportBE.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualBasic;

namespace TransportBE.Models.DataOperators
{
    public class HopGenerator
    {
        

        private const string gMapsApiKey = "AIzaSyBeCFketdO1G0mIkVJRbpos_JvYROwxV_k";
        public static List<Cell> GenerateHopsForTrip(double pickUpLatitude, double pickUpLongitude, double dropLatitude, double dropLongitude) 
        {
         
            List<Cell> listOfWaypoints = new List<Cell>();
            List<Cell> listofHopsIds = new List<Cell>();

            var locationService = new GoogleLocationService(gMapsApiKey);
            
            var PickUpLocation = locationService.GetAddressFromLatLang(pickUpLatitude, pickUpLongitude);
            string sourceCity = PickUpLocation.City.ToString();

            var DropLocation = locationService.GetAddressFromLatLang(dropLatitude, dropLongitude);
            string destinationCity = DropLocation.City.ToString();

            if ((sourceCity == "NOK") || (destinationCity == "NOK")) 
            {
                return listofHopsIds;
            }

            string sourceGridId = GetGridIdOfCity(sourceCity);
            string destinationGridId = GetGridIdOfCity(destinationCity);

            char originX = sourceGridId[0];
            char originY = sourceGridId[1];

            char destX = destinationGridId[0];
            char destY = destinationGridId[1];

            

            listOfWaypoints = GetWaypoints(originX, originY, destX, destY);
            listofHopsIds = GetHopIds(listOfWaypoints);
            
            
            
            

            return listofHopsIds;
        }
        private static List<Cell> GetHopIds(List<Cell> waypoints) 
        {
            List<Cell> hops = new List<Cell>();
            Cell lastCell = waypoints.First();
            string caseSwitch = "null";
            double distance = 0;
            foreach (Cell waypoint in waypoints)
            { 
                
                if ((waypoint.x == lastCell.x) && (waypoint.y > lastCell.y)) { caseSwitch = "N"; distance = distance + 100; lastCell.x = waypoint.x; lastCell.y = waypoint.y; }
                else if ((waypoint.x > lastCell.x) && (waypoint.y > lastCell.y)) { caseSwitch = "NE"; distance = distance + 150; lastCell.x = waypoint.x; lastCell.y = waypoint.y; }
                else if ((waypoint.x > lastCell.x) && (waypoint.y == lastCell.y)) { caseSwitch = "E"; distance = distance + 100; lastCell.x = waypoint.x; lastCell.y = waypoint.y; }
                else if ((waypoint.x > lastCell.x) && (waypoint.y < lastCell.y)) { caseSwitch = "SE"; distance = distance + 150; lastCell.x = waypoint.x; lastCell.y = waypoint.y; }
                else if ((waypoint.x == lastCell.x) && (waypoint.y < lastCell.y)) { caseSwitch = "S"; distance = distance + 100; lastCell.x = waypoint.x; lastCell.y = waypoint.y; }
                else if ((waypoint.x < lastCell.x) && (waypoint.y < lastCell.y)) { caseSwitch = "SW"; distance = distance + 150; lastCell.x = waypoint.x; lastCell.y = waypoint.y; }
                else if ((waypoint.x < lastCell.x) && (waypoint.y == lastCell.y)) { caseSwitch = "W"; distance = distance + 100; lastCell.x = waypoint.x; lastCell.y = waypoint.y; }
                else if ((waypoint.x < lastCell.x) && (waypoint.y > lastCell.y)) { caseSwitch = "NW"; distance = distance + 150; lastCell.x = waypoint.x; lastCell.y = waypoint.y; }

                if ((distance >= 250) && (distance < 350))
                {
                    hops.Add(waypoint);
                }
                if ((distance >= 550) && (distance < 650))
                {
                    hops.Add(waypoint);
                }
                if ((distance >= 850) && (distance < 950))
                {
                    hops.Add(waypoint);
                }
            }
            return hops;
        
        }
        private static string GetGridIdOfCity(string City)
        {
            string cityId = GridProcessor.GetGridIdOfCity(City);
            if(cityId != "NOK")
                return cityId;
            return "NOK";
        }

        private static List<Cell> GetWaypoints(char originX, char originY, char destX, char destY) 
        {
            int ox = int.Parse(originX.ToString());
            int oy = int.Parse(originY.ToString());
            int dx = int.Parse(destX.ToString());
            int dy = int.Parse(destY.ToString());

            double distance = 0.0;
            List<Cell> routeGridIds = new List<Cell>();
            routeGridIds.Add(new Cell { x = ox, y = oy });
          
            string caseSwitch = "none";

            if ((dx == ox) && (dy > oy)) { caseSwitch = "N"; }
            else if ((dx > ox) && (dy > oy)) { caseSwitch = "NE"; }
            else if ((dx > ox) && (dy == oy)) { caseSwitch = "E"; }
            else if ((dx > ox) && (dy < oy)) { caseSwitch = "SE"; }
            else if ((dx == ox) && (dy < oy)) { caseSwitch = "S"; }
            else if ((dx < ox) && (dy < oy)) { caseSwitch = "SW"; }
            else if ((dx < ox) && (dy == oy)) { caseSwitch = "W"; }
            else if ((dx < ox) && (dy > oy)) { caseSwitch = "NW"; }



            switch (caseSwitch) 
            {
                case "N":
                    while(dy >= oy) 
                    {
                        distance = distance + 100;
                        oy++;
                        Cell newCell = new Cell { x = ox, y = oy };
                        routeGridIds.Add(newCell);
                        if ((ox == dx) && (oy == dy))
                        { break; }
                    }
                    break;
                case "NE":
                    while ((dx >= ox) || (dy >= oy)) 
                    { 
                        if(dx != ox)
                        {
                            ox++;
                        }
                        if (dy != oy) 
                        {
                            oy++;
                        }
                        Cell newCell = new Cell { x = ox, y = oy };
                        routeGridIds.Add(newCell);
                        if ((ox == dx) && (oy == dy))
                        { break; }
                    }
                    break;
                case "E":
                    while(dx >= ox) 
                    {
                        ox++;
                        Cell newCell = new Cell { x = ox, y = oy };
                        routeGridIds.Add(newCell);
                        if ((ox == dx) && (oy == dy))
                        { break; }
                    }
                    break;
                case "SE":
                    while((dx >= ox) || (dy <= oy))
                    {
                       if (ox != dx) 
                       {
                            ox++;
                       }
                       if (dy != oy) 
                       {
                            oy--;
                       }
                        Cell newCell = new Cell { x = ox, y = oy };
                        routeGridIds.Add(newCell);
                        if((ox == dx)&&(oy == dy)) 
                        { break; }
                    }
                    break;
                case "S":
                    while (dy <= oy)
                    {
                        oy--;
                        Cell newCell = new Cell { x = ox, y = oy };
                        routeGridIds.Add(newCell);
                        if ((ox == dx) && (oy == dy))
                        { break; }
                    }
                    break;
                case "SW":
                    while ((dx <= ox) || (dy <= oy))
                    {
                        if (ox != dx)
                        {
                            ox--;
                        }
                        if (dy != oy)
                        {
                            oy--;
                        }
                        Cell newCell = new Cell { x = ox, y = oy };
                        routeGridIds.Add(newCell);
                        if ((ox == dx) && (oy == dy))
                        { break; }
                    }
                    break;
                case "W":
                    while (dx <= ox)
                    {
                        ox--;
                        Cell newCell = new Cell { x = ox, y = oy };
                        routeGridIds.Add(newCell);
                        if ((ox == dx) && (oy == dy))
                        { break; }
                    }
                    break;
                case "NW":
                    while ((dx <= ox) || (dy >= oy))
                    {
                        if (ox != dx)
                        {
                            ox--;
                        }
                        if (dy != oy)
                        {
                            oy++;
                        }
                        Cell newCell = new Cell { x = ox, y = oy };
                        routeGridIds.Add(newCell);
                        if ((ox == dx) && (oy == dy))
                        { break; }
                    }
                    break;
                default:
                    break;

            }
           

            return routeGridIds;
        }
        
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using TransportBE.Models.DTOs;
using GoogleMaps.LocationServices;

namespace TransportBE.Models.DataOperators
{
    public class LoadProcessor
    {
        private const string gMapsApiKey = "AIzaSyBeCFketdO1G0mIkVJRbpos_JvYROwxV_k";

  
        internal static  List<Load> GenerateLoadsFromOrder(int id, Order order)
        {                                   
            double pickUpLatitude =  (double)order.NPICKUPLAT;
            double pickUpLongitude = (double)order.NPICKUPLONG;
            double dropLatitude =    (double)order.NDROPLAT;
            double dropLongitude =   (double)order.NDROPLONG;

            List<GeoTag> RouteWithWaypoints = new List<GeoTag>();
           
            List<Load> GeneratedLoads = new List<Load>();
            List<Cell> hops = new List<Cell>();

            var locationService = new GoogleLocationService(gMapsApiKey);

            var PickUpLocation = locationService.GetAddressFromLatLang(pickUpLatitude, pickUpLongitude);
            string sourceAddress = PickUpLocation.Address.ToString();

            var DropLocation = locationService.GetAddressFromLatLang(dropLatitude, dropLongitude);
            string destinationAddress = DropLocation.Address.ToString();

            decimal distance = order.NDISTANCE;
            RouteWithWaypoints = getWaypointsOnSlope(pickUpLatitude, pickUpLongitude, dropLatitude, dropLongitude, distance);

            decimal MaxBoxCount = 8;

            decimal BoxCount = order.NBOXCOUNT;

            int vehiclesRequired = (int)Math.Ceiling(BoxCount / MaxBoxCount);
            int boxesPerVehicle = (int)(BoxCount / vehiclesRequired);
            int boxesLeft = (int)(order.NBOXCOUNT - (boxesPerVehicle*vehiclesRequired));

            if (distance < 300)
            {
  
                if (BoxCount > MaxBoxCount)
                {
                    int trip = 0;
                    for (int i = 1; i <= vehiclesRequired; i++)
                    {
                       
                            Load load = new Load
                            {
                                NORDERID = id,
                                NBOXCOUNT = boxesPerVehicle+boxesLeft,
                                NBOXID = order.NBOXID,
                                SADDRESSPICKUP = sourceAddress,
                                SADDRESSDROP = destinationAddress,
                                NLEGID = trip,
                                NISMULTIVEHICLELOAD = i

                            };
                            boxesLeft = 0;
                            GeneratedLoads.Add(load); 
                    }
                }
                else
                {
                    int trip = 0;
                    Load load = new Load
                    {
                        NORDERID = id,
                        NBOXCOUNT = order.NBOXCOUNT,
                        NBOXID = order.NBOXID,
                        SADDRESSPICKUP = sourceAddress,
                        SADDRESSDROP = destinationAddress,
                        NLEGID = trip,
                        NISMULTIVEHICLELOAD = 1

                    };
                    GeneratedLoads.Add(load);
                }

            }
            else
            {
                
                //hops = HopGenerator.GenerateHopsForTrip(pickUpLatitude, pickUpLongitude, dropLatitude, dropLongitude);
                int hopCount = RouteWithWaypoints.Count();
                
                if (hopCount != 0) 
                {
                    int trips = hopCount - 1;
                 

                    if (BoxCount > MaxBoxCount)
                    {
                        //generate trips 
                        for (int trip = 0; trip < trips; trip++)
                        {
                            //load per vehicle
                            
                            for (int i = 1; i <= vehiclesRequired; i++)
                            {
                                GeoTag pick = RouteWithWaypoints[trip];
                                GeoTag drop = RouteWithWaypoints[trip+1];
                                var PickUpLoc = locationService.GetAddressFromLatLang(pick.latitude, pick.longitude);
                                string sourceAdd = PickUpLoc.City.ToString();

                                var DropLoc = locationService.GetAddressFromLatLang(drop.latitude, drop.longitude);
                                string dropAdd = DropLoc.City.ToString();

                                Load load = new Load
                                {
                                    NORDERID = id,
                                    NBOXCOUNT = boxesPerVehicle+boxesLeft,
                                    NBOXID = order.NBOXID,
                                    SADDRESSPICKUP = sourceAdd,
                                    SADDRESSDROP = dropAdd,
                                    NLEGID = trip,
                                    NISMULTIVEHICLELOAD = i 

                                };
                                boxesLeft = 0;
                                GeneratedLoads.Add(load);
                                //boxesAllocatedTovehicles += boxesPerVehicle;
                            }
                        }
                    }
                    else
                    {
                        for (int trip = 0; trip < trips; trip++)
                        {
                            GeoTag pick = RouteWithWaypoints[trip];
                            GeoTag drop = RouteWithWaypoints[trip + 1];
                            var PickUpLoc = locationService.GetAddressFromLatLang(pick.latitude, pick.longitude);
                            string sourceAdd = PickUpLoc.City.ToString();

                            var DropLoc = locationService.GetAddressFromLatLang(drop.latitude, drop.longitude);
                            string dropAdd = DropLoc.City.ToString();

                            Load load = new Load
                            {
                                NORDERID = id,
                                NBOXCOUNT = boxesPerVehicle+boxesLeft,
                                NBOXID = order.NBOXID,
                                SADDRESSPICKUP = sourceAdd,
                                SADDRESSDROP = dropAdd,
                                NLEGID = trip,
                                NISMULTIVEHICLELOAD = 1

                            };
                            boxesLeft = 0;
                            GeneratedLoads.Add(load);
                        }
                    }
                }
                else
                {
                    return GeneratedLoads;
                }
                

            }

            return GeneratedLoads;
        }

        private static List<GeoTag> getWaypointsOnSlope(double pickUpLatitude, double pickUpLongitude, double dropLatitude, double dropLongitude, decimal distance)
        {
            List<GeoTag> hopPoints = new List<GeoTag>();
            
            double yDiff = dropLatitude - pickUpLatitude;
            double xDiff =  dropLongitude - pickUpLongitude;
            Vector originalVector = new Vector((decimal)xDiff, (decimal)yDiff);

            double m = yDiff / xDiff;
            double totalDisplacement= Math.Sqrt((xDiff * xDiff) + (yDiff * yDiff));
            
            GeoTag source = new GeoTag { latitude = pickUpLatitude, longitude = pickUpLongitude };
            GeoTag destination = new GeoTag { latitude = dropLatitude, longitude = dropLongitude };

            var locationService = new GoogleLocationService(gMapsApiKey);

            var DestinationAddress = locationService.GetAddressFromLatLang(dropLatitude, dropLongitude);


            hopPoints.Add(source);
            totalDisplacement = Math.Round(totalDisplacement, 2);
            if (distance > 300) 
            {
                decimal routeSplitter = (distance / 300);
                decimal dPart = routeSplitter % 1.0m;
                decimal hops;
                

                if (dPart > (decimal)0.17)
                {
                    hops = Math.Ceiling(routeSplitter) - 1;
                }
                else
                {
                    hops = Math.Floor(routeSplitter) - 1;
                }

                double routeLegs = (double)hops + 1;

                double segment = totalDisplacement / routeLegs;
                segment = Math.Round(segment, 2);
                double coveredDistance = 0;

                double distanceLeft = (double)distance;

                GeoTag start = source;

                while ((coveredDistance < (double)distance) ) 
                {

                    
                    GeoTag coordinates = PrintPoints(start, (double)segment, m, originalVector);
                   
                    
                    //coveredDistance = coveredDistance + segment;
                    var PickUpLocation = locationService.GetAddressFromLatLang(start.latitude, start.longitude);
                    
                    var DropLocation = locationService.GetAddressFromLatLang(coordinates.latitude, coordinates.longitude);

                    var directions = locationService.GetDirections(PickUpLocation, DropLocation);
                    start = coordinates;
                    String[] split = directions.Distance.Split(" ");

                    var directionsToDestination = locationService.GetDirections(PickUpLocation, DestinationAddress);
                    String[] distanceToDest = directionsToDestination.Distance.Split(" ");

                    distanceLeft = Double.Parse(distanceToDest[0]);

                    coveredDistance += Double.Parse(split[0]);

                    
                    if (distanceLeft > 150) 
                    {
                        hopPoints.Add(coordinates);
                    }
                }
                
            }
            else 
            {
                hopPoints.Add(destination);
            }
            



            return hopPoints;
        }

        private static GeoTag PrintPoints(GeoTag source, double distance, double slope, Vector original)
        {
            // m is the slope of line, and the  
            // required Point lies distance l  
            // away from the source Point 
            GeoTag o = new GeoTag();
            GeoTag d = new GeoTag();

            List <GeoTag> cities_on_line = new List<GeoTag>();        
            // slope is 0 
            if (slope == 0)
            {
                o.longitude = source.longitude + distance;
                o.latitude = source.latitude;

                d.longitude = source.longitude - distance;
                d.latitude = source.latitude;

                Vector vO = new Vector(((decimal)(o.longitude - source.longitude)), ((decimal)(o.latitude - source.latitude)));
                Vector vD = new Vector(((decimal)(d.longitude - source.longitude)), ((decimal)(d.latitude - source.latitude)));

                if (original.x == vO.x && original.y == vO.y) {
                    return o;
                }
                if (original.x == vD.x && original.y == vD.y)
                {
                    return d;
                }
            }

            // if slope is infinte 
            else if (double.IsInfinity(slope))
            {
                o.longitude = source.longitude;
                o.latitude = source.latitude + distance;

                d.longitude = source.longitude;
                d.latitude = source.latitude - distance;
                decimal originalslope = (decimal)slope;
                decimal newSloped = (decimal)((d.latitude - source.latitude) / (d.longitude - source.longitude));

                Vector vO = new Vector(((decimal)(o.longitude - source.longitude)), ((decimal)(o.latitude - source.latitude)));
                Vector vD = new Vector(((decimal)(d.longitude - source.longitude)), ((decimal)(d.latitude - source.latitude)));

                if (original.x == vO.x && original.y == vO.y)
                {
                    return o;
                }
                if (original.x == vD.x && original.y == vD.y)
                {
                    return d;
                }
            }
            else
            {
                double dx = (distance / Math.Sqrt(1 + (slope * slope)));
                double dy = slope * dx;
                o.longitude = source.longitude - dx;
                o.latitude = source.latitude - dy;
                d.longitude = source.longitude + dx;
                d.latitude = source.latitude + dy;
                decimal originalslope = (decimal)slope;
                decimal newSloped = (decimal) ((d.latitude - source.latitude) / (d.longitude - source.longitude));

                decimal newSlopeo = (decimal)((o.latitude - source.latitude) / (o.longitude - source.longitude));

                Vector vO = new Vector(((decimal)(o.longitude - source.longitude)), ((decimal)(o.latitude - source.latitude)));
                Vector vD = new Vector(((decimal)(d.longitude - source.longitude)), ((decimal)(d.latitude - source.latitude)));

                if (original.x == vO.x && original.y == vO.y)
                {
                    return o;
                }
                if (original.x == vD.x && original.y == vD.y)
                {
                    return d;
                }

            }
            return new GeoTag {latitude= 0, longitude= 0 };
        }
    }  
}


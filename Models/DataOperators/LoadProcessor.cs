using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
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

            List<Load> GeneratedLoads = new List<Load>();
            List<Cell> hops = new List<Cell>();

            var locationService = new GoogleLocationService(gMapsApiKey);

            var PickUpLocation = locationService.GetAddressFromLatLang(pickUpLatitude, pickUpLongitude);
            string source = PickUpLocation.City.ToString();

            var DropLocation = locationService.GetAddressFromLatLang(dropLatitude, dropLongitude);
            string destination = DropLocation.City.ToString();

            List<City> places = new List<City>();
            

            string sourceid = GridProcessor.GetGridIdOfCity(source);
            string destid = GridProcessor.GetGridIdOfCity(destination);
            if ((sourceid == "NOK" ) || (destid == "NOK")) 
            {
                
                return GeneratedLoads;
            }
                
            
            City originCity = new City { sCityName = source, sGridId = sourceid };
            City destCity = new City { sCityName = destination, sGridId = destid };

            places.Add(originCity);


            
            //Will get distance from Order
            decimal distance = order.NDISTANCE;

            decimal MaxBoxCount = 8;

            decimal BoxCount = order.NBOXCOUNT;

            int vehiclesRequired = (int)Math.Ceiling(BoxCount / MaxBoxCount);
            int boxesPerVehicle = (int)(BoxCount / vehiclesRequired);

            if (distance < 300)
            {
                places.Add(destCity);
                if (BoxCount > MaxBoxCount)
                {
                    int trip = 0;
                    for (int i = 1; i <= vehiclesRequired; i++)
                    {
                        Load load = new Load
                        {
                            NORDERID = id,
                            NBOXCOUNT = boxesPerVehicle,
                            NBOXID = order.NBOXID,
                            SADDRESSPICKUP = places[trip].sCityName,
                            SADDRESSDROP = places[trip + 1].sCityName,
                            NLEGID = trip,
                            NISMULTIVEHICLELOAD = 1,
                            SGRIDIDPICKUP = places[trip].sGridId,
                            SGRIDIDDROP = places[trip + 1].sGridId


                        };
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
                         SADDRESSPICKUP = places[trip].sCityName,
                            SADDRESSDROP = places[trip + 1].sCityName,
                            NLEGID = trip,
                            NISMULTIVEHICLELOAD = 1,
                            SGRIDIDPICKUP = places[trip].sGridId,
                            SGRIDIDDROP = places[trip + 1].sGridId

                    };
                    GeneratedLoads.Add(load);
                }

            }
            else
            {

                hops = HopGenerator.GenerateHopsForTrip(pickUpLatitude, pickUpLongitude, dropLatitude, dropLongitude);
                int hopCount = hops.Count();
                { 
                /*List<City>[] AllHopCities = new List<City>[hopCount];
                //int a = 0;
                //foreach (Cell cell in hops)
                //{
                //
                //    string idx = cell.x.ToString();
                //    string idy = cell.y.ToString();
                //
                //    string HopCellId = idx + idy;
                //    AllHopCities[a] = GridProcessor.GetCitiesInGridCell(HopCellId);
                //    a++;
                //}*/
            }
                if (hops.Count != 0) 
                {
                    int trips = hopCount + 1;
                    int boxesAllocatedTovehicles = 0;
                    int totalVehiclesRequred = vehiclesRequired * 2;




                    //list out  hop points
                    List<String> hopPointsCitys = new List<string>();



                    List<City>[] AllHopCities = new List<City>[hopCount];
                    int a = 0;
                    foreach (Cell cell in hops)
                    {

                        string idx = cell.x.ToString();
                        string idy = cell.y.ToString();

                        string HopCellId = idx + idy;
                        AllHopCities[a] = GridProcessor.GetCitiesInGridCell(HopCellId);
                        a++;
                    }

                    List<string> usedCell = new List<string>();


                    //Hops
                    for (int i = 0; i < hopCount; i++)
                    {

                        foreach (City city in AllHopCities[i])
                        {
                            if (!usedCell.Contains(city.sGridId))
                            {
                                usedCell.Add(city.sGridId);
                                places.Add(city);
                            }
                        }

                    }


                    places.Add(destCity);


                    if (BoxCount > MaxBoxCount)
                    {
                        //generate trips 
                        for (int trip = 0; trip < trips; trip++)
                        {
                            //load per vehicle

                            for (int i = 1; i <= vehiclesRequired; i++)
                            {
                                Load load = new Load
                                {
                                    NORDERID = id,
                                    NBOXCOUNT = boxesPerVehicle,
                                    NBOXID = order.NBOXID,
                                    SADDRESSPICKUP = places[trip].sCityName,
                                    SADDRESSDROP = places[trip + 1].sCityName,
                                    NLEGID = trip,
                                    NISMULTIVEHICLELOAD = i,
                                    SGRIDIDPICKUP = places[trip].sGridId,
                                    SGRIDIDDROP = places[trip + 1].sGridId

                                };

                                GeneratedLoads.Add(load);
                                //boxesAllocatedTovehicles += boxesPerVehicle;
                            }
                        }
                    }
                    else
                    {
                        for (int trip = 0; trip < trips; trip++)
                        {
                            Load load = new Load
                            {
                                NORDERID = id,
                                NBOXCOUNT = order.NBOXCOUNT,
                                NBOXID = order.NBOXID,
                                SADDRESSPICKUP = places[trip].sCityName,
                                SADDRESSDROP = places[trip + 1].sCityName,
                                NLEGID = trip,
                                NISMULTIVEHICLELOAD = 1,
                                SGRIDIDPICKUP = places[trip].sGridId,
                                SGRIDIDDROP = places[trip + 1].sGridId

                            };
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
    }
   
    }


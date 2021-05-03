using System;
using System.Collections.Generic;
using System.Text;
using One_Sgp4;
using System.Net;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;

namespace NearSatMobile
{
    public static class SatelliteCalculations
    {

        public static string pathToUpdateFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "LastUpdateFile.txt");
        public static string pathToTLEData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TLEData.txt");

        public static async Task<Satellite> FindClosestSatellite(double userLat, double userLong, double userHeight)
        {
            List<Tle> tleList = ParserTLE.ParseFile(pathToTLEData);

            Vector3 difference = new Vector3(0, 0, 0);

            Satellite closest = new Satellite("Temporary Satellite. If you see this, you done messed up", double.MaxValue);
            List<Satellite> satellites = new List<Satellite>();

            One_Sgp4.Coordinate observer = new Coordinate(userLat, userLong, userHeight);

            for (int i = 0; i < tleList.Count; i++)
            {
                //Create Time points
                EpochTime startTime = new EpochTime(DateTime.UtcNow);
                EpochTime stopTime = new EpochTime(DateTime.UtcNow.AddHours(1));

                One_Sgp4.Sgp4 sgp4Propagator = new Sgp4(tleList[i], Sgp4.wgsConstant.WGS_84);

                try
                {
                    sgp4Propagator.runSgp4Cal(startTime, stopTime, 0.5);
                }
                catch
                {
                    Console.WriteLine("Something went wrong with " + tleList[i].getName());
                }

                List<One_Sgp4.Sgp4Data> resultDataList = new List<Sgp4Data>();
                //Return Results containing satellite Position x,y,z (ECI-Coordinates in Km) and Velocity x_d, y_d, z_d (ECI-Coordinates km/s) 
                resultDataList = sgp4Propagator.getResults();

                try
                {
                    double localSiderealTime = startTime.getLocalSiderealTime(observer.getLongitude());

                    Sgp4Data grrPoint = One_Sgp4.SatFunctions.getSatPositionAtTime(tleList[0], startTime, Sgp4.wgsConstant.WGS_84);

                    One_Sgp4.Point3d sphCoordsSat = One_Sgp4.SatFunctions.calcSphericalCoordinate(observer, startTime, resultDataList[0]);

                    double distance = sphCoordsSat.x;

                    Satellite satellite = new Satellite(tleList[i].getName(), distance);
                    satellites.Add(satellite);

                    if (distance < closest.distance)
                    {
                        closest = satellite;
                    }
                }
                catch
                {
                    //Something went wrong with a satellite, skipped
                }
            }

            closest.distance = Math.Round(closest.distance, 2);

            return closest;

            //Console.WriteLine("Done: " + satellites.Count + " satellites successfully analyzed from " + tleList.Count + " in the dataset.");
            //Console.WriteLine("The closest active satellite is " + closest.name.Trim() + " which is " + Math.Round(closest.distance, 2).ToString() + " kilometres away from you. Time: " + DateTime.Now.ToString("HH:mm:ss"));
            //Console.WriteLine();
        }

        public static bool UpdateTLEData()
        {
            string url = "http://www.celestrak.com/NORAD/elements/active.txt";

            string htmlText;

            WebClient wb = new WebClient();
            htmlText = wb.DownloadString(url);

            using (var writer = new StreamWriter(File.Create(SatelliteCalculations.pathToTLEData)))
            {
                writer.Write(htmlText);
            }
            return true;
        }

    }
}

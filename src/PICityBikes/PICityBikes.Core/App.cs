using OSIsoft.AF.Asset;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PICityBikes.Core
{
    public class App
    {
        private PISystemManager piSystemManager = null;
        private Networks networks = null;
        private Dictionary<Network, List<BikeStation>> bikeStationsDic = null;
        private bool cont = true;
        private Task t = null;

        public App()
        {
            bikeStationsDic = new Dictionary<Network, List<BikeStation>>();
        }

        public void GetData()
        {

            networks = BikeAPIManager.GetAllCities();
            foreach (Network network in networks)
            {
                List<BikeStation> bikeStations = BikeAPIManager.GetCityBikeStations(network);
                bikeStationsDic.Add(network, bikeStations);
            }
        }

        public void Start()
        {
            int secondsToWait = Convert.ToInt32(ConfigurationManager.AppSettings["secondsToWait"]);
            bool createObjects = Convert.ToBoolean(ConfigurationManager.AppSettings["createObjects"]);
            LogManager.Instance.Info("Starting application...");
            CheckConnection();
            if (createObjects == true)
            {
                bool templatesCreated = CreateAFTemplates();
                GetData();
                bool treeCreated = CreateAFTree();
                bool pointsCreated = CreatePoints();
            }




            t = Task.Run(() =>
            {

                while (true)
                {
                    LogManager.Instance.Info("Starting new update values cycle");
                    try
                    {
                        if (cont == true)
                        {
                            UpdateValues();
                            LogManager.Instance.Info("Finish updating values");
                        }
                        else
                        {
                            break;
                        }
                        LogManager.Instance.Info("Waiting " + secondsToWait + " seconds");
                        for (int i = 0; i < secondsToWait; i++)
                        {
                            Thread.Sleep(1000);
                            if (cont == false)
                            {
                                break;
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        LogManager.Instance.Error(ex.Message);
                    }

                }
            });


        }

        private void UpdateValues()
        {
            AFValues cityValues = new AFValues();
            Networks networks = BikeAPIManager.GetAllCities();

            foreach (Network network in networks)
            {


                List<BikeStation> bikeStations = BikeAPIManager.GetCityBikeStations(network);
                AFElement cityElement = piSystemManager.GetCity(network);

                foreach (BikeStation bikeStation in bikeStations)
                {
                    if (cont == false)
                    {
                        break;
                    }
                    AFElement stationElement = cityElement.Elements[bikeStation.FixedName];
                    if (stationElement == null)
                    {
                        bool result = piSystemManager.CreateBikeStation(bikeStation, cityElement);
                        stationElement = cityElement.Elements[bikeStation.FixedName];
                    }

                    if (stationElement != null)
                    {
                        AFValues values = piSystemManager.GetBikeStationValues(bikeStation, stationElement);
                        if ((values != null) && (values.Count > 0))
                        {
                            cityValues.AddRange(values);
                        }
                    }

                }
                if (cont == false)
                {
                    break;
                }
            }
            piSystemManager.UpdateValues(cityValues);
        }

        private bool CreatePoints()
        {
            try
            {
                List<string> piPointNameList = new List<string>();
                foreach (Network network in networks)
                {
                    List<BikeStation> bikeStations = bikeStationsDic[network];
                    AFElement cityElement = piSystemManager.GetCity(network);
                    foreach (BikeStation bikeStation in bikeStations)
                    {
                        AFElement stationElement = cityElement.Elements[bikeStation.FixedName];
                        if (stationElement != null)
                        {
                            string point1Name = "CityBikes_" + cityElement.Name + "_" + stationElement.Name + "_" + "Free Bikes";
                            string point2Name = "CityBikes_" + cityElement.Name + "_" + stationElement.Name + "_" + "Empty Slots";
                            if (piPointNameList.Where(p => p == point1Name).Count() == 0)
                            {
                                piPointNameList.Add(point1Name);
                            }
                            if (piPointNameList.Where(p => p == point2Name).Count() == 0)
                            {
                                piPointNameList.Add(point2Name);
                            }
                        }
                    }
                }

                if (piPointNameList.Count > 0)
                {
                    piSystemManager.CreateAllPIPoints(piPointNameList);
                }
                return true;
            }
            catch (Exception ex)
            {
                LogManager.Instance.Fatal("Error: Could not PI Points. " + ex.Message);
                throw ex;
            }
        }

        private bool CreateAFTree()
        {
            bool result = true;
            try
            {
                foreach (Network network in networks)
                {
                    List<BikeStation> bikeStations = bikeStationsDic[network];
                    AFElement cityElement = piSystemManager.CreateCity(network);
                    foreach (BikeStation bikeStation in bikeStations)
                    {
                        bool r = piSystemManager.CreateBikeStation(bikeStation, cityElement);
                        if (r == false)
                        {
                            result = false;
                        }
                    }
                }
                piSystemManager.Save();
                return result;

            }
            catch (Exception ex)
            {
                LogManager.Instance.Fatal("Error: Could not create AF Tree. " + ex.Message);
                throw ex;
            }
        }

        private void CheckConnection()
        {
            piSystemManager = new PISystemManager();
            string afDatabaseName = ConfigurationManager.AppSettings["afDatabase"];
            string afServerName = ConfigurationManager.AppSettings["afServer"];
            string piServerName = ConfigurationManager.AppSettings["piServer"];
            piSystemManager.Connect(piServerName, afDatabaseName, afServerName);
        }

        private bool CreateAFTemplates()
        {
            try
            {
                bool r1 = piSystemManager.CreateCityTemplate();
                bool r2 = piSystemManager.CreateBikeStationTemplate();
                piSystemManager.Save();
                return (r1 && r2);
            }
            catch (Exception ex)
            {
                LogManager.Instance.Fatal("Error: Could not create AF Element templates. " + ex.Message);
                throw ex;
            }
        }


        public void Stop()
        {
            LogManager.Instance.Info("Stopping application...");
            cont = false;
            t.Wait();
        }
    }
}

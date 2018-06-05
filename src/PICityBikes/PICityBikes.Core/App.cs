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
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private PISystemManager piSystemManager = null;
        private Networks networks = null;
        private Dictionary<Network, List<BikeStation>> bikeStationsDic = null;
        private bool cont = true;
        private Task t = null;

        public App()
        {

        }

        public void GetData()
        {
            bikeStationsDic = new Dictionary<Network, List<BikeStation>>();
            BikeAPIManager bikeApiManager = new BikeAPIManager();
            networks = bikeApiManager.GetAllCities().Result;
            int totalNetworks = networks.Count();
            int i = 1;

            foreach (Network network in networks)
            {
                List<BikeStation> bikeStations = bikeApiManager.GetCityBikeStations(network).Result;
                bikeStationsDic.Add(network, bikeStations);
                log.Info($"GetData() Processing {i++}/{totalNetworks}: {network.Name}");
            }
        }

        public void Start()
        {

            int secondsToWait = Convert.ToInt32(ConfigurationManager.AppSettings["secondsToWait"]);
            bool createObjects = Convert.ToBoolean(ConfigurationManager.AppSettings["createObjects"]);
            log.Info("Starting application...");



            t = Task.Run(() =>
            {
                SetupProxies();
                CheckConnection();
                if (createObjects == true)
                {
                    bool templatesCreated = CreateAFTemplates();
                    GetData();
                    bool treeCreated = CreateAFTree();
                    bool pointsCreated = CreatePoints();
                }

                while (true)
                {
                    log.Info("Starting new update values cycle");

                    if (cont == true)
                    {
                        UpdateValues();
                        log.Info("Finish updating values");
                    }
                    else
                    {
                        break;
                    }
                    log.Info("Waiting " + secondsToWait + " seconds");
                    for (int i = 0; i < secondsToWait; i++)
                    {
                        Thread.Sleep(1000);
                        if (cont == false)
                        {
                            break;
                        }
                    }
                }
            });
        }



        private void SetupProxies()
        {
            CustomHttpRequest.Instance = new CustomHttpRequest();
            CustomHttpRequest.Instance.SearchForProxies();
        }

        private void UpdateValues()
        {
            try
            {
                BikeAPIManager bikeApiManager = new BikeAPIManager();
                Networks networks = bikeApiManager.GetAllCities().Result;
                int i = 0;
                foreach (Network network in networks)
                {


                    List<BikeStation> bikeStations = bikeApiManager.GetCityBikeStations(network).Result;
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
                                piSystemManager.UpdateValues(values);
                            }
                        }

                    }
                    if (cont == false)
                    {
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Error updating values...", ex);
                SetupProxies();
            }

        }

        private bool CreatePoints()
        {


            try
            {
                piSystemManager.ConnectToPIDataArchive();
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
                            string point1Name = "CityBikes_" + network.Id + "_" + bikeStation.id + "_" + "Free Bikes";
                            string point2Name = "CityBikes_" + network.Id + "_" + bikeStation.id + "_" + "Empty Slots";
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

                    if (piPointNameList.Count > 0)
                    {
                        piSystemManager.CreateAllPIPoints(piPointNameList);
                        piPointNameList = new List<string>();
                    }
                }


                return true;
            }
            catch (Exception ex)
            {
                log.Fatal("Error: Could not create PI Points.", ex);
                throw ex;
            }
        }






        private bool CreateAFTree()
        {
            bool result = true;
            int i = 1;

            foreach (Network network in networks)
            {
                AFElement cityElement = piSystemManager.CreateCity(network);
                log.Info($"Create City Element() Processing {i++}/{networks.Count}: {network.Name} {cityElement.Name}");
            }

            piSystemManager.Save();
            foreach (Network network in networks)
            {

                List<BikeStation> bikeStations = bikeStationsDic[network];
                AFElement cityElement = piSystemManager.CreateCity(network);
                log.Info($"Create City Station Elements() Processing {i++}/{networks.Count}: {network.Name} {cityElement.Name}");
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
                log.Fatal("Error: Could not create AF Element templates. " + ex.Message);
                throw ex;
            }
        }


        public void Stop()
        {
            log.Info("Stopping application...");
            cont = false;
            t.Wait();
        }
    }
}

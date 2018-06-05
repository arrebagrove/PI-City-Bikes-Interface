using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;


namespace PICityBikes.Core
{
    public class BikeAPIManager
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public async Task<Networks> GetAllCities()
        {
            Networks networks = new Networks();
            Dictionary<string, string> test = new Dictionary<string, string>();
            dynamic dObj = await CustomHttpRequest.Instance.GetAllNetworksData();

            for (int i = 0; i < dObj["networks"].Count; i++)
            {
                Network network = new Network();
                network.CityNumber = i + 1;
                if (dObj["networks"][i].company == null)
                {
                    network.Company = null;
                }
                else if (dObj["networks"][i].company.Type.ToString() == "Array")
                {
                    for (int j = 0; j < dObj["networks"][i].company.Count; j++)
                    {

                        network.Company.Add(dObj["networks"][i].company[j].Value.ToString());
                    }
                }
                else
                {
                    network.Company.Add(dObj["networks"][i].company.Value.ToString());

                }
                network.Href = dObj["networks"][i].href.Value.ToString();
                network.Id = dObj["networks"][i].id.Value.ToString();
                network.Name = dObj["networks"][i].name.Value.ToString();
                network.Location.City = dObj["networks"][i].location.city.Value.ToString();
                network.Location.Latitude = Convert.ToDouble(dObj["networks"][i].location.latitude.Value.ToString());
                network.Location.Longitude = Convert.ToDouble(dObj["networks"][i].location.longitude.Value.ToString());
                network.Location.Country = dObj["networks"][i].location.country.Value.ToString();
                if (test.Keys.ToList().Where(k => k == network.FixedName).Count() == 0)
                {
                    test.Add(network.FixedName, "a");
                    networks.Add(network);
                }
            }
            return networks;
        }




        public static string ToFirstUpperCase(string name)
        {
            return name[0].ToString().ToUpper() + name.Substring(1);
        }

        public async Task<List<BikeStation>> GetCityBikeStations(Network network)
        {

            dynamic dObj = await CustomHttpRequest.Instance.GetNetworkStationsData(network);
            if (dObj == null)
            {
                return new List<BikeStation>();
            }
            List<BikeStation> bikeStationList = new List<BikeStation>();
            if (dObj["network"]["stations"] != null)
            {
                for (int i = 0; i < dObj["network"]["stations"].Count; i++)
                {
                    BikeStation bikeStation = new BikeStation();
                    bikeStation.StationNumber = i + 1;
                    if (dObj["network"]["stations"][i].empty_slots != null)
                    {
                        bikeStation.empty_slots = Convert.ToInt32(dObj["network"]["stations"][i].empty_slots.Value.ToString());
                    }
                    if (dObj["network"]["stations"][i].free_bikes != null)
                    {
                        bikeStation.free_bikes = Convert.ToInt32(dObj["network"]["stations"][i].free_bikes.Value.ToString());
                    }
                    bikeStation.id = dObj["network"]["stations"][i].id.Value.ToString();
                    bikeStation.latitude = Convert.ToDouble(dObj["network"]["stations"][i].latitude.Value.ToString());
                    bikeStation.longitude = Convert.ToDouble(dObj["network"]["stations"][i].longitude.Value.ToString());
                    bikeStation.name = dObj["network"]["stations"][i].name.Value.ToString();
                    bikeStation.timestamp = DateTime.Now;
                    bikeStationList.Add(bikeStation);
                    //if (IsValid(bikeStation.name))
                    //{
                    //    bikeStationList.Add(bikeStation);
                    //}
                    //else
                    //{
                    //    log.Info($"{bikeStation.name} is not a valid name. ");
                    //}

                }
            }

            return bikeStationList;
        }

        private bool IsValid(string name)
        {
            name = name
              .Replace("`", string.Empty)
              .Replace("'", string.Empty)
              .Replace(":", string.Empty)
              .Replace("°", string.Empty)
              .Replace(".", string.Empty)
              .Replace("*", string.Empty)
              .Replace("!", string.Empty)
              .Replace("+", string.Empty)
              .Replace(",", string.Empty)
              .Replace("#", string.Empty)
              .Replace("-", string.Empty)
              .Replace("'", string.Empty)
              .Replace(".", string.Empty).Trim();
            return name.All(c => Char.IsLetterOrDigit(c) || c.Equals('-') || c.Equals('@') || c.Equals(' ') || c.Equals('.') || c.Equals('\\') || c.Equals('"') || c.Equals('\'') || c.Equals('(') || c.Equals(')') || c.Equals('&') || c.Equals('|') || c.Equals('/') || c.Equals(',') || c.Equals('-') || c.Equals('_'));
        }
    }
}

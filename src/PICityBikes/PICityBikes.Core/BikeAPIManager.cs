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
        private static string baseUrl = "http://api.citybik.es";
        public static Networks GetAllCities()
        {
            string response = GetJsonReponse(baseUrl + "/v2/networks");
            dynamic dObj = JObject.Parse(response);
            Networks networks = new Networks();
            for (int i = 0; i < dObj["networks"].Count; i++)
            {
                Network network = new Network();

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
                networks.Add(network);

            }


            return networks;
        }


        public static List<BikeStation> GetCityBikeStations(Network network)
        {
            string response = GetJsonReponse(baseUrl + network.Href);

            dynamic dObj = JObject.Parse(response);
            List<BikeStation> bikeStationList = new List<BikeStation>();
            if (dObj["network"]["stations"] != null)
            {
                for (int i = 0; i < dObj["network"]["stations"].Count; i++)
                {
                    BikeStation bikeStation = new BikeStation();
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
                }
            }

            return bikeStationList;
        }

        private static string GetJsonReponse(string url)
        {
            WebRequest request = WebRequest.Create(url);
            WebResponse response = request.GetResponse();
            StreamReader sw = new StreamReader(response.GetResponseStream());
            return sw.ReadToEnd();
        }
    }
}

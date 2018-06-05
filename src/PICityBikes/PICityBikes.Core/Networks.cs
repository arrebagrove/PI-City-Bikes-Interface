using System.Collections.Generic;
using System.Diagnostics;

namespace PICityBikes.Core
{

    public class Networks : List<Network>
    {

    }

    [DebuggerDisplay("Name = {Name}")]
    public class Network
    {
        public string CompanyString
        {
            get
            {
                string temp = string.Empty;
                if (Company == null || Company.Count == 0)
                {
                    return temp;
                }
                foreach (string str in Company)
                {
                    temp = temp + str + ", ";
                }
                return temp.Substring(0, temp.Length - 2);

            }

        }
        public List<string> Company { get; set; }
        public string Href { get; set; }
        public string Id { get; set; }
        public Location Location { get; set; }
        public string Name { get; set; }

        public string FixedName
        {
            get
            {
                return GetFixedName(this.Name) + "  " + GetFixedName(this.Location.City.ToString());

            }
        }

        public string GetFixedName(string name)
        {
            return name.Replace('ô', 'o')
                    .Replace('*', ' ')
                    .Replace('?', ' ')
                    .Replace(';', ' ')
                    .Replace('{', '(')
                    .Replace('}', ')')
                    .Replace('[', '(')
                    .Replace(']', ')')
                    .Replace('|', ' ')
                    .Replace('\'', ' ')
                    .Replace('\\', ' ')
                    .Replace('\'', ' ')
                    .Replace('"', ' ')
                    .Replace('/', ' ')
                    .Replace('`', ' ');
        }

        public int CityNumber { get; set; }

        public Network()
        {
            Location = new Location();
            Company = new List<string>();
        }

    }

    public class Location
    {
        public string City { get; set; }
        public string Country { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public Location()
        {

        }
    }
}

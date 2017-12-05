using System.Collections.Generic;


namespace PICityBikes.Core
{

    public class Networks : List<Network>
    {

    }

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
                return Name
                    .Replace('ô', 'o')
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
        }
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace PICityBikes.Core
{
    public class BikeStation
    {
        public int empty_slots { get; set; }
        public int free_bikes { get; set; }
        public string id { get; set; }
        public double latitude { get; set; }
        public double longitude { get; set; }
        public string name { get; set; }
        public DateTime timestamp { get; set; }

        public string FixedName
        {
            get
            {
                return name
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
    }
}

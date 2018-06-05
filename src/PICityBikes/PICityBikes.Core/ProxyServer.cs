using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PICityBikes.Core
{
    public class ProxyServer
    {
        public string IpAddress { get; set; }
        public int Port { get; set; }
        public bool Https { get; set; }

        public string Host
        {
            get
            {
                return $"{IpAddress}:{Port}";
            }
        }

        public bool Valid { get; internal set; }

        public ProxyServer()
        {

        }

        internal IWebProxy ToWebProxy()
        {
            IWebProxy proxy = new WebProxy(Host, false);
            return proxy;
        }
    }
}

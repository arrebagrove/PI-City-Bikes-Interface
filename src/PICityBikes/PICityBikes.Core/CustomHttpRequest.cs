using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace PICityBikes.Core
{
    public class CustomHttpRequest
    {
        private Stack<ProxyServer> proxyServers = null;
        private ProxyServer currentProxyServer = null;
        private static string baseUrl = "http://api.citybik.es";
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public CustomHttpRequest()
        {

        }

        public static CustomHttpRequest Instance { get; set; }




        public IWebProxy GetProxyServer()
        {
            if ((proxyServers == null) || (proxyServers.Count == 0))
            {
                return null;
            }
            else if (currentProxyServer == null) 
            {
                currentProxyServer = proxyServers.Pop();
            
            }
            return currentProxyServer.ToWebProxy();

        }

        private async Task<string> MakeRequest(string url)
        {
            while (true)
            {
                try
                {
                    IWebProxy proxy = GetProxyServer();
                    return await MakeRequest(url, proxy);
                }
                catch (Exception e)
                {
                    log.Error("Error", e);
                    currentProxyServer = null;
                }
            }
        }

        private async Task<string> MakeRequest(string url, IWebProxy proxy)
        {
            await Task.Delay(0);
            WebRequest request = WebRequest.Create(url);
            request.Timeout = 8000;
            request.Proxy = proxy;
            WebResponse response = request.GetResponse();
            StreamReader sw = new StreamReader(response.GetResponseStream());
            return sw.ReadToEnd();
        }

        internal async Task<dynamic> GetAllNetworksData()
        {

            string json = await MakeRequest(baseUrl + "/v2/networks");
            return JObject.Parse(json);
        }

        internal async Task<dynamic> GetNetworkStationsData(Network network)
        {
            string json = await MakeRequest(baseUrl + network.Href);
            return JObject.Parse(json);
        }

        internal void SearchForProxies()
        {

            var htmlWeb = new HtmlWeb();
            var htmlDoc = htmlWeb.Load("https://free-proxy-list.net");

            List<HtmlNode> serverRows = htmlDoc.DocumentNode.SelectNodes("//table[@id='proxylisttable']/tbody[1]/tr").ToList();
            proxyServers = new Stack<ProxyServer>();
            foreach (HtmlNode serverRow in serverRows)
            {
                ProxyServer proxyServer = new ProxyServer();
                proxyServer.IpAddress = serverRow.SelectSingleNode("td[1]").InnerText.Trim();
                proxyServer.Port = Convert.ToInt32(serverRow.SelectSingleNode("td[2]").InnerText.Trim());
                proxyServer.Https = serverRow.SelectSingleNode("td[7]").InnerText.Trim() == "yes";
                proxyServers.Push(proxyServer);
            }

            List<Task> tasks = new List<Task>();


            foreach (var proxyServer in proxyServers)
            {
                Task task = Task.Run(async () =>
                 {
                     try
                     {
                         string res = await MakeRequest(baseUrl, proxyServer.ToWebProxy());
                         proxyServer.Valid = true;
                         log.Info($"Proxy TRUE {proxyServer.Host}");
                     }
                     catch (Exception e)
                     {
                         proxyServer.Valid = false;
                         log.Info($"Proxy FALSE {proxyServer.Host}");
                     }
                 });
                tasks.Add(task);
            }


            Task.WaitAll(tasks.ToArray());
            var list = proxyServers.ToList().Where(s => s.Valid == true).ToList();
            proxyServers = new Stack<ProxyServer>(list);
            log.Info($"Finished testing the proxies...Found {proxyServers.Count()} valid proxies...");
        }
    }
}

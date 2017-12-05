using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Configuration.Install;
using PICityBikes.Core;


namespace PICityBikes.Service
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        private static void Main(string[] args)
        {
            if (Environment.UserInteractive)
            {
                App app = new App();

                string parameter = string.Concat(args);
                if (parameter == "--install")
                {
                    ManagedInstallerClass.InstallHelper(new[] { Assembly.GetExecutingAssembly().Location });
                }
                else if (parameter == "--uninstall")
                {
                    ManagedInstallerClass.InstallHelper(new[] { "/u", Assembly.GetExecutingAssembly().Location });
                }
                else
                {
                    app.Start();
                    Console.WriteLine("Press escape key to quit...");
                    while(true)
                    {
                        if (Console.ReadKey(true).Key == ConsoleKey.Escape)
                        {
                            Console.WriteLine("Stopping application...");
                            app.Stop();
                            break;
                        }
                    }
 
                }
            }
            else
            {
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[] 
                { 
                    new Service() 
                };
                ServiceBase.Run(ServicesToRun);
            }
        }
    }
}

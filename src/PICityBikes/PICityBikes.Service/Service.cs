using PICityBikes.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace PICityBikes.Service
{
    public partial class Service : ServiceBase
    {
        private App app; 
        public Service()
        {
            InitializeComponent();
            app = new App();
        }

        protected override void OnStart(string[] args)
        {
            app.Start();
        }

        protected override void OnStop()
        {
            app.Stop();
        }
    }
}

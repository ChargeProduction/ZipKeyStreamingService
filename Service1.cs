using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using ZipKeyStreamingService.Camera;
using ZipKeyStreamingService.Interface;

namespace ZipKeyStreamingService
{
    public partial class Service1 : ServiceBase
    {
        public Service1()
        {
            InitializeComponent();
        }
        public void onDebug()
        {
            OnStart(null);
        }

        protected override void OnStart(string[] args)
        {
            var ip = IPAddress.Parse("127.0.0.1");
            var port = 24456;

            if (args?.Length >= 1)
            {
                IPAddress.TryParse(args[0], out ip);
            }
            if (args?.Length >= 2)
            {
                Int32.TryParse(args[1], out port);
            }

            Server.Start(ip, port);
            Webcam.Init();
        }

        protected override void OnStop()
        {
        }
    }
}

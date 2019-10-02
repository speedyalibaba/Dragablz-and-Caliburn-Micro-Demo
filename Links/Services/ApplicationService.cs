using Links.Contract;
using Links.Services;
using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Links
{
    public class ApplicationService : IApplicationService
    {
        public static bool IgnoreShuttingDown { get; set; }
        public static bool IsStarting { get; set; }

        public void Restart()
        {
            var info = new ProcessStartInfo();
            info.Arguments = "/C choice /C Y /N /D Y /T 1 & START \"\" \"" + Assembly.GetExecutingAssembly().Location + "\"";
            info.WindowStyle = ProcessWindowStyle.Hidden;
            info.CreateNoWindow = true;
            info.FileName = "cmd.exe";
            Process.Start(info);
            Application.Current.Shutdown();
        }

        public void ShutDown()
        {
            if (IgnoreShuttingDown)
                return;
            IgnoreShuttingDown = true;

            var layoutManager = IoC.Get<LayoutManager>();
            layoutManager.Save();

            Application.Current.Shutdown();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;

namespace AutoInstallAPK
{
    class RunCommand
    {
        public static async Task Run(string cmd,string para)
        {
            ApplicationData.Current.LocalSettings.Values["command"] = cmd;
            ApplicationData.Current.LocalSettings.Values["parameters"] = para;
            await FullTrustProcessLauncher.LaunchFullTrustProcessForCurrentAppAsync("RunCommand");
        }
    }
}

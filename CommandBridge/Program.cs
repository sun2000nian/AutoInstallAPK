using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.AppService;
using Windows.Storage;
using Windows.Foundation.Collections;
using System.Threading;
using Microsoft.Win32;

namespace CommandBridge
{
    class Program
    {
        static AppServiceConnection connection = null;
        static AutoResetEvent appServiceExit = null;
        static void Main(string[] args)
        {
            MainAsync(args);
        }

        static async void MainAsync(string[] args)
        {
            Console.WriteLine("Hello");
            Process newProcess = new Process();
            string application = ApplicationData.Current.LocalSettings.Values["command"] as string;
            string parameters = ApplicationData.Current.LocalSettings.Values["parameters"] as string;
            newProcess.StartInfo.FileName = application;
            newProcess.StartInfo.Arguments = parameters;
            newProcess.StartInfo.UseShellExecute = false;
            newProcess.StartInfo.CreateNoWindow = false;
            newProcess.Start();
            newProcess.WaitForExit();

            ApplicationData.Current.LocalSettings.Values["finished"] = "true";
            //appServiceExit = new AutoResetEvent(false);

            //InitializeAppServiceConnection();
            //appServiceExit.WaitOne(1);
            //ValueSet msg = new ValueSet();
            //connection.SendMessageAsync(msg);
        }

        static async void InitializeAppServiceConnection()
        {
            connection = new AppServiceConnection();
            connection.AppServiceName = "SampleInteropService";
            connection.PackageFamilyName = Package.Current.Id.FamilyName;

            connection.OpenAsync();

        }
    }
}

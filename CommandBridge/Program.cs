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
using System.IO;

namespace CommandBridge
{
    class Program
    {
        static AppServiceConnection connection = null;
        static AutoResetEvent appServiceExit = null;
        static StorageFolder storageFolder = null;
        static StorageFile commandFile = null;
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
            newProcess.StartInfo.CreateNoWindow = true;
            newProcess.StartInfo.RedirectStandardOutput = true;
            newProcess.Start();
            string temp = newProcess.StandardOutput.ReadToEnd();
            string folder = ApplicationData.Current.LocalCacheFolder.Path;
            
            newProcess.WaitForExit();
            newProcess.Close();
            
            string cmdOutFile = folder + "/commandOutput";
            FileStream fileStream = new FileStream(cmdOutFile,FileMode.OpenOrCreate, FileAccess.Write);
            StreamWriter streamWriter = new StreamWriter(fileStream);
            fileStream.SetLength(0);
            streamWriter.Write(temp);
            streamWriter.Close();
            fileStream.Close();

            ApplicationData.Current.LocalSettings.Values["finished"] = "true";
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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;

namespace AutoInstallAPK
{
    class RunCommand
    {
        //static bool flag = false;
        //static DispatcherTimer timer = null;
        static StorageFile commandFile = null;
        private static Task<string> WaitFinish()
        {
            var task = Task.Run(() =>
            {
                string result;
                while (true)
                {
                    result = ApplicationData.Current.LocalSettings.Values["finished"] as string;
                    if (result == "true")
                    {
                        return "HelloWorld";
                    }
                    Thread.Sleep(30);
                }
            });
            return task;
        }

        //static void dispatcherTimer_Tick(object sender, object e)
        //{
        //    string result = ApplicationData.Current.LocalSettings.Values["finished"] as string;
        //    if (result == "true")
        //    {
        //        flag = true;
        //        timer.Stop();
        //    }
        //}

        public static async Task<string> AdbRun(string para)
        {
            if(ApplicationData.Current.LocalSettings.Values["UseDefaultADB"]as string == "false")
            {
                return await Run(ApplicationData.Current.LocalSettings.Values["adbPath"] as string, para);
            }
            else
            {
                return await Run(Directory.GetCurrentDirectory() + "/platform-tools/adb.exe", para);
            }
        }

        public static async Task<string> Run(string cmd,string para)
        {
            //timer = new DispatcherTimer();
            //timer.Tick += dispatcherTimer_Tick;
            //timer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            //flag = false;
            ApplicationData.Current.LocalSettings.Values["finished"] = "false";
            ApplicationData.Current.LocalSettings.Values["command"] = cmd;
            ApplicationData.Current.LocalSettings.Values["parameters"] = para;
            await FullTrustProcessLauncher.LaunchFullTrustProcessForCurrentAppAsync("RunCommand");
            await WaitFinish();
            commandFile = (await ApplicationData.Current.LocalCacheFolder.TryGetItemAsync("commandOutput")) as StorageFile;
            if (commandFile == null)
            {
                await ApplicationData.Current.LocalCacheFolder.CreateFileAsync("commandOutput");
                return null;
            }
            else
            {
                var stream = await commandFile.OpenAsync(Windows.Storage.FileAccessMode.Read);
                ulong size = stream.Size;
                using (var inputStream = stream.GetInputStreamAt(0))
                {
                    using (var dataReader = new Windows.Storage.Streams.DataReader(inputStream))
                    {
                        uint numBytesLoaded = await dataReader.LoadAsync((uint)size);
                        string result = dataReader.ReadString(numBytesLoaded);
                        return result;
                    }
                }
                //string result = await FileIO.ReadTextAsync(commandFile);
            }
        }
    }
}

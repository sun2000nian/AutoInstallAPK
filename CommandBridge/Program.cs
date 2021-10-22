using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace CommandBridge
{
    class Program
    {
        static void Main(string[] args)
        {
            Process newProcess = null;
            string application = ApplicationData.Current.LocalSettings.Values["command"] as string;
            string parameters = ApplicationData.Current.LocalSettings.Values["parameters"] as string;
            newProcess=Process.Start(application, parameters);
        }
    }
}

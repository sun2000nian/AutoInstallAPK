using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.AppService;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Foundation.Metadata;
using Windows.Storage;
using Windows.System.Threading;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace AutoInstallAPK
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        DispatcherTimer timer = null;
        public MainPage()
        {
            this.InitializeComponent();
            timer = new DispatcherTimer();
            timer.Tick += dispatcherTimer_Tick;
            timer.Interval = new TimeSpan(0, 0, 0, 0, 100);
        }

        //protected async override void OnNavigatedTo(NavigationEventArgs e)
        //{
        //    base.OnNavigatedTo(e);

        //    if (ApiInformation.IsApiContractPresent("Windows.ApplicationModel.FullTrustAppContract", 1, 0))
        //    {
        //        App.AppServiceConnected += MainPage_AppServiceConnected;
        //        App.AppServiceDisconnected += MainPage_AppServiceDisconnected;
        //    }
        //}
        void dispatcherTimer_Tick(object sender, object e)
        {
            string result = ApplicationData.Current.LocalSettings.Values["finished"] as string;
            if (result == "true")
            {
                autoSuggestBox.Text = result;
                timer.Stop();
            }
        }

        private async void button_Click(object sender, RoutedEventArgs e)
        {
            autoSuggestBox.Text = "运行中";
            timer.Start();
            ApplicationData.Current.LocalSettings.Values["finished"] = "false";
            await RunCommand.Run("C:\\Users\\sun20\\Desktop\\test\\default_test.exe", "install D:\\Downloads\\weixin8015android2020_arm64.apk");
            //TimeSpan period = TimeSpan.FromMilliseconds(100);
            //var timer = ThreadPoolTimer.CreatePeriodicTimer((source) =>
            //{
            //    try
            //    {
            //        //do something
            //        string result = ApplicationData.Current.LocalSettings.Values["finished"] as string;
            //        if ( result == "true")
            //        {
            //            autoSuggestBox.Text = "运行完成";
            //        }
            //    }
            //    catch (Exception)
            //    {

            //    }
            //},period);
        }

        //private async void MainPage_AppServiceConnected(object sender, AppServiceTriggerDetails e)
        //{
        //    autoSuggestBox.Text = "运行完成";
        //}

        /// <summary>
        /// When the desktop process is disconnected, reconnect if needed
        /// </summary>
        //private async void MainPage_AppServiceDisconnected(object sender, EventArgs e)
        //{
        //    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
        //    {

        //    });
        //}

    }
}

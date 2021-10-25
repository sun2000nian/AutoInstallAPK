using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.AppService;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Foundation.Metadata;
using Windows.Storage;
using Windows.System.Threading;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
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
        //DispatcherTimer timer = null;
        ObservableCollection<FontFamily> devices = new ObservableCollection<FontFamily>();
        public MainPage()
        {
            this.InitializeComponent();
            ApplicationView.GetForCurrentView().TryResizeView(new Size(550, 325));
            if (ApplicationData.Current.LocalSettings.Values["UseDefaultADB"] as string == "false")
            {
                checkBox_useDefaultADB.IsChecked = false;
                if (autoSuggestBox_PATH != null)
                {
                    autoSuggestBox_PATH.IsEnabled = true;
                    if (ApplicationData.Current.LocalSettings.Values["adbPath"] != null)
                    {
                        autoSuggestBox_PATH.Text = ApplicationData.Current.LocalSettings.Values["adbPath"] as string;
                    }
                }
                if (buttonApplyADBPath != null)
                {
                    buttonApplyADBPath.IsEnabled = true;
                }
            }
            else
            {
                checkBox_useDefaultADB.IsChecked = true;
                if (autoSuggestBox_PATH != null)
                {
                    autoSuggestBox_PATH.IsEnabled = false;
                }
                if (buttonApplyADBPath != null)
                {
                    buttonApplyADBPath.IsEnabled = false;
                }
            }
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            await loadDeviceInfo();
        }

        private async Task loadDeviceInfo()
        {
            string result = await RunCommand.AdbRun("devices");
            var list = result.Split("\r\n");
            List<string> deviceStrList = new List<string>(list);
            int default_select = -1;
            while (deviceStrList.Contains(""))
            {
                deviceStrList.Remove("");
            }
            devices.Clear();
            if (deviceStrList.Count == 1)
            {
                devices.Add(new FontFamily("无已连接设备"));
                comboBox_devices.SelectedIndex = 0;
                return;
            }
            for (int i = 1; i <deviceStrList.Count ; i++)
            {
                devices.Add(new FontFamily(deviceStrList[i]));
                if (deviceStrList[i].Contains("127")&&default_select==-1)
                {
                    default_select = i-1;
                }
            }
            if (default_select != -1)
            {
                comboBox_devices.SelectedIndex = default_select;
            }
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
        //void dispatcherTimer_Tick(object sender, object e)
        //{
        //    string result = ApplicationData.Current.LocalSettings.Values["finished"] as string;
        //    if (result == "true")
        //    {
        //        autoSuggestBox.Text = result;
        //        timer.Stop();
        //    }
        //}


        private void checkBox_useDefaultADB_Checked(object sender, RoutedEventArgs e)
        {
            ApplicationData.Current.LocalSettings.Values["UseDefaultADB"] = "true";
            checkBox_useDefaultADB.IsChecked = true;
            
            if (autoSuggestBox_PATH != null)
            {
                autoSuggestBox_PATH.IsEnabled = false;
            }
            if (buttonApplyADBPath != null)
            {
                buttonApplyADBPath.IsEnabled = false;
            }
        }

        private void checkBox_useDefaultADB_Unchecked(object sender, RoutedEventArgs e)
        {
            checkBox_useDefaultADB.IsChecked = false;
            
            if (autoSuggestBox_PATH != null)
            {
                autoSuggestBox_PATH.IsEnabled = true;
            }
            if (buttonApplyADBPath != null)
            {
                buttonApplyADBPath.IsEnabled = true;
            }
        }

        private async void buttonApplyADBPath_Click(object sender, RoutedEventArgs e)
        {
            buttonApplyADBPath.IsEnabled = false;
            textBlock_message.Foreground = textBlock.Foreground;
            textBlock_message.Text = "adb路径验证中";
            ApplicationData.Current.LocalSettings.Values["UseDefaultADB"] = "true";
            //string result= await RunCommand.Run(Directory.GetCurrentDirectory() + "/platform-tools/adb.exe", "devices");
            string newPath = autoSuggestBox_PATH.Text;
            newPath=newPath.Replace("\"","");
            if (Path.GetFileName(newPath) != "adb.exe")
            {
                textBlock_message.Text = "路径需指向一个adb.exe文件";
                textBlock_message.Foreground = new SolidColorBrush(Colors.Red);
                buttonApplyADBPath.IsEnabled = true;
                return;
            }
            ApplicationData.Current.LocalSettings.Values["noFile"] = "false";
            string result = await RunCommand.Run(newPath, "version");

            if (ApplicationData.Current.LocalSettings.Values["noFile"] as string == "true")
            {
                textBlock_message.Text = "路径需指向一个adb.exe文件";
                textBlock_message.Foreground = new SolidColorBrush(Colors.Red);
                buttonApplyADBPath.IsEnabled = true;
                return;
            }

            if (result.ToLower().Contains("installed as"))
            {
                //可用
                ApplicationData.Current.LocalSettings.Values["adbPath"] = newPath;
                
                textBlock_message.Text = "adb路径验证通过";
                textBlock_message.Foreground = new SolidColorBrush(Colors.Green);
                ApplicationData.Current.LocalSettings.Values["UseDefaultADB"] = "false";
            }
            else
            {
                textBlock_message.Text = "adb路径不可用";
                textBlock_message.Foreground = new SolidColorBrush(Colors.Red);
                ApplicationData.Current.LocalSettings.Values["UseDefaultADB"] = "true";
            }
            buttonApplyADBPath.IsEnabled = true;
        }

        public static bool ValidateIPv4(string ipString)
        {
            if (String.IsNullOrWhiteSpace(ipString))
            {
                return false;
            }
            string address = null;
            string port = null;
            string[] splt = ipString.Split(':');

            switch (splt.Length)
            {
                case 1:
                    address = ipString;
                    break;
                case 2:
                    address = ipString.Split(':')[0];
                    port = ipString.Split(':')[1];
                    if (String.IsNullOrWhiteSpace(address) || String.IsNullOrWhiteSpace(port))
                    {
                        return false;
                    }
                    break;
                default:
                    return false;
            }

            string[] splitValues = address.Split('.');
            if (splitValues.Length != 4)
            {
                return false;
            }

            byte tempForParsing;

            return splitValues.All(r => byte.TryParse(r, out tempForParsing));
        }

        private async void button_Connect_Click(object sender, RoutedEventArgs e)
        {
            if(!ValidateIPv4(autoSuggestBox_IP.Text))
            {
                textBlock_message.Text = "请输入一个有效的设备ip地址";
                textBlock_message.Foreground = new SolidColorBrush(Colors.Red);
                return;
            }
            button_connect.IsEnabled = false;
            textBlock_message.Text = "连接中...";
            textBlock_message.Foreground = textBlock.Foreground;
            string result = await RunCommand.AdbRun("connect " + autoSuggestBox_IP.Text);
            textBlock_message.Foreground = textBlock.Foreground;
            textBlock_message.Text = result;
            button_connect.IsEnabled = true;
            await loadDeviceInfo();
        }

        private async void autoSuggestBox_PATH_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.Add(".exe");

            Windows.Storage.StorageFile file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                // Application now has read/write access to the picked file
                autoSuggestBox_PATH.Text = file.Path;
            }
        }

        private async void buttonRefresh_Click(object sender, RoutedEventArgs e)
        {
            await loadDeviceInfo();
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ApplicationView.GetForCurrentView().TryResizeView(new Size(550, 325));
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

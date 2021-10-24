using Iteedee.ApkReader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using System.Net.Mime;
using HeyRed.Mime;
using System.Collections.ObjectModel;
using Windows.UI;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace AutoInstallAPK
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class InstallPage : Page
    {
        ApkInfo info;
        string path;
        List<string> deviceStrList = null;
        ObservableCollection<FontFamily> devices = new ObservableCollection<FontFamily>();
        bool device_connected = false;
        //DispatcherTimer timer = null;
        //StorageFolder storageFolder = null;
        //StorageFile commandFile = null;
        int term = 0;
        private async Task<ApkInfo> getApkInfo(StorageFile file)
        {
            byte[] manifestData = null;
            byte[] resourcesData = null;
            if (file != null)
                using (var stream1 = await file.OpenAsync(FileAccessMode.Read))
                {
                    using (var stream2 = await file.OpenAsync(FileAccessMode.Read))
                    {
                        using (ICSharpCode.SharpZipLib.Zip.ZipInputStream zip = new ICSharpCode.SharpZipLib.Zip.ZipInputStream(stream1.AsStreamForRead()))
                        {
                            ICSharpCode.SharpZipLib.Zip.ZipFile zipfile = new ICSharpCode.SharpZipLib.Zip.ZipFile(stream2.AsStreamForRead());
                            ICSharpCode.SharpZipLib.Zip.ZipEntry item;
                            while (true)
                            {
                                item = zip.GetNextEntry();
                                if (item == null)
                                    return null;
                                else if (item.Name.ToLower() == "androidmanifest.xml")
                                {
                                    /*
                                    //byte[] bytes = new byte[item.Size];

                                    //Stream strm = zipfile.GetInputStream(item);
                                    //int size = strm.Read(bytes, 0, bytes.Length);

                                    //using (BinaryReader s = new BinaryReader(strm))
                                    //{
                                    //    byte[] bytes2 = new byte[size];
                                    //    Array.Copy(bytes, bytes2, size);
                                    //    AndroidDecompress decompress = new AndroidDecompress();
                                    //    content = decompress.decompressXML(bytes);
                                    //    return content;
                                    //}
                                    //*/
                                    manifestData = new byte[item.Size+5];
                                    Stream strm = zipfile.GetInputStream(item);
                                    int size = strm.Read(manifestData, 0, manifestData.Length);
                                    if (resourcesData != null) break;
                                }
                                else if (item.Name.ToLower() == "resources.arsc")
                                {
                                    using (Stream strm = zipfile.GetInputStream(item))
                                    {
                                        using (BinaryReader s = new BinaryReader(strm))
                                        {
                                            resourcesData = s.ReadBytes((int)item.Size);
                                            if (manifestData != null)
                                                break;
                                        }
                                    }
                                }
                            }
                            if (manifestData != null && resourcesData != null)
                            {
                                ApkReader apkReader = new ApkReader();
                                ApkInfo info = apkReader.extractInfo(manifestData, resourcesData);
                                return info;
                            }
                            else return null;
                        }
                    }
                }
            else return null;
        }

        private async Task<SoftwareBitmap> getIcon(StorageFile file,string iconName)
        {
            SoftwareBitmap bitmap;
            if (file != null)
                using (var stream1 = await file.OpenAsync(FileAccessMode.Read))
                {
                    using (var stream2 = await file.OpenAsync(FileAccessMode.Read))
                    {
                        using (ICSharpCode.SharpZipLib.Zip.ZipInputStream zip = new ICSharpCode.SharpZipLib.Zip.ZipInputStream(stream1.AsStreamForRead()))
                        {
                            ICSharpCode.SharpZipLib.Zip.ZipFile zipfile = new ICSharpCode.SharpZipLib.Zip.ZipFile(stream2.AsStreamForRead());
                            ICSharpCode.SharpZipLib.Zip.ZipEntry item;
                            while (true)
                            {
                                item = zip.GetNextEntry();
                                if (item == null)
                                    return null;
                                else if (item.Name.ToLower() == iconName.ToLower())
                                {
                                    Stream stream = zipfile.GetInputStream(item);
                                    BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream.AsRandomAccessStream());
                                    bitmap = await decoder.GetSoftwareBitmapAsync();
                                    return bitmap;
                                }

                            }
                        }
                    }
                }
            else return null;
        }

        private async Task loadDeviceInfo()
        {
            string result = await RunCommand.AdbRun("devices");
            var list = result.Split("\r\n");
            deviceStrList = new List<string>(list);
            int default_select = -1;
            while (deviceStrList.Contains(""))
            {
                deviceStrList.Remove("");
            }
            devices.Clear();
            if (deviceStrList.Count == 1)
            {
                devices.Add(new FontFamily("无已连接设备"));
                device_connected = false;
                comboBox_SelectDevice.SelectedIndex = 0;
                return;
            }
            device_connected = true;
            for (int i = 1; i < deviceStrList.Count; i++)
            {
                devices.Add(new FontFamily(deviceStrList[i]));
                if (deviceStrList[i].Contains("127") && default_select == -1)
                {
                    default_select = i - 1;
                }
            }
            if (default_select != -1)
            {
                comboBox_SelectDevice.SelectedIndex = default_select;
            }
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            var file = e.Parameter as StorageFile;
            try
            {
                info = await getApkInfo(file);
                var imglist = info.iconFileName;
                int i = imglist.Count() - 1;
                string teststr;
                while (i >= 0)
                {
                    teststr = MimeTypesMap.GetMimeType(imglist[i]);
                    if (teststr.Contains("image"))
                        break;
                    i--;
                }
                if (info != null)
                {
                    SoftwareBitmap bitmap = await getIcon(file, info.iconFileName[i]);
                    if (bitmap != null)
                    {
                        if (bitmap.BitmapPixelFormat != BitmapPixelFormat.Bgra8 || bitmap.BitmapAlphaMode == BitmapAlphaMode.Straight)
                        {
                            bitmap = SoftwareBitmap.Convert(bitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
                        }

                        var source = new SoftwareBitmapSource();
                        await source.SetBitmapAsync(bitmap);
                        IconArea.Source = source;
                    }
                }
                
                textBlockPackageName.Text = info.label;
                buttonInstall.Content = "安装";
                textBlock_package.Text = "包: "+info.packageName;
                textBlock_version.Text = "版本: "+info.versionName;
                buttonInstall.IsEnabled = true;
            }
            catch(Exception)
            {
                textBlockPackageName.Text = "包信息解析失败，但仍可安装";
                
                buttonInstall.Content = "安装";
                buttonInstall.IsEnabled = true;
            }
            await loadDeviceInfo();
        }

        public InstallPage()
        {
            this.InitializeComponent();
            path = ApplicationData.Current.LocalSettings.Values["openningPath"] as string;
            textBlockPath.Text = "路径: "+ path;
            //timer = new DispatcherTimer();
            //timer.Tick += dispatcherTimer_Tick;
            //timer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            textBlockPackageName.Text = "加载中...";
            buttonInstall.Content = "安装";
            textBlock_package.Text = "";
            textBlock_version.Text = "";
            textBlock_out.Text = "";
            buttonInstall.IsEnabled = false;
            term = 1;
        }

        //void dispatcherTimer_Tick(object sender, object e)
        //{
        //    string result = ApplicationData.Current.LocalSettings.Values["finished"] as string;
        //    if (result == "true")
        //    {
        //        buttonInstall.Content = "关闭";
        //        buttonInstall.IsEnabled = true;
        //        timer.Stop();
        //        term = 3;
        //    }
        //}

        private async void ButtonInstall_Click(object sender, RoutedEventArgs e)
        {
            if (!device_connected)
            {
                textBlock_out.Text = "请连接一个设备";
                textBlock_out.Foreground = new SolidColorBrush(Colors.Red);
                return;
            }
            string selectedStr = null;
            if (devices != null)
            {
                selectedStr = deviceStrList[comboBox_SelectDevice.SelectedIndex+1];
            }
            selectedStr = selectedStr.Split('\t')[0];
            
            if (path != null && selectedStr!=null)
            {
                buttonInstall.Content = "安装中";
                buttonInstall.IsEnabled = false;
                string result = await RunCommand.AdbRun("-s " + selectedStr + " install " + path);
                //string result = await RunCommand.Run("C:\\Users\\sun20\\Desktop\\test\\default_test.exe", "install " + selectedStr);
                buttonCancel.Content = "完成";
                buttonInstall.IsEnabled = true;
                buttonInstall.Visibility = Visibility.Collapsed;
                textBlock_out.Text = result;
                textBlock_out.Foreground = textBlock.Foreground;

            }
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            CoreApplication.Exit();
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
            if (!ValidateIPv4(autoSuggestBox.Text))
            {
                textBlock_out.Text = "请输入一个有效的设备ip地址";
                textBlock_out.Foreground = new SolidColorBrush(Colors.Red);
                return;
            }
            button_Connect.IsEnabled = false;
            textBlock_out.Text = "连接中...";
            textBlock_out.Foreground = textBlock.Foreground;
            string result = await RunCommand.AdbRun("connect " + autoSuggestBox.Text);
            textBlock_out.Text = result;
            textBlock_out.Foreground = textBlock.Foreground;
            button_Connect.IsEnabled = true;
            await loadDeviceInfo();
        }
    }
}

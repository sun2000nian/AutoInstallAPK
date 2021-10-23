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
        DispatcherTimer timer = null;
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
                                    manifestData = new byte[item.Size];
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

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            var file = e.Parameter as StorageFile;
            try
            {
                info = await getApkInfo(file);
                var imglist = info.iconFileName;
                if (info != null)
                {
                    SoftwareBitmap bitmap = await getIcon(file, info.iconFileName[0]);
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
                buttonInstall.Content = "安装";
                buttonInstall.IsEnabled = true;
            }
            catch(Exception)
            {

            }
        }

        public InstallPage()
        {
            this.InitializeComponent();
            path = ApplicationData.Current.LocalSettings.Values["openningPath"] as string;
            textBlockPath.Text = path;
            timer = new DispatcherTimer();
            timer.Tick += dispatcherTimer_Tick;
            timer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            buttonInstall.Content = "加载中";
            buttonInstall.IsEnabled = false;
            term = 1;
        }

        void dispatcherTimer_Tick(object sender, object e)
        {
            string result = ApplicationData.Current.LocalSettings.Values["finished"] as string;
            if (result == "true")
            {
                buttonInstall.Content = "关闭";
                buttonInstall.IsEnabled = true;
                timer.Stop();
                term = 3;
            }
        }

        private async void ButtonInstall_Click(object sender, RoutedEventArgs e)
        {
            if (term==1)
            {
                if (path != null)
                {
                    ApplicationData.Current.LocalSettings.Values["finished"] = "false";
                    await RunCommand.Run("C:\\Users\\sun20\\Desktop\\test\\default_test.exe", "install " + path);
                    buttonInstall.Content = "安装中";
                    buttonInstall.IsEnabled = false;
                    timer.Start();
                    term = 2;
                }
            }
            else if (term == 3)
            {
                //关闭应用
                CoreApplication.Exit();
            }
        }
    }
}

﻿using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http;
using Windows.Web.Http.Filters;
using ZoDream.Helper;
using ZoDream.Layout;
using ZoDream.Model;
using ZoDream.Services;
using ZoDreamToolkit.Common;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace ZoDream.View
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class WebPage : Page, INotifyPropertyChanged
    {
        public WebPage()
        {
            this.InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Required;
        }

        public bool IsMore
        {
            get { return MoreGrid.Visibility == Visibility.Visible; }
            set {
                if (value && MoreGrid.Visibility == Visibility.Collapsed)
                {
                    MoreGrid.Visibility = Visibility.Visible;
                }
                else if (!value && MoreGrid.Visibility == Visibility.Visible)
                {
                    MoreGrid.Visibility = Visibility.Collapsed;
                }
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is RoutedEventArgs)
            {
                return;
            }
            Navigate((string)e.Parameter);
        }


        public event PropertyChangedEventHandler PropertyChanged;

        public void SetPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(name));
            }
        }

        public void SetPropertyChanged(object name)
        {
            SetPropertyChanged(nameof(name));
        }

        private void Search_GotFocus(object sender, RoutedEventArgs e)
        {
            IsMore = false;
            MoreBtn.Visibility = BackBtn.Visibility = ForwardBtn.Visibility = Visibility.Collapsed;
        }

        private void Search_LostFocus(object sender, RoutedEventArgs e)
        {
            MoreBtn.Visibility = Visibility.Visible;
            if (WebBrowser.CanGoBack)
            {
                BackBtn.Visibility = Visibility.Visible;
            }
            if (WebBrowser.CanGoForward)
            {
                ForwardBtn.Visibility = Visibility.Visible;
            }
        }

        private void ForwardBtn_Click(object sender, RoutedEventArgs e)
        {
            if (WebBrowser.CanGoForward)
            {
                WebBrowser.GoForward();
            }
            else
            {
                WebBrowser.Refresh();
            }
        }

        private void BackBtn_Click(object sender, RoutedEventArgs e)
        {
            if (WebBrowser.CanGoBack)
            {
                WebBrowser.GoBack();
            }
        }

        public void Navigate(string arg)
        {
            if (string.IsNullOrWhiteSpace(arg))
            {
                return;
            }
            try
            {
                var uri = new Uri(GetUrl(arg));
                WebBrowser.Navigate(uri);
            }
            catch (Exception)
            {
                Search.Source = string.Empty;
                Search.Title = "网址错误！";
            }


        }

        public string GetUrl(string arg)
        {
            if (string.IsNullOrWhiteSpace(arg) || !Regex.IsMatch(arg, @"\w+\.\w+"))
            {
                return "https://m.baidu.com/s?wd=" + arg;
            }
            switch (arg.IndexOf("//"))
            {
                case -1:
                    return "http://" + arg;
                case 0:
                    return "http:" + arg;
                case 1:
                    return "http" + arg;
                default:
                    return arg;
            }
        }


        private void WebBrowser_NewWindowRequested(WebView sender, WebViewNewWindowRequestedEventArgs args)
        {
            WebBrowser.Navigate(args.Uri);
            args.Handled = true;
        }

        private void WebBrowser_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            LoadingRing.IsActive = false;
            LoadingRing.Visibility = StopBtn.Visibility = Visibility.Collapsed;
            if (WebBrowser.CanGoBack)
            {
                BackBtn.Visibility = Visibility.Visible;
            }
            if (WebBrowser.CanGoForward)
            {
                ForwardBtn.Visibility = Visibility.Visible;
            }
            Search.Title = WebBrowser.DocumentTitle;
        }

        public async Task<string> GetHtml()
        {
            return await Js("document.getElementsByTagName('html')[0].innerHTML");
        }

        public async Task<string> GetBody()
        {
            return await Js("document.getElementsByTagName('body')[0].innerHTML");
        }

        public async Task<string> Js(string arg)
        {
            return await Js(new string[] { arg });
        }

        public async Task<string> Js(string[] args)
        {
            return await WebBrowser.InvokeScriptAsync("eval", args);
        }


        private void WebBrowser_NavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
        {
            if (Search != null)
            {
                Search.Title = Search.Source = args.Uri.AbsoluteUri;
                LoadingRing.Visibility = StopBtn.Visibility = Visibility.Visible;
                LoadingRing.IsActive = true;
            }
            // webView.AddWebAllowedObject("nativeObject", new MyNativeClass()); 
        }

        private void Search_OnEnter(object sender, EnterEventArgs e)
        {
            Navigate(e.Source);
            WebBrowser.Focus(FocusState.Pointer);
        }

        private void StopBtn_Click(object sender, RoutedEventArgs e)
        {
            WebBrowser.Stop();
        }

        private void RefreshBtn_Click(object sender, RoutedEventArgs e)
        {
            if (WebBrowser.Source == null)
            {
                return;
            }
            WebBrowser.Refresh();
        }

        private void MoreBtn_Click(object sender, RoutedEventArgs e)
        {
            IsMore = !IsMore;
        }

        private void ReadBtn_Click(object sender, RoutedEventArgs e)
        {
            if (WebBrowser.Source == null)
            {
                return;
            }
            Frame root = Window.Current.Content as Frame;
            //这里参数自动装箱
            root.Navigate(typeof(ReadPage), WebBrowser.Source);
        }

        private void FavoriteBtn_Click(object sender, RoutedEventArgs e)
        {
            Frame root = Window.Current.Content as Frame;
            //这里参数自动装箱
            root.Navigate(typeof(HistoryPage));
        }

        private void DownLoadBtn_Click(object sender, RoutedEventArgs e)
        {
            if (WebBrowser.Source == null)
            {
                return;
            }
            // 保存单页
            _savePageAsync();
        }

        private async Task _savePageAsync()
        {
            var book = await BookHelper.OpenAsync(WebBrowser.Source);
            if (book != null)
            {
                Toast.ShowInfo($"下载成功！共下载了 {book.Count} 章");
                return;
            }
            book = new Book()
            {
                Name = WebBrowser.DocumentTitle,
                IsLocal = false,
                Count = 1,
                Url = WebBrowser.Source.AbsoluteUri
            };
            var chapter = new BookChapter()
            {
                Name = book.Name,
                Url = WebBrowser.Source.ToString(),
            };
            chapter.Content = BookHelper.HtmlToText(await GetBody());
            SqlHelper.Conn.Open();
            book.Save();
            chapter.BookId = book.Id;
            chapter.Save();
            SqlHelper.Conn.Close();
            Toast.ShowInfo("页面保存成功！");
        }

        private void ScanBtn_Click(object sender, RoutedEventArgs e)
        {
            Frame root = Window.Current.Content as Frame;
            //这里参数自动装箱
            root.Navigate(typeof(QrPage));
        }

        private void AddRuleBtn_Click(object sender, RoutedEventArgs e)
        {
            Frame root = Window.Current.Content as Frame;
            //这里参数自动装箱
            root.Navigate(typeof(BookRulePage));
        }

        private void WebBrowser_UnviewableContentIdentified(WebView sender, WebViewUnviewableContentIdentifiedEventArgs args)
        {
            Frame root = Window.Current.Content as Frame;
            //这里参数自动装箱
            root.Navigate(typeof(HistoryPage));
            Messenger.Default.Send(new NotificationMessageAction<Uri>(args.Uri, null, item =>
            {

            }), "down");
            //_downLoadFileAsync(args.Uri);
        }

        private static async Task _downLoadFileAsync(Uri uri)
        {
            var fileName = Regex.Match(uri.LocalPath, @"[^/\?]+(\.[^\?]+)?", RegexOptions.RightToLeft).Value;
            var folder = await StorageHelper.OpenFolderAsync();
            if (folder == null)
            {
                return;
            }
            
            var http = new HttpClient();
            
            // 从响应Content-Disposition 获取正确的文件名
            using (var response = await http.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead))
            {
                if (response.StatusCode != Windows.Web.Http.HttpStatusCode.Ok)
                {
                    Toast.ShowInfo("网址有误...");
                }

                var httpFileName = response.Content.Headers.ContentDisposition.FileName;
                if (!string.IsNullOrWhiteSpace(httpFileName))
                {
                    fileName = httpFileName;
                }else if (string.IsNullOrWhiteSpace(fileName))
                {
                    fileName = "新下载文件.txt";
                }
                var file = await folder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
                var contentLength = response.Content.Headers.ContentLength;
                Toast.ShowInfo(fileName + " 正在下载...");

                using (var stream = await response.Content.ReadAsInputStreamAsync())
                using (var inputStream = stream.AsStreamForRead())
                using (var fileStream = await file.OpenStreamForWriteAsync())
                {
                    await inputStream.CopyToAsync(fileStream);
                    Toast.ShowInfo(fileName + " 下载完成！");
                }
            }
            
        }

        private void CopyBtn_Click(object sender, RoutedEventArgs e)
        {
            if (WebBrowser.Source == null)
            {
                return;
            }
            DataPackage dp = new DataPackage();
            dp.SetText(WebBrowser.Source.AbsoluteUri);
            Clipboard.SetContent(dp);
            Toast.ShowInfo("已复制当前网址！");
        }

        private void ClearBtn_Click(object sender, RoutedEventArgs e)
        {
            // 清除缓存
        }
    }
}

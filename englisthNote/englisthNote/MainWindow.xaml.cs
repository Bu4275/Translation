using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net;
using System.Web;
using System.IO;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using System.Reflection;
using Microsoft.VisualBasic;
using System.Runtime.Serialization.Formatters.Binary;
namespace englisthNote
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string url;
        string curSearchStr;
        public MainWindow()
        {
            InitializeComponent();

            url = urldic[website.Google];
            webBrowser1.Navigate(url);
            // set cookie
            //InternetSetCookie(url, "JSESSIONID", ""); 
            // silent mode 關閉alert
            webBrowser1.Navigated += new NavigatedEventHandler(wbMain_Navigated);
            textBox1.Focus();
            readLog_enLB();
        }

        #region silent mode
        void wbMain_Navigated(object sender, NavigationEventArgs e)
        {
            SetSilent(webBrowser1, true); // make it silent
        }
        public static void SetSilent(WebBrowser browser, bool silent)
        {
            if (browser == null)
                throw new ArgumentNullException("browser");

            // get an IWebBrowser2 from the document
            IOleServiceProvider sp = browser.Document as IOleServiceProvider;
            if (sp != null)
            {
                Guid IID_IWebBrowserApp = new Guid("0002DF05-0000-0000-C000-000000000046");
                Guid IID_IWebBrowser2 = new Guid("D30C1661-CDAF-11d0-8A3E-00C04FC9E26E");

                object webBrowser;
                sp.QueryService(ref IID_IWebBrowserApp, ref IID_IWebBrowser2, out webBrowser);
                if (webBrowser != null)
                {
                    webBrowser.GetType().InvokeMember("Silent", BindingFlags.Instance | BindingFlags.Public | BindingFlags.PutDispProperty, null, webBrowser, new object[] { silent });
                }
            }
        }
        [ComImport, Guid("6D5140C1-7436-11CE-8034-00AA006009FA"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IOleServiceProvider
        {
            [PreserveSig]
            int QueryService([In] ref Guid guidService, [In] ref Guid riid, [MarshalAs(UnmanagedType.IDispatch)] out object ppvObject);
        }
        #endregion

        // 允許cookie(測試中)
        [DllImport("wininet.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool InternetSetCookie(string lpszUrlName, string lbszCookieName, string lpszCookieData);
        // 送出查詢 Click
        private void insertBtn_Click(object sender, RoutedEventArgs e)
        {
            enLB.Items.Add(textBox1.Text);
            // 目前搜尋的單字
            curSearchStr = textBox1.Text;
            webBrowser1.Navigate(url + curSearchStr);
            // 記錄listBox
            log_enLB();
            // 全選
            textBox1.SelectAll();
        }
        // listBox選擇改變
        private void enLB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (enLB.SelectedItem != null)
            {
                curSearchStr = enLB.SelectedItem.ToString();
                webBrowser1.Navigate(url + curSearchStr);
            }
        }
        // listBox刪除內容
        private void enLB_KeyDown(object sender, KeyEventArgs e)
        {
            if (enLB.SelectedIndex != -1 && e.Key == Key.Delete)
                enLB.Items.Remove(enLB.SelectedItem);
        }
        // textbox 按下Enter
        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                insertBtn_Click(null, null);
        }

        // 切換查詢網站
        private void radiobtnGoogle_Checked(object sender, RoutedEventArgs e)
        {
            if (radiobtnGoogle.IsChecked == true)
                url = urldic[website.Google];
            webBrowser1.Navigate(url + curSearchStr);
        }
        private void radiobtnYahoo_Checked(object sender, RoutedEventArgs e)
        {
            if (radiobtnYahoo.IsChecked == true)
                url = urldic[website.Yahoo];
            webBrowser1.Navigate(url + curSearchStr);
        }
        private void radiobtnCambridge_Checked(object sender, RoutedEventArgs e)
        {
            if (radiobtnCambridge.IsChecked == true)
                url = urldic[website.Cambridge];
            webBrowser1.Navigate(url + curSearchStr);
        }
        private void radiobtnDictionary_Checked(object sender, RoutedEventArgs e)
        {
            if (radiobtnDictionary.IsChecked == true)
                url = urldic[website.Dictionary];
            webBrowser1.Navigate(url + curSearchStr);
        }

        private void log_enLB()
        {
            using (FileStream fs = new FileStream("word.txt", FileMode.Create))
            {
                using (StreamWriter sr = new StreamWriter(fs))
                {
                    foreach (string word in enLB.Items)
                        sr.WriteLine(word);
                }
            }
        }

        private void readLog_enLB()
        {
            if (File.Exists("word.txt"))
            {
                using (StreamReader sr = new StreamReader("word.txt"))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        enLB.Items.Add(line);
                    }
                }
            }
        }
        #region 支援的網站列表
        enum website
        {
            Google,
            Dictionary,
            Yahoo,
            Cambridge,
        }
        Dictionary<website, string> urldic = new Dictionary<website, string>
        {
            {website.Google,"https://translate.google.com/?q=google&ie=UTF-8&hl=zh-TW&sa=N#en/zh-TW/"},
            {website.Dictionary,"http://dictionary.reference.com/browse/"},
            {website.Yahoo,"https://tw.dictionary.yahoo.com/dictionary?p="},
            {website.Cambridge,"http://dictionary.cambridge.org/dictionary/english-chinese-traditional/"},
            
        };
        #endregion



    }
}

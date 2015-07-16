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
using System.Windows.Forms;
namespace englisthNote
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string url;
        string curSearchStr;
        Timer timer1 = new Timer();
        public MainWindow()
        {
            InitializeComponent();
            //指定預設網站
            url = urldic[website.Google];
            webBrowser1.Navigate(url);

            // silent mode 關閉alert
            webBrowser1.Navigated += new NavigatedEventHandler(wbMain_Navigated);
            textBox1.Focus();
            load_words();
        }

        #region silent mode
        void wbMain_Navigated(object sender, NavigationEventArgs e)
        {
            SetSilent(webBrowser1, true); // make it silent
        }
        public static void SetSilent(System.Windows.Controls.WebBrowser browser, bool silent)
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

        // 送出查詢 Click
        private void insertBtn_Click(object sender, RoutedEventArgs e)
        {
            listBox_word.Items.Add(textBox1.Text);
            // 目前搜尋的單字
            curSearchStr = textBox1.Text;
            webBrowser1.Navigate(url + curSearchStr);

            // 記錄listBox
            log_words();
            // 全選
            textBox1.SelectAll();


            // 關閉dialog
            timer1.Tick += new EventHandler(timer1_Tick);
            timer1.Enabled = true;
            timer1.Interval = 100;
            count_hasNotWindows = 0;
        }
        // listBox選擇改變
        private void listBox_word_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listBox_word.SelectedItem != null)
            {
                curSearchStr = listBox_word.SelectedItem.ToString();
                webBrowser1.Navigate(url + curSearchStr);
            }
        }
        private void listBox_word_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (listBox_word.SelectedIndex != -1 && e.Key == Key.Delete)
            {
                int selectedIndex = listBox_word.SelectedIndex;
                listBox_word.Items.Remove(listBox_word.SelectedItem);
                // 可以連續刪除
                listBox_word.SelectedIndex = selectedIndex - 1;
                log_words();
            }
        }
        // textbox 按下Enter
        private void textBox1_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
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
        // 記錄目前的單字
        private void log_words()
        {
            using (FileStream fs = new FileStream("word.txt", FileMode.Create))
            {
                using (StreamWriter sr = new StreamWriter(fs))
                {
                    foreach (string word in listBox_word.Items)
                        sr.WriteLine(word);
                }
            }
        }
        // 讀取之前記錄的單字
        private void load_words()
        {
            if (File.Exists("word.txt"))
            {
                using (StreamReader sr = new StreamReader("word.txt"))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        listBox_word.Items.Add(line);
                    }
                }
            }
        }


        #region 關閉指定Title的Dialog
        long WM_CLOSE = Convert.ToInt32("10", 16);
        string WINDOW_TITLE = "Windows 安全性警告";

        [DllImport("user32.dll")]
        public static extern int FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll")]
        public static extern long PostMessageA(long hWnd, long wMsg, long wParam, long lParam);
        public void closeWindow(string winTitle)
        {
            long lngHWND = FindWindow(null, winTitle);
            if (lngHWND != 0) PostMessageA(lngHWND, WM_CLOSE, 0, 0);
        }
        int count_hasNotWindows = 0;
        private void timer1_Tick(object sender, EventArgs e)
        {
            closeWindow(WINDOW_TITLE);
            return;
            count_hasNotWindows++;
            if (count_hasNotWindows > 15) timer1.Enabled = false;
        }
        #endregion

        #region 支援的網站列表
        enum website
        {
            Google,
            Yahoo,
            Cambridge,
        }
        Dictionary<website, string> urldic = new Dictionary<website, string>
        {
            {website.Google,"https://translate.google.com/?q=google&ie=UTF-8&hl=zh-TW&sa=N#en/zh-TW/"},
            {website.Cambridge,"http://dictionary.cambridge.org/dictionary/english-chinese-traditional/"},
            {website.Yahoo,"https://tw.dictionary.yahoo.com/dictionary?p="},
        };
        #endregion

        private void Grid_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.F1)
                radiobtnGoogle.IsChecked = true;
            if (e.Key == Key.F2)
                radiobtnYahoo.IsChecked = true;
            if (e.Key == Key.F3)
                radiobtnCambridge.IsChecked = true;
            
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Navigation;
using System.IO;
using System.Runtime.InteropServices;
using System.Reflection;
using gma.System.Windows;
using System.Windows.Interop;
//using UserActivety;
namespace englisthNote
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string curTranslateUrl;                 // 當前使用的翻譯網站網址
        string curCopyStr = string.Empty;       // Ctrl+C 的內容
        static bool isFormTop = true;           // 應用程式是否為最上層
        UserActivityHook actHook;               // 全域鍵盤滑鼠監聽
        WMPLib.WindowsMediaPlayer Player;       // 聲音播放
        string saveword_filename = "word.txt";
        string listen_preword = string.Empty;      // 上一次聆聽的單字
        int listen_count = 0;                      // 同一單字聆聽的次數 (用來區別快與慢的發音)
        bool islistBox_word_selectchang = true;
        // 建構子
        public MainWindow()
        {

            InitializeComponent();

            // 載入查詢過的單字
            load_words();

            //指定預設網站
            curTranslateUrl = Websties_Url[Websites_Names.Google];
            webBrowser1.Navigate(curTranslateUrl);

            // silent mode 關閉alert
            webBrowser1.Navigated += new NavigatedEventHandler(wbMain_Navigated);

            //hook
            actHook = new UserActivityHook();
            //actHook.OnMouseActivity += new System.Windows.Forms.MouseEventHandler(MouseMoved);
            //actHook.KeyPress += new System.Windows.Forms.KeyPressEventHandler(MyKeyPress);
            actHook.KeyDown += new System.Windows.Forms.KeyEventHandler(MyKeyDown);
            actHook.KeyUp += new System.Windows.Forms.KeyEventHandler(MyKeyUp);
            actHook.Start();

            // Application Idel (閒置時) 判斷視窗是否在最上層
            //DispatcherTimer timer = new DispatcherTimer
            //  (
            //  TimeSpan.FromTicks(400),
            //  DispatcherPriority.ApplicationIdle,// Or DispatcherPriority.SystemIdle
            //  (s, e) => { isFormTopCheck(); },
            //  System.Windows.Application.Current.Dispatcher
            //  );
            //timer.Start();

            textBox1.Focus();
            curCopyStr = System.Windows.Forms.Clipboard.GetText();
        }

        #region Function
        // 念出單字
        private void listen_word(string word)
        {
            string url = getVoiceUrl(word);
            if (url == null) return;

            Player = new WMPLib.WindowsMediaPlayer();
            Player.URL = url;
            Player.controls.play();
        }
        // 取得聲音來源網址
        private string getVoiceUrl(string word)
        {
            // 同個單字念第二次時，回傳唸較慢的版本
            if (listen_preword == word)
                listen_count++;
            else
                listen_count = 0;

            // cdict
            if (radiobtn_CdictListen.IsChecked == true)
            {
                return "http://s.cdict.net/" + word[0] + "/" + word + ".mp3";
            }

            //google
            if (listen_count % 2 == 0)
            { // The faster version
                listen_preword = word;
                return "https://translate.google.com/translate_tts?ie=UTF-8&q=" + word + "&tl=en&total=1&idx=0&textlen=6&tk=107576&client=t&prev=input&sa=N";
            }
            else
            {
                //The slower version
                return "https://translate.google.com/translate_tts?ie=UTF-8&q=" + word + "&tl=en&total=1&idx=0&textlen=6&tk=107576&client=t&prev=input&sa=N&ttsspeed=0.24";
            }



        }
        // 翻譯textbox內的單字，並記錄起來。
        private void translate(string word)
        {
            // 目前搜尋的單字
            textBox1.Text = word;
            webBrowser1.Navigate(curTranslateUrl + word);
        }
        // log word in listbox into word.txt
        private void log_words()
        {
            using (FileStream fs = new FileStream(saveword_filename, FileMode.Create))
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
            if (File.Exists(saveword_filename))
            {
                using (StreamReader sr = new StreamReader(saveword_filename))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        listBox_word.Items.Add(line);
                    }
                }
            }
        }
        #endregion

        #region 物件事件
        // 送出查詢 Click ( MyKeyUp、textBox1_KeyDown 會觸發此Click)
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            listBox_word.Items.Add(textBox1.Text);
            translate(textBox1.Text);
            log_words();
            // 全選
            textBox1.SelectAll();
            // 發音
            if (CheckBox_listenWord.IsChecked == true) listen_word(textBox1.Text);
        }
        // 清除全部(listBox) Click
        private void btn_clearAll_Click(object sender, RoutedEventArgs e)
        {
            listBox_word.Items.Clear();
            using (FileStream fs = new FileStream(saveword_filename, FileMode.Create))
            {
                using (StreamWriter sr = new StreamWriter(fs)) { }
            }
        }
        // listBox 選擇改變
        private void listBox_word_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listBox_word.SelectedItem != null && islistBox_word_selectchang)
            {
                translate(listBox_word.SelectedItem.ToString());
            }
            else
                islistBox_word_selectchang = true;
        }
        // listBox 按 下Delete時(注意會觸發 listBox_word_SelectionChanged )
        private void listBox_word_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (listBox_word.SelectedIndex != -1 && e.Key == Key.Delete)
            {
                int selectedIndex = listBox_word.SelectedIndex;
                listBox_word.Items.Remove(listBox_word.SelectedItem);
                // 可以連續刪除
                listBox_word.SelectedIndex = selectedIndex - 1;
                log_words();
                islistBox_word_selectchang = false;
            }
        }
        // textbox 按下 Enter 時
        private void textBox1_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                btnSearch_Click(null, null);
        }
        // radiobutton 切換查詢網站
        private void radiobtnGoogle_Checked(object sender, RoutedEventArgs e)
        {
            if (radiobtn_Google.IsChecked == true)
                curTranslateUrl = Websties_Url[Websites_Names.Google];
            if (textBox1.Text != string.Empty)
                translate(textBox1.Text);
        }
        private void radiobtnYahoo_Checked(object sender, RoutedEventArgs e)
        {
            if (radiobtn_Yahoo.IsChecked == true)
                curTranslateUrl = Websties_Url[Websites_Names.Yahoo];
            if (textBox1.Text != string.Empty)
                translate(textBox1.Text);
        }
        private void radiobtnCambridge_Checked(object sender, RoutedEventArgs e)
        {
            if (radiobtn_Cambridge.IsChecked == true)
                curTranslateUrl = Websties_Url[Websites_Names.Cambridge];
            if (textBox1.Text != string.Empty)
                translate(textBox1.Text);
        }
        // local shourtcut
        private void Grid_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.F1)
                radiobtn_GoogleListen.IsChecked = true;
            else if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.F2)
                radiobtn_CdictListen.IsChecked = true;
            else
            {
                if (e.Key == Key.F1)
                    radiobtn_Google.IsChecked = true;
                if (e.Key == Key.F2)
                    radiobtn_Yahoo.IsChecked = true;
                if (e.Key == Key.F3)
                    radiobtn_Cambridge.IsChecked = true;
            }
        }
        // listBox_Drop
        private void listBox_word_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, false))
                e.Effects = DragDropEffects.Copy;
            else
                e.Effects = DragDropEffects.None;
        }
        private void listBox_word_Drop(object sender, DragEventArgs e)
        {
            string[] fileInfo = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            string filePath = string.Join("", fileInfo, 0, fileInfo.Length);

            if (File.Exists(filePath))
            {
                using (StreamReader sr = new StreamReader(filePath))
                {
                    string line = string.Empty;
                    while ((line = sr.ReadLine()) != null)
                        listBox_word.Items.Add(line);
                }
            }
            log_words();
        }

        #endregion

        #region Global shoutcut(目前只HOOK KeyDown、KeyUP)
        public void MyKeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            Console.WriteLine("keydown: " + e.KeyData.ToString());
            if (e.KeyData.ToString() == Key.F4.ToString())
            {
                listen_word(textBox1.Text);
            }
        }
        public void MyKeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
        {
            Console.WriteLine("keypress");
        }
        public void MyKeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            Console.WriteLine("keyup");
            // 複製的內容改變時觸發(limit string's length = 100)
            if (curCopyStr != System.Windows.Forms.Clipboard.GetText() && System.Windows.Forms.Clipboard.GetText().Length < 100)
            {
                Console.WriteLine(System.Windows.Forms.Clipboard.GetText());
                curCopyStr = System.Windows.Forms.Clipboard.GetText();
                textBox1.Text = curCopyStr;
                btnSearch_Click(null, null);
            }
        }
        public void MouseMoved(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            Console.WriteLine("X: " + e.X + ", Y: " + e.Y);
        }
        #endregion

        #region 支援的網站列表
        enum Websites_Names
        {
            Google,
            Yahoo,
            Cambridge,
        }
        Dictionary<Websites_Names, string> Websties_Url = new Dictionary<Websites_Names, string>
        {
            {Websites_Names.Google,"https://translate.google.com/?q=google&ie=UTF-8&hl=zh-TW&sa=N#auto/zh-TW/"},
            {Websites_Names.Cambridge,"http://dictionary.cambridge.org/dictionary/english-chinese-traditional/"},
            {Websites_Names.Yahoo,"https://tw.dictionary.yahoo.com/dictionary?p="},
        };
        #endregion

        #region silent mode (取消SSL警告)
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

        #region 判段視窗是否在最上層 目前未使用
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();
        private bool IsActive(IntPtr handle)
        {
            IntPtr activeHandle = GetForegroundWindow();
            return (activeHandle == handle);
        }
        public IntPtr GetHandle(Window w)
        {
            WindowInteropHelper h = new WindowInteropHelper(this);
            return h.Handle;
        }
        private void isFormTopCheck()
        {
            if (!IsActive(GetHandle(this)) && isFormTop)
            {
                isFormTop = false;
                Console.Write("FormIsNotTop");
            }
        }


        #endregion

        private void webBrowser1_Navigated(object sender, NavigationEventArgs e)
        {
            Console.WriteLine("webBrowser1_Navigated: url={0}", e.Uri);
            string curUrl = e.Uri.ToString();
            // 如果用GOOGLE翻譯時，使用者改變翻譯的語言，就將其設為預設
            if (curUrl.Contains("https://translate.google.com/?q=google&ie=UTF-8&hl=zh-TW&sa=N#"))
            {
                string[] urls = curUrl.Split('/');
                StringBuilder tmpSb = new StringBuilder();

                // 取得新的翻譯網址
                for (int i = 0; i < 5; i++)
                    tmpSb.Append(urls[i]).Append("/");

                // change the language of tranlate
                curTranslateUrl = tmpSb.ToString();
                tmpSb.Clear();

                // get the google's translate content;
                for (int i = 5; i < urls.Length; i++)
                    tmpSb.Append(urls[i]).Append("/");
                tmpSb.Remove(tmpSb.Length - 1, 1); // remove last char, that is "/"

                textBox1.Text = tmpSb.ToString();

            }
        }

        // Auto changing the size of webBrowser1
        private void Form1_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            int offset = 30;
            if (Form1.Width - webBrowser1.Margin.Left - offset > 0)
                webBrowser1.Width = Form1.Width - webBrowser1.Margin.Left - offset;
            else
                webBrowser1.Width = 0;

            if (Form1.Height - webBrowser1.Margin.Bottom - offset > 0)
                webBrowser1.Height = Form1.Height - webBrowser1.Margin.Top - offset - 20;
            else
                webBrowser1.Height = 0;
        }



    }
}

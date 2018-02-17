using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using System.IO;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using CefSharp;
using CefSharp.WinForms;


namespace Youtube_Watcher_with_Chrome
{
    public partial class Form1 : Form
    {
        public ChromiumWebBrowser chrome;

        // Setting up and initiating the chrome browser
        public void InitBrowser()
        {
            CefSettings settings = new CefSettings();
            // Where to save cache
            settings.CachePath = "cache";
            // Enable WebRTC
            settings.CefCommandLineArgs.Add("enable-media-stream", "1");
            // Don't use a proxy server, always make direct connections. Overrides any other proxy server flags that are passed.
            settings.CefCommandLineArgs.Add("no-proxy-server", "1");
            // Dumps extra logging about plugin loading to the log file
            settings.CefCommandLineArgs.Add("debug-plugin-loading", "1");
            // By default, an https page cannot run JavaScript, CSS or plugins from http URLs. This provides an override to get the old insecure behavior. Only available in 47 and above.
            settings.CefCommandLineArgs.Add("allow-running-insecure-content", "1");

            // Initializing Crhome Browser with the settings above
            Cef.Initialize(settings);
            chrome = new ChromiumWebBrowser("")
            {
                BrowserSettings = new BrowserSettings()
                {
                    Plugins = CefState.Enabled,
                    BackgroundColor = 255,
                    Javascript = CefState.Enabled,
                    WindowlessFrameRate = 60,
                },
                // Some classes have to be overridden in order for Drag&Drop to work
                DragHandler = new DragHandler(),
                AllowDrop = false,
                BackColor = Color.Black,
                Enabled = true,
                LifeSpanHandler = new LifeSpanHandler(this),
            };
            Controls.Add(chrome);
            chrome.Dock = DockStyle.None;
        }

        #region Moving Form
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();
        #endregion

        public Form1()
        {
            InitializeComponent();
            InitBrowser();

            // Adding mouse wheel event for volume control
            MouseWheel += new MouseEventHandler(Form1_MouseWheel);

            // Turning on double buffering (this is fixed flickering on some cases)
            DoubleBuffered = true;
        }

        // Initializing variables
        Timer hiderTimer = new Timer();
        Timer sensor = new Timer();
        PictureBox close = new PictureBox();
        PictureBox resizer = new PictureBox();
        PictureBox maximize = new PictureBox();
        PictureBox settings = new PictureBox();
        PictureBox minimize = new PictureBox();
        PictureBox volumeIndicator = new PictureBox();

        YWSettings Settings;

        Panel settingsPanel = new Panel() { Visible = false };
        public CheckBox checkbox_clearCacheOnExit = new CheckBox();
        public CheckBox checkbox_rememberMainformPosition = new CheckBox();
        public CheckBox checkbox_rememberMainformSize = new CheckBox();
        public CheckBox checkbox_alwaysOnTop = new CheckBox();
        public Label label_volumeIndicatorSteps = new Label();
        public HScrollBar scrollbar_volumeIndicatorSteps = new HScrollBar();

        bool mouseClicked = false;
        bool dragEnter = false;
        bool maximized = false;
        bool browserInitialized = false;
        public int volume = 50;
        public int volumeIndicatorSteps = 10;
        Point mousePos;
        Point defaultSettingsPropertyPosition = new Point(5, 25);
        Size maxSize, minSize;
        Size buttonSize;
        FormWindowState previousState = FormWindowState.Normal;


        private void Form1_Load(object sender, EventArgs e)
        {
            // Setting up basics and UI
            Settings = new YWSettings(this);
            TransparencyKey = Color.Fuchsia;
            BackColor = Color.Fuchsia;

            minSize = new Size(400, 300);
            maxSize = new Size(Screen.PrimaryScreen.WorkingArea.Width, Screen.PrimaryScreen.WorkingArea.Height);

            buttonSize = new Size(20, 18);

            chrome.Size = new Size(ClientSize.Width, ClientSize.Height - 50);
            chrome.Location = new Point(0, 25);
            chrome.IsBrowserInitializedChanged += Chrome_IsBrowserInitializedChanged;

            //chrome.RegisterAsyncJsObject("mainVolume", volume);

            sensor.Tick += Sensor_Tick;
            sensor.Interval = 1;
            sensor.Enabled = true;

            hiderTimer.Tick += Timer_Tick;
            hiderTimer.Interval = 2000;
            hiderTimer.Enabled = true;

            volumeIndicator.Size = new Size(ClientSize.Width - 100, 2);
            volumeIndicator.Location = new Point(20, 0);
            Controls.Add(volumeIndicator);
            volumeIndicator.BringToFront();

            close.Size = buttonSize;
            close.Location = new Point(ClientSize.Width - close.Size.Width - 20, 2);
            close.MouseClick += Close_MouseClick;
            Controls.Add(close);
            close.BringToFront();
            
            resizer.Size = buttonSize;
            resizer.Location = new Point(ClientSize.Width - resizer.Size.Width - 20, ClientSize.Height - resizer.Size.Height - 2);
            resizer.MouseDown += Resizer_MouseDown;
            resizer.MouseUp += Resizer_MouseUp;
            resizer.MouseMove += Resizer_MouseMove;
            Controls.Add(resizer);
            resizer.BringToFront();

            maximize.Size = buttonSize;
            maximize.Location = new Point(close.Location.X - buttonSize.Width - 7, 2);
            maximize.MouseDown += Maximize_MouseDown;
            Controls.Add(maximize);
            maximize.BringToFront();

            minimize.Size = buttonSize;
            minimize.Location = new Point(close.Location.X - buttonSize.Width - buttonSize.Width - 14, 2);
            minimize.MouseDown += Minimize_MouseDown;
            Controls.Add(minimize);
            minimize.BringToFront();

            settings.Size = buttonSize;
            settings.Location = new Point(20, 2);
            settings.MouseDown += Settings_MouseDown;
            Controls.Add(settings);
            settings.BringToFront();


            settingsPanel.Size = new Size(150, 160);
            settingsPanel.Location = new Point(settings.Location.X, settings.Location.Y+settings.Height);
            settingsPanel.BackColor = Color.Black;

            checkbox_clearCacheOnExit.Location = defaultSettingsPropertyPosition;
            checkbox_clearCacheOnExit.Text = "clear cache on exit";
            checkbox_clearCacheOnExit.BackColor = settingsPanel.BackColor;
            checkbox_clearCacheOnExit.ForeColor = Color.Gray;
            checkbox_clearCacheOnExit.AutoSize = true;
            checkbox_clearCacheOnExit.CheckedChanged += Checkbox_clearCacheOnExit_CheckedChanged;

            checkbox_rememberMainformPosition.Location = new Point(defaultSettingsPropertyPosition.X, defaultSettingsPropertyPosition.Y + checkbox_clearCacheOnExit.Height);
            checkbox_rememberMainformPosition.Text = "remember position";
            checkbox_rememberMainformPosition.BackColor = settingsPanel.BackColor;
            checkbox_rememberMainformPosition.ForeColor = Color.Gray;
            checkbox_rememberMainformPosition.AutoSize = true;
            checkbox_rememberMainformPosition.CheckedChanged += Checkbox_rememberMainformPosition_CheckedChanged;
            
            checkbox_rememberMainformSize.Location = new Point(defaultSettingsPropertyPosition.X, checkbox_rememberMainformPosition.Location.Y + checkbox_rememberMainformPosition.Height);
            checkbox_rememberMainformSize.Text = "remember size";
            checkbox_rememberMainformSize.BackColor = settingsPanel.BackColor;
            checkbox_rememberMainformSize.ForeColor = Color.Gray;
            checkbox_rememberMainformSize.AutoSize = true;
            checkbox_rememberMainformSize.CheckedChanged += Checkbox_rememberMainformSize_CheckedChanged;

            checkbox_alwaysOnTop.Location = new Point(defaultSettingsPropertyPosition.X, checkbox_rememberMainformSize.Location.Y + checkbox_rememberMainformSize.Height);
            checkbox_alwaysOnTop.Text = "always on top";
            checkbox_alwaysOnTop.BackColor = settingsPanel.BackColor;
            checkbox_alwaysOnTop.ForeColor = Color.Gray;
            checkbox_alwaysOnTop.AutoSize = true;
            checkbox_alwaysOnTop.CheckedChanged += Checkbox_alwaysOnTop_CheckedChanged;

            label_volumeIndicatorSteps.Location = new Point(defaultSettingsPropertyPosition.X, checkbox_alwaysOnTop.Location.Y + checkbox_alwaysOnTop.Height);
            label_volumeIndicatorSteps.Size = new Size(settingsPanel.Width, 15);
            label_volumeIndicatorSteps.BackColor = settingsPanel.BackColor;
            label_volumeIndicatorSteps.ForeColor = Color.Gray;
            label_volumeIndicatorSteps.TextAlign = ContentAlignment.MiddleCenter;
            label_volumeIndicatorSteps.Text = volumeIndicatorSteps.ToString();

            scrollbar_volumeIndicatorSteps.Location = new Point(defaultSettingsPropertyPosition.X, label_volumeIndicatorSteps.Location.Y + label_volumeIndicatorSteps.Height);
            scrollbar_volumeIndicatorSteps.Size = new Size(settingsPanel.Width - (defaultSettingsPropertyPosition.X * 2), 15);
            scrollbar_volumeIndicatorSteps.LargeChange = 5;
            scrollbar_volumeIndicatorSteps.Maximum = 104;
            scrollbar_volumeIndicatorSteps.Visible = true;
            scrollbar_volumeIndicatorSteps.Value = volumeIndicatorSteps;
            scrollbar_volumeIndicatorSteps.Scroll += Scrollbar_volumeIndicatorSteps_Scroll;

            settingsPanel.Controls.Add(checkbox_clearCacheOnExit);
            settingsPanel.Controls.Add(checkbox_rememberMainformPosition);
            settingsPanel.Controls.Add(checkbox_rememberMainformSize);
            settingsPanel.Controls.Add(checkbox_alwaysOnTop);
            settingsPanel.Controls.Add(label_volumeIndicatorSteps);
            settingsPanel.Controls.Add(scrollbar_volumeIndicatorSteps);
            Controls.Add(settingsPanel);
            settingsPanel.BringToFront();
            settingsPanel.Visible = false;


            drawButtons();

            relocator();

        }

        // Loading the default Youtube page with Youtube API
        private void Chrome_IsBrowserInitializedChanged(object sender, IsBrowserInitializedChangedEventArgs e)
        {
            if (e.IsBrowserInitialized && !browserInitialized)
            {
                chrome.GetMainFrame().LoadUrl("dummy:");
                string html =
                    "<!DOCTYPE html>" +
                    "<html>" +
                    "<head>" +
                    "<meta charset='utf-8' />" +
                    "<style>body{ background-color: black; margin: 0; padding: 0;} #player { position: absolute; top: 0; left: 0; width: 100%; height: 100%; } </style>" +
                    "</head>" +
                    "<body>" +

                    "<div id='player'></div>" +

                    "<script>" +
                    "var tag = document.createElement('script');" +

                    "tag.src = 'https://www.youtube.com/iframe_api';" +
                    "var firstScriptTag = document.getElementsByTagName('script')[0];" +
                    "firstScriptTag.parentNode.insertBefore(tag, firstScriptTag);" +

                    "function SetVolume(volume) {" +
                        "player.setVolume(volume);" +
                    "}" +

                    "function GetVolume() {" +
                        "window.mainVolume = player.getVolume();" +
                    "}" +

                    "var player;" +
                    "function onYouTubeIframeAPIReady() {" +
                        "player = new YT.Player('player', {" +
                                "height: '" + chrome.Height + "'," +
                                "width: '" + chrome.Width + "'," +
                                "videoId: 'JwwI9SagjfU'," +
                                "playerVars: { 'autoplay': 1, 'fs': 0, 'iv_load_policy': 3, 'modestbranding': 1, 'showinfo': 0 }," +
                                "events: {" +
                                    "'onReady': onPlayerReady," +
                                "}" +
                         "}); " +
                    "}" +

                    "function loadVideoById(videoID, start) {" +
                            "player.loadVideoById(videoID, start, \"default\");" +
                            "$('video').currentTime = $('video').duration" +
                    "}" +

                    "function loadPlaylist(playlistId, index, start) {" +
                        "player.loadPlaylist({" +
                            "list: playlistId," +
                            "index: index," +
                            "startSeconds: start," +
                            "suggestedQuality: \"default\"});" +
                            "$('video').currentTime = $('video').duration" +
                    "}" +

                    "function onPlayerReady(event) {" +
                        "event.target.playVideo();" +
                    "}" +

                    "</script>" +
                    "</body>" +
                    "</html>";

                chrome.GetMainFrame().LoadStringForUrl(html, "https://www.youtube.com/");
                browserInitialized = true;
            }
        }

        // This sensor checks if an object is being dragged to the main window
        private void Sensor_Tick(object sender, EventArgs e)
        {
            if (Cursor.Position.X >= Location.X && Cursor.Position.X < Location.X + Size.Width && Cursor.Position.Y >= (Location.Y+25) && Cursor.Position.Y < (Location.Y+25) + (Size.Height-50))
            {
                if (dragEnter) chrome.Enabled = false;
                    else chrome.Enabled = true;
            }
            else
            {
                chrome.Enabled = false;
            }
        }

        // This function applies a link
        public void linkApply(string text)
        {
            chrome.ExecuteScriptAsync("SetVolume(" + volume + ");");

            if (text != "")
            {
                Uri url;
                // Cheking the validity of the URL
                if (Uri.TryCreate(text, UriKind.Absolute, out url))
                {
                    // Check if it is a Youtube URL
                    if (url.Host.Contains("youtube") || url.Host.Contains("youtu.be"))
                    {
                        string videoId = "";
                        int start = 0;
                        string tempUrl = url.ToString();

                        // Loading the URL based on its content
                        if (tempUrl.Contains("/watch"))
                        {
                            int tempIndex = tempUrl.IndexOf("v=");
                            videoId = tempUrl.Substring(tempIndex + 2, 11);

                            if (tempUrl.Contains("list="))
                            {
                                int tempIndex2 = tempUrl.IndexOf("list=");
                                videoId = tempUrl.Substring(tempIndex2 + 5, 13);

                                int plli = tempUrl.IndexOf("index=");
                                int plIndex = 1;
                                if (plli > 0) plIndex = Convert.ToInt32(tempUrl.Substring(plli + 6,tempUrl.IndexOf("&", plli+6)-(plli+6)));
                                if (plIndex <= 0) plIndex = 1;
                                chrome.ExecuteScriptAsync("loadPlaylist(\"" + videoId + "\", " + plIndex + "," + start + ");");
                            }
                            else
                            chrome.ExecuteScriptAsync("loadVideoById(\"" + videoId + "\", " + start + ");");
                        }
                        else if (tempUrl.Contains("/embed/"))
                        {
                            int tempIndex = tempUrl.IndexOf("/embed/") + 7;
                            videoId = tempUrl.Substring(tempIndex, 11);

                            chrome.ExecuteScriptAsync("loadVideoById(\"" + videoId + "\", " + start + ");");
                        }
                        else if (tempUrl.Contains("/v/"))
                        {
                            int tempIndex = tempUrl.IndexOf("/v/") + 3;
                            videoId = tempUrl.Substring(tempIndex, 11);

                            chrome.ExecuteScriptAsync("loadVideoById(\"" + videoId + "\", " + start + ");");
                        }
                        else if (tempUrl.Contains("youtu.be/"))
                        {
                            int tempIndex = tempUrl.IndexOf("youtu.be/") + 9;
                            videoId = tempUrl.Substring(tempIndex, 11);

                            chrome.ExecuteScriptAsync("loadVideoById(\"" + videoId + "\", " + start + ");");
                        }
                        else if (tempUrl.Contains("list="))
                        {
                            int tempIndex = tempUrl.IndexOf("list=") + 5;
                            videoId = tempUrl.Substring(tempIndex, 34);

                            int plli = tempUrl.IndexOf("index=");
                            int plIndex = 1;
                            if (plli > 0) plIndex = Convert.ToInt32(tempUrl.Substring(plli + 6));
                            if (plIndex <= 0) plIndex = 1;
                            chrome.ExecuteScriptAsync("loadPlaylist(\"" + videoId + "\", "+plIndex+"," + start + ");");
                        }
                    }
                }
            }
        }

        private void Scrollbar_volumeIndicatorSteps_Scroll(object sender, ScrollEventArgs e)
        {
            Settings.VolumeIndicatorSteps = e.NewValue;
            label_volumeIndicatorSteps.Text = e.NewValue.ToString();
        }

        private void Checkbox_alwaysOnTop_CheckedChanged(object sender, EventArgs e)
        {
            Settings.AlwaysOnTop = checkbox_alwaysOnTop.Checked;
        }

        private void Checkbox_rememberMainformSize_CheckedChanged(object sender, EventArgs e)
        {
            Settings.RememberMainformSize = checkbox_rememberMainformSize.Checked;
            Settings.MainformSize = ClientSize;
        }

        private void Checkbox_rememberMainformPosition_CheckedChanged(object sender, EventArgs e)
        {
            Settings.RememberMainformPosition = checkbox_rememberMainformPosition.Checked;
            Settings.MainformPosition = Location;
        }

        private void Checkbox_clearCacheOnExit_CheckedChanged(object sender, EventArgs e)
        {
            Settings.ClearCacheOnExit = checkbox_clearCacheOnExit.Checked;
        }

        private void Settings_MouseDown(object sender, MouseEventArgs e)
        {
            settingsPanel.Visible = !settingsPanel.Visible;
        }

        private void Minimize_MouseDown(object sender, MouseEventArgs e)
        {
            if (WindowState == FormWindowState.Normal || WindowState == FormWindowState.Maximized)
            {
                previousState = WindowState;
                WindowState = FormWindowState.Minimized;
            }
            else
            {
                WindowState = previousState;
            }
        }

        private void Maximize_MouseDown(object sender, MouseEventArgs e)
        {
            if (maximized)
            {
                WindowState = FormWindowState.Normal;
                Settings.MainformMaximized = false;
                if (Settings.RememberMainformSize) Settings.MainformSize = ClientSize;
                if (Settings.RememberMainformPosition) Settings.MainformPosition = Location;
                chrome.Size = new Size(ClientSize.Width, ClientSize.Height - 50);
                relocator();
                maximized = false;
            }
            else
            {
                WindowState = FormWindowState.Maximized;
                Settings.MainformMaximized = true;
                chrome.Size = new Size(ClientSize.Width, ClientSize.Height - 50);
                relocator();
                maximized = true;
            }
        }

        private void Resizer_MouseMove(object sender, MouseEventArgs e)
        {
            if (mouseClicked && WindowState == FormWindowState.Normal)
            {
                Size newSize = new Size((resizer.Location.X + mousePos.X) - ((resizer.Location.X + mousePos.X) - (resizer.Location.X + e.X)), (resizer.Location.Y + mousePos.Y) - ((resizer.Location.Y + mousePos.Y) - (resizer.Location.Y + e.Y)));
                if (newSize.Width < maxSize.Width && newSize.Width > minSize.Width && newSize.Height < maxSize.Height && newSize.Height > minSize.Height)
                {
                    ClientSize = newSize;
                    chrome.Size = new Size(newSize.Width, newSize.Height - 50);
                    mousePos = e.Location;

                    relocator();
                }
            }
        }

        private void Resizer_MouseUp(object sender, MouseEventArgs e)
        {
            mouseClicked = false;
            if (Settings.RememberMainformSize && WindowState != FormWindowState.Maximized) Settings.MainformSize = ClientSize;
        }

        private void Resizer_MouseDown(object sender, MouseEventArgs e)
        {
            mousePos = new Point(e.X, e.Y);
            mouseClicked = true;
        }

        private void Close_MouseClick(object sender, MouseEventArgs e)
        {
            Application.Exit();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            hiderTimer.Enabled = false;
            close.Visible = false;
            resizer.Visible = false;
            maximize.Visible = false;
            minimize.Visible = false;
            settings.Visible = false;
            volumeIndicator.Visible = false;
            hiderTimer.Stop();
        }

        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            dragEnter = true;
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            linkApply(e.Data.GetData(DataFormats.Text, false).ToString());
            dragEnter = false;
        }

        private void Form1_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.UnicodeText)) e.Effect = DragDropEffects.Copy;
                else e.Effect = DragDropEffects.None;
        }

        private void Form1_DragLeave(object sender, EventArgs e)
        {
            dragEnter = false;
        }

        private void Form1_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta > 0)
            {
                Settings.Volume += volumeIndicatorSteps;
            }
            else if (e.Delta < 0)
            {
                Settings.Volume -= volumeIndicatorSteps;
            }
            if (volume > 100) volume = 100;
            if (volume < 0) volume = 0;
            displayVolume();

            chrome.ExecuteScriptAsync("SetVolume(" + volume + ");");
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Left:
                    {
                        ReleaseCapture();
                        SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
                    }
                    break;
            }
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {

        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            close.Visible = true;
            resizer.Visible = true;
            maximize.Visible = true;
            minimize.Visible = true;
            settings.Visible = true;
            volumeIndicator.Visible = true;
            hiderTimer.Start();
            
        }

        private void Form1_MouseHover(object sender, EventArgs e)
        {
            //////////////////////
        }

        private void Form1_MouseLeave(object sender, EventArgs e)
        {
            //////////////////////
        }

        private void Form1_MouseEnter(object sender, EventArgs e)
        {
            //////////////////////
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.F5: chrome.GetBrowser().Reload();
                    break;
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            settingsPanel.Enabled = false;
            settingsPanel.Dispose();
            Cef.Shutdown();
            if (Settings.ClearCacheOnExit) Settings.ClearCache();
            if (Settings.RememberMainformPosition) Settings.MainformPosition = Location;
            if (Settings.RememberMainformSize)
            {
                switch (WindowState)
                {
                    case FormWindowState.Maximized: Settings.MainformMaximized = true;
                        break;
                    case FormWindowState.Normal: Settings.MainformSize = ClientSize;
                        break;
                }
            }
            Settings.SaveSettings();
        }

        // This function draws the buttons on the UI
        void drawButtons()
        {
            Pen defaultPen = new Pen(Color.FromArgb(255, 58, 59, 59), 2f);
            SolidBrush defaultBrush = new SolidBrush(Color.FromArgb(255, 58, 59, 59));

            Bitmap tmp = new Bitmap(buttonSize.Width, buttonSize.Height);
            Graphics g = Graphics.FromImage(tmp);
            g.FillRectangle(Brushes.Black, new Rectangle(0, 0, buttonSize.Width, buttonSize.Height));
            g.DrawLine(defaultPen, new Point(0, 0), new Point(buttonSize.Width, buttonSize.Height));
            g.DrawLine(defaultPen, new Point(0, buttonSize.Height), new Point(buttonSize.Width, 0));
            close.Image = tmp;

            tmp = new Bitmap(buttonSize.Width, buttonSize.Height);
            g = Graphics.FromImage(tmp);
            g.FillRectangle(Brushes.Black, new Rectangle(0, 0, buttonSize.Width, buttonSize.Height));
            g.DrawLine(defaultPen, 0, 0, 18, 18);
            g.DrawLine(defaultPen, 16, 5, 18, 18);
            g.DrawLine(defaultPen, 5, 16, 18, 18);
            resizer.Image = tmp;

            tmp = new Bitmap(buttonSize.Width, buttonSize.Height);
            g = Graphics.FromImage(tmp);
            g.FillRectangle(Brushes.Black, new Rectangle(0, 0, buttonSize.Width, buttonSize.Height));
            g.DrawLine(defaultPen, 2, 4, 2, 16);
            g.DrawLine(defaultPen, 2, 16, 18, 16);
            g.DrawLine(defaultPen, 18, 16, 18, 4);
            g.DrawLine(defaultPen, 18, 4, 2, 4);
            maximize.Image = tmp;

            tmp = new Bitmap(buttonSize.Width, buttonSize.Height);
            g = Graphics.FromImage(tmp);
            g.FillRectangle(Brushes.Black, new Rectangle(0, 0, buttonSize.Width, buttonSize.Height));
            g.DrawLine(defaultPen, 2, buttonSize.Height / 2, buttonSize.Width - 2, buttonSize.Height / 2);
            minimize.Image = tmp;

            tmp = new Bitmap(buttonSize.Width, buttonSize.Height);
            g = Graphics.FromImage(tmp);
            g.FillRectangle(Brushes.Black, new Rectangle(0, 0, buttonSize.Width, buttonSize.Height));
            g.DrawImage(Properties.Resources.gear_512, 0, 0, buttonSize.Width, buttonSize.Height);
            settings.Image = tmp;

            tmp = new Bitmap(settingsPanel.Width, settingsPanel.Height);
            g = Graphics.FromImage(tmp);
            FontFamily fontFamily = new FontFamily("Arial");
            Font font = new Font(
               fontFamily,
               12,
               FontStyle.Regular,
               GraphicsUnit.Point);
            g.DrawString("Settings", font, Brushes.Gray, new Point(0, 0));
            settingsPanel.BackgroundImage = tmp;

            GC.Collect();
        }

        // This function draws the visible window
        void drawRoundedFormWindow()
        {
            Bitmap bmp = new Bitmap(ClientSize.Width, ClientSize.Height);
            Graphics gfx = Graphics.FromImage(bmp);

            Rectangle Bounds = new Rectangle(new Point(0, 0), ClientSize);
            int CornerRadius = 50;
            Pen DrawPen = Pens.Black;
            Color FillColor = Color.Black;

            int strokeOffset = Convert.ToInt32(Math.Ceiling(DrawPen.Width));
            Bounds = Rectangle.Inflate(Bounds, -strokeOffset, -strokeOffset);

            GraphicsPath gfxPath = new GraphicsPath();
            gfxPath.AddArc(Bounds.X, Bounds.Y, CornerRadius, CornerRadius, 180, 90);
            gfxPath.AddArc(Bounds.X + Bounds.Width - CornerRadius, Bounds.Y, CornerRadius, CornerRadius, 270, 90);
            gfxPath.AddArc(Bounds.X + Bounds.Width - CornerRadius, Bounds.Y + Bounds.Height - CornerRadius, CornerRadius, CornerRadius, 0, 90);
            gfxPath.AddArc(Bounds.X, Bounds.Y + Bounds.Height - CornerRadius, CornerRadius, CornerRadius, 90, 90);
            gfxPath.CloseAllFigures();

            gfx.FillPath(new SolidBrush(FillColor), gfxPath);
            gfx.DrawPath(DrawPen, gfxPath);

            this.BackgroundImage = bmp;

            GC.Collect();
        }

        // This function shows the volume indicator
        void displayVolume()
        {
            Bitmap bmp = new Bitmap(ClientSize.Width - 100, 20);
            Graphics g = Graphics.FromImage(bmp);

            double volumeRatio = volume / 100f;
            g.FillRectangle(Brushes.DarkGray, 0, 0, Convert.ToInt32(bmp.Width * volumeRatio), 20);

            volumeIndicator.Image = bmp;
        }

        
        // This function relocates the buttons (it is called when the window gets resized)
        void relocator()
        {
            if (WindowState == FormWindowState.Maximized)
            {
                close.Location = new Point(ClientSize.Width - close.Size.Width - 2, 2);
                resizer.Location = new Point(ClientSize.Width - resizer.Size.Width - 2, ClientSize.Height - resizer.Size.Height - 2);
                maximize.Location = new Point(close.Location.X - buttonSize.Width - 7, 2);
                minimize.Location = new Point(close.Location.X - buttonSize.Width - buttonSize.Width - 14, 2);
                settings.Location = new Point(2, 2);

                this.BackColor = Color.Black;
                this.BackgroundImage = null;
            }
            else
            {
                close.Location = new Point(ClientSize.Width - close.Size.Width - 20, 2);
                resizer.Location = new Point(ClientSize.Width - resizer.Size.Width - 20, ClientSize.Height - resizer.Size.Height - 2);
                maximize.Location = new Point(close.Location.X - buttonSize.Width - 7, 2);
                minimize.Location = new Point(close.Location.X - buttonSize.Width - buttonSize.Width - 14, 2);
                settings.Location = new Point(20, 2);

                this.BackColor = this.TransparencyKey;

                drawRoundedFormWindow();
            }
            displayVolume();
        }

    }
}

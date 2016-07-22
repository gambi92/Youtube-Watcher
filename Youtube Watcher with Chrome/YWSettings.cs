using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;
using System.Windows.Forms;



/// <summary>
/// 
/// 
/// Save maximized state too!!!!
/// 
/// 
/// </summary>

/* settings.cfg format:

    clearCacheOnExit=0
    rememberMainformPosition=0
    rememberMainformSize=0
    mainformMaximized=0
    alwaysOnTop=1
    mainformSize=400x300
    mainformPosition=0x0
    volume=100
    volumeIndicatorSteps=10

*/

namespace Youtube_Watcher_with_Chrome
{
    class YWSettings
    {
        Form1 mainForm;
        string settingsFilePath = "YWsettings.cfg";
        List<string> rawSettings = new List<string>();
        bool clearCacheOnExit = false;
        bool rememberMainformPosition = false;
        bool rememberMainformSize = false;
        bool mainformMaximized = false;
        bool alwaysOnTop = true;
        Size mainformSize;
        Point mainformPosition;
        // Default Size and Position!!!

        
        // Soo many unnecessary variables in this class!!!!! (I could have used the mainform's variables!)
        #region Properties
        public bool ClearCacheOnExit { get { return clearCacheOnExit; }
            set
            {
                clearCacheOnExit = value;
                int tmpIndex = rawSettings.FindIndex(a => a.Contains("clearCacheOnExit="));
                if (tmpIndex >= 0 && tmpIndex < rawSettings.Count)
                {
                    if (value) rawSettings[tmpIndex] = "clearCacheOnExit=1";
                        else rawSettings[tmpIndex] = "clearCacheOnExit=0";
                }
                else
                {
                    if (value) rawSettings.Add("clearCacheOnExit=1");
                        else rawSettings.Add("clearCacheOnExit=0");
                }
                SaveSettings();

            }
        }

        public bool RememberMainformPosition { get { return rememberMainformPosition; }
            set
            {
                rememberMainformPosition = value;
                int tmpIndex = rawSettings.FindIndex(a => a.Contains("rememberMainformPosition="));
                if (tmpIndex >= 0 && tmpIndex < rawSettings.Count)
                {
                    if (value) rawSettings[tmpIndex] = "rememberMainformPosition=1";
                    else rawSettings[tmpIndex] = "rememberMainformPosition=0";
                }
                else
                {
                    if (value) rawSettings.Add("rememberMainformPosition=1");
                    else rawSettings.Add("rememberMainformPosition=0");
                }
                SaveSettings();
            }
        }

        public bool RememberMainformSize { get { return rememberMainformSize; }
            set
            {
                rememberMainformSize = value;
                int tmpIndex = rawSettings.FindIndex(a => a.Contains("rememberMainformSize="));
                if (tmpIndex >= 0 && tmpIndex < rawSettings.Count)
                {
                    if (value) rawSettings[tmpIndex] = "rememberMainformSize=1";
                    else rawSettings[tmpIndex] = "rememberMainformSize=0";
                }
                else
                {
                    if (value) rawSettings.Add("rememberMainformSize=1");
                    else rawSettings.Add("rememberMainformSize=0");
                }
                SaveSettings();
            }
        }

        public bool MainformMaximized { get { return mainformMaximized; }
            set
            {
                mainformMaximized = value;
                int tmpIndex = rawSettings.FindIndex(a => a.Contains("mainformMaximized="));
                if (tmpIndex >= 0 && tmpIndex < rawSettings.Count)
                {
                    if (value) rawSettings[tmpIndex] = "mainformMaximized=1";
                    else rawSettings[tmpIndex] = "mainformMaximized=0";
                }
                else
                {
                    if (value) rawSettings.Add("mainformMaximized=1");
                    else rawSettings.Add("mainformMaximized=0");
                }
                SaveSettings();
            }
        }

        public bool AlwaysOnTop { get { return alwaysOnTop; }
            set
            {
                alwaysOnTop = value;
                mainForm.TopMost = value;
                int tmpIndex = rawSettings.FindIndex(a => a.Contains("alwaysOnTop="));
                if (tmpIndex >= 0 && tmpIndex < rawSettings.Count)
                {
                    if (value) rawSettings[tmpIndex] = "alwaysOnTop=1";
                    else rawSettings[tmpIndex] = "alwaysOnTop=0";
                }
                else
                {
                    if (value) rawSettings.Add("alwaysOnTop=1");
                    else rawSettings.Add("alwaysOnTop=0");
                }
                SaveSettings();
            }
        }

        public Size MainformSize { get { return mainformSize; }
            set
            {
                mainformSize = value;
                int tmpIndex = rawSettings.FindIndex(a => a.Contains("mainformSize="));
                if (tmpIndex >= 0 && tmpIndex < rawSettings.Count)
                {
                    rawSettings[tmpIndex] = "mainformSize=" + mainformSize.Width + "x" + mainformSize.Height;
                }
                else
                {
                    rawSettings.Add("mainformSize=" + mainformSize.Width + "x" + mainformSize.Height);
                }
                SaveSettings();
            }
        }

        public Point MainformPosition { get { return mainformPosition; }
            set
            {
                mainformPosition = value;
                int tmpIndex = rawSettings.FindIndex(a => a.Contains("mainformPosition="));
                if (tmpIndex >= 0 && tmpIndex < rawSettings.Count)
                {
                    rawSettings[tmpIndex] = "mainformPosition=" + mainformPosition.X + "x" + mainformPosition.Y;
                }
                else
                {
                    rawSettings.Add("mainformPosition=" + mainformPosition.X + "x" + mainformPosition.Y);
                }
                SaveSettings();
            }
        }

        // Volume property (it's not used yet)
        public int Volume { get { return mainForm.volume; }
            set
            {
                mainForm.volume = value;
                int tmpIndex = rawSettings.FindIndex(a => a.Contains("volume="));
                if (tmpIndex >= 0 && tmpIndex < rawSettings.Count)
                {
                    rawSettings[tmpIndex] = "volume=" + mainForm.volume;
                }
                else
                {
                    rawSettings.Add("volume=" + mainForm.volume);
                }
                SaveSettings();
            }
        }

        public int VolumeIndicatorSteps { get { return mainForm.volumeIndicatorSteps; }
            set
            {
                mainForm.volumeIndicatorSteps = value;
                int tmpIndex = rawSettings.FindIndex(a => a.Contains("volumeIndicatorSteps="));
                if (tmpIndex >= 0 && tmpIndex < rawSettings.Count)
                {
                    rawSettings[tmpIndex] = "volumeIndicatorSteps=" + mainForm.volumeIndicatorSteps;
                }
                else
                {
                    rawSettings.Add("volumeIndicatorSteps=" + mainForm.volumeIndicatorSteps);
                }
                SaveSettings();
            }
        }
        #endregion

        public YWSettings(Form1 mainForm)
        {
            this.mainForm = mainForm;
            if (!File.Exists(settingsFilePath))
            {
                ResetSettings();
            }
            else
            {
                LoadSettings();
            }
        }

        public void ClearCache()
        {
            try
            {
                Directory.Delete("cache", true);
                Directory.CreateDirectory("cache");
            }

            catch
            {
                //MessageBox.Show("I couldn't delete cache, because I don't have permission!");
            }
            
        }

        public void LoadSettings()
        {
            try
            {
                rawSettings = File.ReadLines(settingsFilePath).ToList();
            }
            catch
            {
                MessageBox.Show("I couldn't load settings, because I don't have permission to read my own folder!");
            }
            
            if (rawSettings.Contains("clearCacheOnExit=1")) clearCacheOnExit = true;
            if (rawSettings.Contains("rememberMainformPosition=1")) rememberMainformPosition = true;
            if (rawSettings.Contains("rememberMainformSize=1")) rememberMainformSize = true;
            if (rawSettings.Contains("mainformMaximized=1")) mainformMaximized = true;
            if (rawSettings.Contains("alwaysOnTop=0")) alwaysOnTop = false;

            int tmpIndex;
            string[] rawSize;
            string[] rawPosition;
            string[] rawVolume;
            string[] rawVolumeIndicatorSteps;


            tmpIndex = rawSettings.FindIndex(a => a.Contains("mainformSize="));
            if (tmpIndex >= 0 && tmpIndex < rawSettings.Count)
            {
                rawSize = rawSettings[tmpIndex].Substring(13).Split('x');
                mainformSize = new Size(Convert.ToInt32(rawSize[0]), Convert.ToInt32(rawSize[1]));
            } else mainformSize = mainForm.Size;

            tmpIndex = rawSettings.FindIndex(a => a.Contains("mainformPosition="));
            if (tmpIndex >= 0 && tmpIndex < rawSettings.Count)
            {
                rawPosition = rawSettings[tmpIndex].Substring(17).Split('x');
                mainformPosition = new Point(Convert.ToInt32(rawPosition[0]), Convert.ToInt32(rawPosition[1]));
            } else mainformPosition = mainForm.Location;

            if (clearCacheOnExit)
            {
                mainForm.checkbox_clearCacheOnExit.Checked = true;
            }
            else
            {
                mainForm.checkbox_clearCacheOnExit.Checked = false;
            }

            if (rememberMainformSize)
            {
                mainForm.checkbox_rememberMainformSize.Checked = true;
                if (mainformMaximized) mainForm.WindowState = FormWindowState.Maximized;
                    else mainForm.WindowState = FormWindowState.Normal;
                mainForm.ClientSize = mainformSize;
            }
            else
            {
                mainForm.checkbox_rememberMainformSize.Checked = false;
            }

            if (rememberMainformPosition)
            {
                mainForm.checkbox_rememberMainformPosition.Checked = true;
                mainForm.Location = mainformPosition;
            }
            else
            {
                mainForm.checkbox_rememberMainformPosition.Checked = false;
                mainForm.Location = new Point(30, Screen.PrimaryScreen.WorkingArea.Height - mainForm.ClientSize.Height - 30);
            }

            if (alwaysOnTop)
            {
                mainForm.checkbox_alwaysOnTop.Checked = true;
                mainForm.TopMost = true;
            }
            else
            {
                mainForm.checkbox_alwaysOnTop.Checked = false;
                mainForm.TopMost = false;
            }

            tmpIndex = rawSettings.FindIndex(a => a.Contains("volume="));
            if (tmpIndex >= 0 && tmpIndex < rawSettings.Count)
            {
                rawVolume = rawSettings[tmpIndex].Split('=');
                mainForm.volume = Convert.ToInt32(rawVolume[1]);
            }

            tmpIndex = rawSettings.FindIndex(a => a.Contains("volumeIndicatorSteps="));
            if (tmpIndex >= 0 && tmpIndex < rawSettings.Count)
            {
                rawVolumeIndicatorSteps = rawSettings[tmpIndex].Split('=');
                int tmpValue = Convert.ToInt32(rawVolumeIndicatorSteps[1]);
                mainForm.volumeIndicatorSteps = tmpValue;
                mainForm.label_volumeIndicatorSteps.Text = tmpValue.ToString();
                mainForm.scrollbar_volumeIndicatorSteps.Value = tmpValue;
            }
        }

        public void SaveSettings()
        {
            try
            {
                File.WriteAllLines(settingsFilePath, rawSettings);
            }
            catch
            {
                MessageBox.Show("I couldn't save settings, because I don't have permission to modify my own folder!");
            }
            
        }

        public void ResetSettings()
        {
            // OPTIMIZE!!!!
            try
            {
                if (File.Exists(settingsFilePath)) File.Delete(settingsFilePath);
                File.Create(settingsFilePath).Close();
            }
            catch
            {
                MessageBox.Show("I couldn't reset settings, because I don't have permission to modify my own folder!");
            }
            rawSettings = new List<string>();
            rawSettings.Add("clearCacheOnExit=0");
            rawSettings.Add("rememberMainformPosition=0");
            rawSettings.Add("rememberMainformSize=0");
            rawSettings.Add("mainformMaximized=0");
            rawSettings.Add("alwaysOnTop=1");
            rawSettings.Add("mainformSize=" + mainForm.ClientSize.Width + "x" + mainForm.ClientSize.Height);
            rawSettings.Add("mainformPosition=" + mainForm.Location.X + "x" + mainForm.Location.Y);
            clearCacheOnExit = false;
            rememberMainformPosition = false;
            rememberMainformSize = false;
            mainformMaximized = false;
            alwaysOnTop = true;
            mainformSize = mainForm.ClientSize;
            mainformPosition = mainForm.Location;
            SaveSettings();

            mainForm.checkbox_alwaysOnTop.Checked = alwaysOnTop;

            mainForm.TopMost = alwaysOnTop;
        }

    }
}

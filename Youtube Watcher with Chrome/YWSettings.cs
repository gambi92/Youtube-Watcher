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

    clearCacheOnExit=1
    rememberMainformPosition=0
    rememberMainformSize=0
    mainformMaximized=0
    mainformSize=400x300
    mainformPosition=0x0

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
            Directory.Delete("cache", true);
            Directory.CreateDirectory("cache");
        }

        public void LoadSettings()
        {
            rawSettings = File.ReadLines(settingsFilePath).ToList();
            
            if (rawSettings.Contains("clearCacheOnExit=1")) clearCacheOnExit = true;
            if (rawSettings.Contains("rememberMainformPosition=1")) rememberMainformPosition = true;
            if (rawSettings.Contains("rememberMainformSize=1")) rememberMainformSize = true;
            if (rawSettings.Contains("mainformMaximized=1")) mainformMaximized = true;
            if (rawSettings.Contains("alwaysOnTop=0")) alwaysOnTop = false;

            string[] rawSize;
            string[] rawPosition;

            int tmpSizeIndex = rawSettings.FindIndex(a => a.Contains("mainformSize="));
            if (tmpSizeIndex >= 0 && tmpSizeIndex < rawSettings.Count)
            {
                rawSize = rawSettings[tmpSizeIndex].Substring(13).Split('x');
                mainformSize = new Size(Convert.ToInt32(rawSize[0]), Convert.ToInt32(rawSize[1]));
            } else mainformSize = mainForm.Size;

            int tmpPositionIndex = rawSettings.FindIndex(a => a.Contains("mainformPosition="));
            if (tmpPositionIndex >= 0 && tmpPositionIndex < rawSettings.Count)
            {
                rawPosition = rawSettings[tmpPositionIndex].Substring(17).Split('x');
                mainformPosition = new Point(Convert.ToInt32(rawPosition[0]), Convert.ToInt32(rawPosition[1]));
            } else mainformPosition = mainForm.Location;
            
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
        }

        public void SaveSettings()
        {
            File.WriteAllLines(settingsFilePath, rawSettings);
        }

        public void ResetSettings()
        {
            if (File.Exists(settingsFilePath)) File.Delete(settingsFilePath);
            File.Create(settingsFilePath).Close();
            rawSettings = new List<string>();
            rawSettings.Add("clearCacheOnExit=0");
            rawSettings.Add("rememberMainformPosition=0");
            rawSettings.Add("rememberMainformSize=0");
            rawSettings.Add("mainformMaximized=0");
            rawSettings.Add("alwaysOnTop=1");
            rawSettings.Add("mainformSize=" + mainForm.ClientSize.Width + "x" + mainForm.ClientSize.Height);
            rawSettings.Add("mainformPosition=" + mainForm.Location.X + "x" + mainForm.Location.Y);
            SaveSettings();
            clearCacheOnExit = false;
            rememberMainformPosition = false;
            rememberMainformSize = false;
            mainformMaximized = false;
            alwaysOnTop = true;
            mainformSize = mainForm.ClientSize;
            mainformPosition = mainForm.Location;
        }

    }
}

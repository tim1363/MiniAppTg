using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using Microsoft.Web.WebView2.WinForms;
using Microsoft.Web.WebView2.Core;
using System.Drawing;

namespace MultiTelegram
{
    public class TelegramInstanceSimple
    {
        public string InstanceId { get; private set; }
        public string UserDataFolder { get; private set; }
        public Form InstanceForm { get; private set; }
        public WebView2 WebView { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public string UserAgent { get; private set; }

        private static readonly string[] UserAgents = {
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/119.0.0.0 Safari/537.36"
        };

        public TelegramInstanceSimple()
        {
            InstanceId = Guid.NewGuid().ToString();
            CreatedAt = DateTime.Now;
            UserAgent = UserAgents[new Random().Next(UserAgents.Length)];
            
            // Create unique folder for user data
            var appDataFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), 
                "MultiTelegram", "Instances");
            
            UserDataFolder = Path.Combine(appDataFolder, InstanceId);
            Directory.CreateDirectory(UserDataFolder);
            
            CreateInstanceWindow();
        }

        public TelegramInstanceSimple(InstanceInfo info)
        {
            InstanceId = info.InstanceId;
            UserDataFolder = info.UserDataFolder;
            UserAgent = info.UserAgent;
            CreatedAt = info.CreatedAt;
            
            if (!Directory.Exists(UserDataFolder))
            {
                Directory.CreateDirectory(UserDataFolder);
            }
            
            CreateInstanceWindow();
            
            // Restore window position and size
            InstanceForm.Location = new Point(info.WindowX, info.WindowY);
            InstanceForm.Size = new Size(info.WindowWidth, info.WindowHeight);
            
            if (!info.IsVisible)
            {
                InstanceForm.WindowState = FormWindowState.Minimized;
            }
        }

        private void CreateInstanceWindow()
        {
            InstanceForm = new Form
            {
                Text = $"Telegram - {InstanceId.Substring(0, 8)}",
                Size = new Size(420, 700),
                StartPosition = FormStartPosition.CenterScreen,
                ShowIcon = false,
                MinimizeBox = true,
                MaximizeBox = true
            };

            // Create WebView2
            WebView = new WebView2
            {
                Dock = DockStyle.Fill
            };

            InstanceForm.Controls.Add(WebView);
            
            // Handle window events
            InstanceForm.FormClosed += (s, e) => {
                OnInstanceClosed?.Invoke(this);
                CleanUp();
            };
            
            InstanceForm.FormClosing += (s, e) => {
                try
                {
                    var settings = SettingsManager.LoadSettings();
                    var instanceInfo = settings.SavedInstances.Find(i => i.InstanceId == InstanceId);
                    if (instanceInfo != null)
                    {
                        instanceInfo.WindowX = InstanceForm.Location.X;
                        instanceInfo.WindowY = InstanceForm.Location.Y;
                        instanceInfo.WindowWidth = InstanceForm.Size.Width;
                        instanceInfo.WindowHeight = InstanceForm.Size.Height;
                        SettingsManager.SaveSettings(settings);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error saving window position: {ex.Message}");
                }
            };
            
            // Setup WebView2
            InitializeWebView();
        }

        private async void InitializeWebView()
        {
            try
            {
                // Create environment with user data folder
                var environment = await CoreWebView2Environment.CreateAsync(
                    browserExecutableFolder: null,
                    userDataFolder: UserDataFolder);

                await WebView.EnsureCoreWebView2Async(environment);

                // Set User-Agent
                WebView.CoreWebView2.Settings.UserAgent = UserAgent;
                
                // Basic settings
                WebView.CoreWebView2.Settings.IsWebMessageEnabled = false;
                
                // Navigation events
                WebView.CoreWebView2.NavigationStarting += (s, e) =>
                {
                    InstanceForm.Text = $"Telegram - {InstanceId.Substring(0, 8)} (Loading...)";
                };

                WebView.CoreWebView2.NavigationCompleted += (s, e) =>
                {
                    InstanceForm.Text = $"Telegram - {InstanceId.Substring(0, 8)}";
                };

                // Navigate to Telegram
                WebView.CoreWebView2.Navigate("https://web.telegram.org/k/");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"WebView2 error: {ex.Message}\n\nPlease install Microsoft Edge WebView2 Runtime.", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                
                // Fallback
                var label = new Label
                {
                    Text = "WebView2 not available.\nPlease install Microsoft Edge WebView2 Runtime.\n\nDownload from: https://developer.microsoft.com/microsoft-edge/webview2/",
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("Segoe UI", 10),
                    ForeColor = Color.Red
                };
                InstanceForm.Controls.Clear();
                InstanceForm.Controls.Add(label);
            }
        }

        public void Show()
        {
            InstanceForm?.Show();
            InstanceForm?.BringToFront();
            if (InstanceForm?.WindowState == FormWindowState.Minimized)
            {
                InstanceForm.WindowState = FormWindowState.Normal;
            }
        }

        public void Hide()
        {
            InstanceForm?.Hide();
        }

        public void Close()
        {
            try
            {
                InstanceForm?.Close();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error closing instance: {ex.Message}");
            }
        }

        private void CleanUp()
        {
            try
            {
                Debug.WriteLine($"Instance {InstanceId} cleaned up");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error during cleanup: {ex.Message}");
            }
        }

        public event Action<TelegramInstanceSimple> OnInstanceClosed;
    }
}
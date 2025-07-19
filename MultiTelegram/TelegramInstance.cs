using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using Microsoft.Web.WebView2.WinForms;
using Microsoft.Web.WebView2.Core;
using System.Drawing;

namespace MultiTelegram
{
    public class TelegramInstance
    {
        public string InstanceId { get; private set; }
        public string UserDataFolder { get; private set; }
        public Form InstanceForm { get; private set; }
        public WebView2 WebView { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public string UserAgent { get; private set; }

        private static readonly string[] UserAgents = {
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/119.0.0.0 Safari/537.36",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:109.0) Gecko/20100101 Firefox/121.0",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Edge/120.0.0.0 Safari/537.36",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/118.0.0.0 Safari/537.36"
        };

        public TelegramInstance()
        {
            InstanceId = Guid.NewGuid().ToString();
            CreatedAt = DateTime.Now;
            UserAgent = UserAgents[new Random().Next(UserAgents.Length)];
            
            // Create unique folder for user data in persistent location
            var appDataFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), 
                "MultiTelegram", "Instances");
            
            UserDataFolder = Path.Combine(appDataFolder, InstanceId);
            Directory.CreateDirectory(UserDataFolder);
            
            CreateInstanceWindow();
        }

        public TelegramInstance(InstanceInfo info)
        {
            InstanceId = info.InstanceId;
            UserDataFolder = info.UserDataFolder;
            UserAgent = info.UserAgent;
            CreatedAt = info.CreatedAt;
            
            // Ensure folder exists
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
                MaximizeBox = true,
                Icon = null // Will be set later if available
            };

            // Create WebView2 for displaying Telegram Web
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
                // Save position before closing
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
            };
            
            // Setup WebView2
            InitializeWebView();
        }

        private async void InitializeWebView()
        {
            try
            {
                // Setup WebView2 environment with unique user data folder
                var environment = await CoreWebView2Environment.CreateAsync(
                    browserExecutableFolder: null,
                    userDataFolder: UserDataFolder);

                await WebView.EnsureCoreWebView2Async(environment);

                // Setup User-Agent
                WebView.CoreWebView2.Settings.UserAgent = UserAgent;
                
                // Setup additional parameters for bypass detection
                WebView.CoreWebView2.Settings.ArePasswordAutosaveEnabled = true;
                WebView.CoreWebView2.Settings.IsGeneralAutofillEnabled = true;
                WebView.CoreWebView2.Settings.IsWebMessageEnabled = false;
                WebView.CoreWebView2.Settings.AreDevToolsEnabled = false;
                WebView.CoreWebView2.Settings.AreDefaultScriptDialogsEnabled = true;
                WebView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = true;
                
                // Add headers for bypass restrictions
                await WebView.CoreWebView2.AddWebResourceRequestedFilterAsync("*", CoreWebView2WebResourceContext.All);

                WebView.CoreWebView2.WebResourceRequested += (s, e) =>
                {
                    try
                    {
                        // Add random headers to bypass detection
                        e.Request.Headers.Add("X-Forwarded-For", GenerateRandomIP());
                        e.Request.Headers.Add("X-Real-IP", GenerateRandomIP());
                        e.Request.Headers.Add("Accept-Language", "en-US,en;q=0.9,ru;q=0.8");
                        e.Request.Headers.Add("Accept-Encoding", "gzip, deflate, br");
                        e.Request.Headers.Add("DNT", "1");
                        e.Request.Headers.Add("Upgrade-Insecure-Requests", "1");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error adding headers: {ex.Message}");
                    }
                };

                // Handle navigation events
                WebView.CoreWebView2.NavigationStarting += (s, e) =>
                {
                    InstanceForm.Text = $"Telegram - {InstanceId.Substring(0, 8)} (Loading...)";
                };

                WebView.CoreWebView2.NavigationCompleted += (s, e) =>
                {
                    if (e.IsSuccess)
                    {
                        InstanceForm.Text = $"Telegram - {InstanceId.Substring(0, 8)}";
                    }
                    else
                    {
                        InstanceForm.Text = $"Telegram - {InstanceId.Substring(0, 8)} (Error)";
                    }
                };

                // Load Telegram Web
                WebView.CoreWebView2.Navigate("https://web.telegram.org/k/");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"WebView2 initialization error: {ex.Message}\n\nMake sure Microsoft Edge WebView2 is installed.", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                
                // Fallback: show simple message
                var label = new Label
                {
                    Text = "WebView2 not available.\nPlease install Microsoft Edge WebView2 Runtime.",
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("Segoe UI", 12),
                    ForeColor = Color.Red
                };
                InstanceForm.Controls.Clear();
                InstanceForm.Controls.Add(label);
            }
        }

        private string GenerateRandomIP()
        {
            var random = new Random();
            return $"{random.Next(1, 255)}.{random.Next(1, 255)}.{random.Next(1, 255)}.{random.Next(1, 255)}";
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
                // Don't delete user data folder to preserve Telegram session
                // The folder will be reused when instance is restored
                Debug.WriteLine($"Instance {InstanceId} cleaned up (data preserved)");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error during cleanup: {ex.Message}");
            }
        }

        public event Action<TelegramInstance> OnInstanceClosed;
    }
}
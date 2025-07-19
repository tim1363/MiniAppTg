using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using Microsoft.Web.WebView2.WinForms;
using Microsoft.Web.WebView2.Core;

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
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Edge/120.0.0.0 Safari/537.36"
        };

        public TelegramInstance()
        {
            InstanceId = Guid.NewGuid().ToString();
            CreatedAt = DateTime.Now;
            UserAgent = UserAgents[new Random().Next(UserAgents.Length)];
            
            // Создаем уникальную папку для данных пользователя
            UserDataFolder = Path.Combine(Path.GetTempPath(), "MultiTelegram", InstanceId);
            Directory.CreateDirectory(UserDataFolder);
            
            CreateInstanceWindow();
        }

        private void CreateInstanceWindow()
        {
            InstanceForm = new Form
            {
                Text = $"Telegram - {InstanceId.Substring(0, 8)}",
                Size = new System.Drawing.Size(420, 700),
                StartPosition = FormStartPosition.CenterScreen,
                ShowIcon = true,
                MinimizeBox = true,
                MaximizeBox = true
            };

            // Создаем WebView2 для отображения Telegram Web
            WebView = new WebView2
            {
                Dock = DockStyle.Fill
            };

            InstanceForm.Controls.Add(WebView);
            
            // Обработчик закрытия окна
            InstanceForm.FormClosed += (s, e) => OnInstanceClosed?.Invoke(this);
            
            // Настраиваем WebView2
            InitializeWebView();
        }

        private async void InitializeWebView()
        {
            try
            {
                // Настройка среды WebView2 с уникальной папкой данных
                var environment = await CoreWebView2Environment.CreateAsync(
                    browserExecutableFolder: null,
                    userDataFolder: UserDataFolder);

                await WebView.EnsureCoreWebView2Async(environment);

                // Настройка User-Agent
                WebView.CoreWebView2.Settings.UserAgent = UserAgent;
                
                // Настройка дополнительных параметров для обхода детекции
                WebView.CoreWebView2.Settings.ArePasswordAutosaveEnabled = true;
                WebView.CoreWebView2.Settings.IsGeneralAutofillEnabled = true;
                WebView.CoreWebView2.Settings.IsWebMessageEnabled = false;
                
                // Добавляем заголовки для обхода ограничений
                WebView.CoreWebView2.DOMContentLoaded += async (s, e) =>
                {
                    await WebView.CoreWebView2.AddWebResourceRequestedFilterAsync("*", CoreWebView2WebResourceContext.All);
                };

                WebView.CoreWebView2.WebResourceRequested += (s, e) =>
                {
                    var headers = e.Response?.Headers;
                    if (headers != null)
                    {
                        // Добавляем случайные заголовки
                        e.Request.Headers.Add("X-Forwarded-For", GenerateRandomIP());
                        e.Request.Headers.Add("X-Real-IP", GenerateRandomIP());
                        e.Request.Headers.Add("Accept-Language", "ru-RU,ru;q=0.9,en;q=0.8");
                    }
                };

                // Загружаем Telegram Web
                WebView.CoreWebView2.Navigate("https://web.telegram.org/k/");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка инициализации WebView2: {ex.Message}", "Ошибка", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                
                // Очищаем временные файлы
                if (Directory.Exists(UserDataFolder))
                {
                    Directory.Delete(UserDataFolder, true);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при закрытии экземпляра: {ex.Message}");
            }
        }

        public event Action<TelegramInstance> OnInstanceClosed;
    }
}
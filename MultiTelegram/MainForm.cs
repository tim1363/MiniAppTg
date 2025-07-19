using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Diagnostics;

namespace MultiTelegram
{
    public partial class MainForm : Form
    {
        private List<TelegramInstance> instances;
        private ListView instancesListView;
        private Label statusLabel;
        private Button createInstanceButton;
        private Button closeAllButton;
        private Label totalInstancesLabel;
        private Label memoryUsageLabel;
        private Timer updateTimer;

        public MainForm()
        {
            instances = new List<TelegramInstance>();
            InitializeComponent();
            SetupUpdateTimer();
        }

        private void InitializeComponent()
        {
            // Настройка главного окна
            this.Text = "Multi Telegram Manager - KoTim";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(600, 400);
            this.BackColor = Color.FromArgb(240, 240, 240);

            // Создание элементов интерфейса
            CreateControls();
            SetupLayout();
            SetupEventHandlers();
        }

        private void CreateControls()
        {
            // Заголовок
            var titleLabel = new Label
            {
                Text = "Multi Telegram Manager",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(51, 51, 51),
                AutoSize = true,
                Location = new Point(20, 20)
            };

            // Кнопки управления
            createInstanceButton = new Button
            {
                Text = "➕ Создать новый экземпляр Telegram",
                Size = new Size(250, 40),
                Location = new Point(20, 60),
                BackColor = Color.FromArgb(0, 136, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Regular)
            };
            createInstanceButton.FlatAppearance.BorderSize = 0;

            closeAllButton = new Button
            {
                Text = "❌ Закрыть все экземпляры",
                Size = new Size(200, 40),
                Location = new Point(280, 60),
                BackColor = Color.FromArgb(232, 17, 35),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Regular)
            };
            closeAllButton.FlatAppearance.BorderSize = 0;

            // Статистика
            var statsPanel = new Panel
            {
                Size = new Size(760, 80),
                Location = new Point(20, 110),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            totalInstancesLabel = new Label
            {
                Text = "Активных экземпляров: 0",
                Location = new Point(20, 20),
                Font = new Font("Segoe UI", 12, FontStyle.Regular),
                AutoSize = true
            };

            memoryUsageLabel = new Label
            {
                Text = "Использование памяти: ~0 MB",
                Location = new Point(20, 45),
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                AutoSize = true,
                ForeColor = Color.FromArgb(102, 102, 102)
            };

            statsPanel.Controls.AddRange(new Control[] { totalInstancesLabel, memoryUsageLabel });

            // Список экземпляров
            instancesListView = new ListView
            {
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                Location = new Point(20, 200),
                Size = new Size(760, 300),
                Font = new Font("Segoe UI", 10, FontStyle.Regular)
            };

            // Добавляем колонки
            instancesListView.Columns.Add("ID", 100);
            instancesListView.Columns.Add("Создан", 150);
            instancesListView.Columns.Add("User Agent", 200);
            instancesListView.Columns.Add("Статус", 100);
            instancesListView.Columns.Add("Действия", 100);

            // Статусная строка
            statusLabel = new Label
            {
                Text = "Готов к работе",
                Location = new Point(20, 520),
                Size = new Size(760, 20),
                Font = new Font("Segoe UI", 9, FontStyle.Regular),
                ForeColor = Color.FromArgb(102, 102, 102)
            };

            // Добавляем все элементы на форму
            this.Controls.AddRange(new Control[] {
                titleLabel, createInstanceButton, closeAllButton, 
                statsPanel, instancesListView, statusLabel
            });
        }

        private void SetupLayout()
        {
            // Настройка привязки элементов при изменении размера окна
            this.Resize += (s, e) =>
            {
                var width = this.ClientSize.Width;
                var height = this.ClientSize.Height;

                instancesListView.Size = new Size(width - 40, height - 220);
                statusLabel.Location = new Point(20, height - 40);
                statusLabel.Size = new Size(width - 40, 20);
            };
        }

        private void SetupEventHandlers()
        {
            createInstanceButton.Click += CreateInstanceButton_Click;
            closeAllButton.Click += CloseAllButton_Click;
            instancesListView.DoubleClick += InstancesListView_DoubleClick;
            
            // Контекстное меню для списка
            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("Показать", null, ShowInstance_Click);
            contextMenu.Items.Add("Скрыть", null, HideInstance_Click);
            contextMenu.Items.Add("Закрыть", null, CloseInstance_Click);
            instancesListView.ContextMenuStrip = contextMenu;

            // Обработка закрытия приложения
            this.FormClosing += MainForm_FormClosing;
        }

        private void SetupUpdateTimer()
        {
            updateTimer = new Timer
            {
                Interval = 1000 // Обновление каждую секунду
            };
            updateTimer.Tick += UpdateTimer_Tick;
            updateTimer.Start();
        }

        private async void CreateInstanceButton_Click(object sender, EventArgs e)
        {
            try
            {
                statusLabel.Text = "Создание нового экземпляра...";
                createInstanceButton.Enabled = false;

                var instance = new TelegramInstance();
                instance.OnInstanceClosed += Instance_OnInstanceClosed;
                
                instances.Add(instance);
                instance.Show();

                UpdateInstancesList();
                statusLabel.Text = $"Создан новый экземпляр: {instance.InstanceId.Substring(0, 8)}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка создания экземпляра: {ex.Message}", "Ошибка", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                statusLabel.Text = "Ошибка создания экземпляра";
            }
            finally
            {
                createInstanceButton.Enabled = true;
            }
        }

        private void CloseAllButton_Click(object sender, EventArgs e)
        {
            if (instances.Count == 0) return;

            var result = MessageBox.Show(
                $"Вы уверены, что хотите закрыть все {instances.Count} экземпляров?",
                "Подтверждение",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                var instancesToClose = instances.ToList();
                foreach (var instance in instancesToClose)
                {
                    instance.Close();
                }
                instances.Clear();
                UpdateInstancesList();
                statusLabel.Text = "Все экземпляры закрыты";
            }
        }

        private void InstancesListView_DoubleClick(object sender, EventArgs e)
        {
            if (instancesListView.SelectedItems.Count > 0)
            {
                var selectedIndex = instancesListView.SelectedItems[0].Index;
                if (selectedIndex < instances.Count)
                {
                    var instance = instances[selectedIndex];
                    instance.Show();
                    instance.InstanceForm.BringToFront();
                }
            }
        }

        private void ShowInstance_Click(object sender, EventArgs e)
        {
            if (instancesListView.SelectedItems.Count > 0)
            {
                var selectedIndex = instancesListView.SelectedItems[0].Index;
                instances[selectedIndex].Show();
            }
        }

        private void HideInstance_Click(object sender, EventArgs e)
        {
            if (instancesListView.SelectedItems.Count > 0)
            {
                var selectedIndex = instancesListView.SelectedItems[0].Index;
                instances[selectedIndex].Hide();
            }
        }

        private void CloseInstance_Click(object sender, EventArgs e)
        {
            if (instancesListView.SelectedItems.Count > 0)
            {
                var selectedIndex = instancesListView.SelectedItems[0].Index;
                instances[selectedIndex].Close();
            }
        }

        private void Instance_OnInstanceClosed(TelegramInstance instance)
        {
            instances.Remove(instance);
            UpdateInstancesList();
            statusLabel.Text = $"Экземпляр {instance.InstanceId.Substring(0, 8)} закрыт";
        }

        private void UpdateInstancesList()
        {
            instancesListView.Items.Clear();

            for (int i = 0; i < instances.Count; i++)
            {
                var instance = instances[i];
                var item = new ListViewItem(new[]
                {
                    instance.InstanceId.Substring(0, 8),
                    instance.CreatedAt.ToString("HH:mm:ss"),
                    GetBrowserName(instance.UserAgent),
                    instance.InstanceForm.Visible ? "Активен" : "Скрыт",
                    "Управление"
                });

                instancesListView.Items.Add(item);
            }

            // Обновляем статистику
            totalInstancesLabel.Text = $"Активных экземпляров: {instances.Count}";
            memoryUsageLabel.Text = $"Использование памяти: ~{instances.Count * 100} MB";
        }

        private string GetBrowserName(string userAgent)
        {
            if (userAgent.Contains("Firefox")) return "Firefox";
            if (userAgent.Contains("Edge")) return "Edge";
            if (userAgent.Contains("Chrome")) return "Chrome";
            return "Unknown";
        }

        private void UpdateTimer_Tick(object sender, EventArgs e)
        {
            // Обновляем статистику каждую секунду
            if (instances.Count > 0)
            {
                // Можно добавить дополнительную логику обновления
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            updateTimer?.Stop();
            
            if (instances.Count > 0)
            {
                var result = MessageBox.Show(
                    $"У вас открыто {instances.Count} экземпляров Telegram. Закрыть их?",
                    "Подтверждение выхода",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }

                if (result == DialogResult.Yes)
                {
                    foreach (var instance in instances.ToList())
                    {
                        instance.Close();
                    }
                }
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // Горячие клавиши
            if (keyData == (Keys.Control | Keys.N))
            {
                CreateInstanceButton_Click(null, null);
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
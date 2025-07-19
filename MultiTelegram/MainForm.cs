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
            // Setup main window
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
            // Title
            var titleLabel = new Label
            {
                Text = "Multi Telegram Manager",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(51, 51, 51),
                AutoSize = true,
                Location = new Point(20, 20)
            };

            // Control buttons
            createInstanceButton = new Button
            {
                Text = "+ Create New Telegram Instance",
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
                Text = "X Close All Instances",
                Size = new Size(200, 40),
                Location = new Point(280, 60),
                BackColor = Color.FromArgb(232, 17, 35),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Regular)
            };
            closeAllButton.FlatAppearance.BorderSize = 0;

            // Statistics
            var statsPanel = new Panel
            {
                Size = new Size(760, 80),
                Location = new Point(20, 110),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            totalInstancesLabel = new Label
            {
                Text = "Active instances: 0",
                Location = new Point(20, 20),
                Font = new Font("Segoe UI", 12, FontStyle.Regular),
                AutoSize = true
            };

            memoryUsageLabel = new Label
            {
                Text = "Memory usage: ~0 MB",
                Location = new Point(20, 45),
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                AutoSize = true,
                ForeColor = Color.FromArgb(102, 102, 102)
            };

            statsPanel.Controls.AddRange(new Control[] { totalInstancesLabel, memoryUsageLabel });

            // Instances list
            instancesListView = new ListView
            {
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                Location = new Point(20, 200),
                Size = new Size(760, 300),
                Font = new Font("Segoe UI", 10, FontStyle.Regular)
            };

            // Add columns
            instancesListView.Columns.Add("ID", 100);
            instancesListView.Columns.Add("Created", 150);
            instancesListView.Columns.Add("User Agent", 200);
            instancesListView.Columns.Add("Status", 100);
            instancesListView.Columns.Add("Actions", 100);

            // Status bar
            statusLabel = new Label
            {
                Text = "Ready to work",
                Location = new Point(20, 520),
                Size = new Size(760, 20),
                Font = new Font("Segoe UI", 9, FontStyle.Regular),
                ForeColor = Color.FromArgb(102, 102, 102)
            };

            // Add all elements to form
            this.Controls.AddRange(new Control[] {
                titleLabel, createInstanceButton, closeAllButton, 
                statsPanel, instancesListView, statusLabel
            });
        }

        private void SetupLayout()
        {
            // Setup element binding when window size changes
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
            
            // Context menu for list
            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("Show", null, ShowInstance_Click);
            contextMenu.Items.Add("Hide", null, HideInstance_Click);
            contextMenu.Items.Add("Close", null, CloseInstance_Click);
            instancesListView.ContextMenuStrip = contextMenu;

            // Handle application closing
            this.FormClosing += MainForm_FormClosing;
        }

        private void SetupUpdateTimer()
        {
            updateTimer = new Timer
            {
                Interval = 1000 // Update every second
            };
            updateTimer.Tick += UpdateTimer_Tick;
            updateTimer.Start();
        }

        private async void CreateInstanceButton_Click(object sender, EventArgs e)
        {
            try
            {
                statusLabel.Text = "Creating new instance...";
                createInstanceButton.Enabled = false;

                var instance = new TelegramInstance();
                instance.OnInstanceClosed += Instance_OnInstanceClosed;
                
                instances.Add(instance);
                instance.Show();

                UpdateInstancesList();
                statusLabel.Text = $"New instance created: {instance.InstanceId.Substring(0, 8)}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Instance creation error: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                statusLabel.Text = "Instance creation failed";
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
                $"Are you sure you want to close all {instances.Count} instances?",
                "Confirmation",
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
                statusLabel.Text = "All instances closed";
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
            statusLabel.Text = $"Instance {instance.InstanceId.Substring(0, 8)} closed";
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
                    instance.InstanceForm.Visible ? "Active" : "Hidden",
                    "Manage"
                });

                instancesListView.Items.Add(item);
            }

            // Update statistics
            totalInstancesLabel.Text = $"Active instances: {instances.Count}";
            memoryUsageLabel.Text = $"Memory usage: ~{instances.Count * 100} MB";
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
            // Update statistics every second
            if (instances.Count > 0)
            {
                // Additional update logic can be added here
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            updateTimer?.Stop();
            
            if (instances.Count > 0)
            {
                var result = MessageBox.Show(
                    $"You have {instances.Count} Telegram instances open. Close them?",
                    "Exit Confirmation",
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
            // Hotkeys
            if (keyData == (Keys.Control | Keys.N))
            {
                CreateInstanceButton_Click(null, null);
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
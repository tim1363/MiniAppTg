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
        private Button restoreInstancesButton;
        private Label totalInstancesLabel;
        private Label memoryUsageLabel;
        private Timer updateTimer;
        private NotifyIcon notifyIcon;
        private AppSettings settings;

        public MainForm()
        {
            instances = new List<TelegramInstance>();
            settings = SettingsManager.LoadSettings();
            InitializeComponent();
            SetupSystemTray();
            SetupUpdateTimer();
            RestoreSavedInstances();
        }

        private void InitializeComponent()
        {
            // Setup main window
            this.Text = "Multi Telegram Manager - KoTim v1.0";
            this.Size = new Size(900, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(700, 500);
            this.BackColor = Color.FromArgb(245, 245, 245);

            // Создание элементов интерфейса
            CreateControls();
            SetupLayout();
            SetupEventHandlers();
        }

        private void CreateControls()
        {
            // Title with version
            var titleLabel = new Label
            {
                Text = "🚀 Multi Telegram Manager",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(41, 98, 255),
                AutoSize = true,
                Location = new Point(20, 15)
            };

            var versionLabel = new Label
            {
                Text = "v1.0.0 - Unlimited Telegram instances",
                Font = new Font("Segoe UI", 9, FontStyle.Regular),
                ForeColor = Color.FromArgb(102, 102, 102),
                AutoSize = true,
                Location = new Point(20, 45)
            };

            // Control buttons panel
            var buttonsPanel = new Panel
            {
                Size = new Size(860, 50),
                Location = new Point(20, 75),
                BackColor = Color.Transparent
            };

            createInstanceButton = new Button
            {
                Text = "➕ Create New Instance",
                Size = new Size(200, 40),
                Location = new Point(0, 5),
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            createInstanceButton.FlatAppearance.BorderSize = 0;
            createInstanceButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(34, 142, 58);

            restoreInstancesButton = new Button
            {
                Text = "🔄 Restore Saved",
                Size = new Size(150, 40),
                Location = new Point(210, 5),
                BackColor = Color.FromArgb(0, 123, 255),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                Cursor = Cursors.Hand
            };
            restoreInstancesButton.FlatAppearance.BorderSize = 0;
            restoreInstancesButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(0, 105, 217);

            closeAllButton = new Button
            {
                Text = "❌ Close All",
                Size = new Size(130, 40),
                Location = new Point(370, 5),
                BackColor = Color.FromArgb(220, 53, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                Cursor = Cursors.Hand
            };
            closeAllButton.FlatAppearance.BorderSize = 0;
            closeAllButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(200, 35, 51);

            buttonsPanel.Controls.AddRange(new Control[] { 
                createInstanceButton, restoreInstancesButton, closeAllButton 
            });

            // Statistics panel
            var statsPanel = new Panel
            {
                Size = new Size(860, 90),
                Location = new Point(20, 135),
                BackColor = Color.White,
                BorderStyle = BorderStyle.None
            };

            // Add border effect
            statsPanel.Paint += (s, e) =>
            {
                e.Graphics.DrawRectangle(new Pen(Color.FromArgb(230, 230, 230), 1), 
                    0, 0, statsPanel.Width - 1, statsPanel.Height - 1);
            };

            var statsTitle = new Label
            {
                Text = "📊 Statistics",
                Location = new Point(15, 10),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(51, 51, 51),
                AutoSize = true
            };

            totalInstancesLabel = new Label
            {
                Text = "Active instances: 0",
                Location = new Point(15, 35),
                Font = new Font("Segoe UI", 11, FontStyle.Regular),
                AutoSize = true,
                ForeColor = Color.FromArgb(76, 175, 80)
            };

            memoryUsageLabel = new Label
            {
                Text = "Estimated memory usage: ~0 MB",
                Location = new Point(15, 55),
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                AutoSize = true,
                ForeColor = Color.FromArgb(102, 102, 102)
            };

            var maxInstancesLabel = new Label
            {
                Text = $"Max instances: {settings.MaxInstances}",
                Location = new Point(300, 35),
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                AutoSize = true,
                ForeColor = Color.FromArgb(102, 102, 102)
            };

            var hotkeysLabel = new Label
            {
                Text = "Hotkeys: Ctrl+N (New), Double-click (Show/Hide)",
                Location = new Point(300, 55),
                Font = new Font("Segoe UI", 9, FontStyle.Italic),
                AutoSize = true,
                ForeColor = Color.FromArgb(158, 158, 158)
            };

            statsPanel.Controls.AddRange(new Control[] { 
                statsTitle, totalInstancesLabel, memoryUsageLabel, maxInstancesLabel, hotkeysLabel 
            });

            // Instances list
            instancesListView = new ListView
            {
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                Location = new Point(20, 240),
                Size = new Size(860, 380),
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White
            };

            // Add columns with better widths
            instancesListView.Columns.Add("Instance ID", 120);
            instancesListView.Columns.Add("Created At", 120);
            instancesListView.Columns.Add("Browser", 100);
            instancesListView.Columns.Add("Status", 80);
            instancesListView.Columns.Add("Window Position", 120);
            instancesListView.Columns.Add("Session Size", 100);
            instancesListView.Columns.Add("Actions", 120);

            // Status bar
            statusLabel = new Label
            {
                Text = "✅ Application ready. Click 'Create New Instance' to start.",
                Location = new Point(20, 640),
                Size = new Size(860, 20),
                Font = new Font("Segoe UI", 9, FontStyle.Regular),
                ForeColor = Color.FromArgb(102, 102, 102)
            };

            // Add all elements to form
            this.Controls.AddRange(new Control[] {
                titleLabel, versionLabel, buttonsPanel, statsPanel, instancesListView, statusLabel
            });
        }

        private void SetupLayout()
        {
            // Setup element binding when window size changes
            this.Resize += (s, e) =>
            {
                if (this.WindowState == FormWindowState.Minimized && settings.MinimizeToTray)
                {
                    this.Hide();
                    notifyIcon.Visible = true;
                    return;
                }

                var width = this.ClientSize.Width;
                var height = this.ClientSize.Height;

                // Adjust controls based on window size
                instancesListView.Size = new Size(width - 40, height - 280);
                statusLabel.Location = new Point(20, height - 40);
                statusLabel.Size = new Size(width - 40, 20);
            };
        }

        private void SetupEventHandlers()
        {
            createInstanceButton.Click += CreateInstanceButton_Click;
            restoreInstancesButton.Click += RestoreInstancesButton_Click;
            closeAllButton.Click += CloseAllButton_Click;
            instancesListView.DoubleClick += InstancesListView_DoubleClick;
            
            // Context menu for list
            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("👁️ Show Instance", null, ShowInstance_Click);
            contextMenu.Items.Add("🙈 Hide Instance", null, HideInstance_Click);
            contextMenu.Items.Add("📊 Open DevTools", null, OpenDevTools_Click);
            contextMenu.Items.Add("-");
            contextMenu.Items.Add("❌ Close Instance", null, CloseInstance_Click);
            instancesListView.ContextMenuStrip = contextMenu;

            // Handle application closing
            this.FormClosing += MainForm_FormClosing;
        }

        private void SetupSystemTray()
        {
            notifyIcon = new NotifyIcon
            {
                Text = "Multi Telegram Manager",
                Visible = false
            };

            // Create system tray context menu
            var trayMenu = new ContextMenuStrip();
            trayMenu.Items.Add("Open Manager", null, (s, e) => {
                this.Show();
                this.WindowState = FormWindowState.Normal;
                notifyIcon.Visible = false;
            });
            trayMenu.Items.Add("Create New Instance", null, (s, e) => CreateInstanceButton_Click(null, null));
            trayMenu.Items.Add("-");
            trayMenu.Items.Add("Exit", null, (s, e) => Application.Exit());

            notifyIcon.ContextMenuStrip = trayMenu;
            notifyIcon.DoubleClick += (s, e) => {
                this.Show();
                this.WindowState = FormWindowState.Normal;
                notifyIcon.Visible = false;
            };
        }

        private void SetupUpdateTimer()
        {
            updateTimer = new Timer
            {
                Interval = 2000 // Update every 2 seconds
            };
            updateTimer.Tick += UpdateTimer_Tick;
            updateTimer.Start();
        }

        private async void CreateInstanceButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (instances.Count >= settings.MaxInstances)
                {
                    MessageBox.Show($"Maximum number of instances ({settings.MaxInstances}) reached.\n" +
                        "Close some instances or increase the limit in settings.", 
                        "Limit Reached", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                statusLabel.Text = "🔄 Creating new Telegram instance...";
                statusLabel.ForeColor = Color.FromArgb(255, 193, 7);
                createInstanceButton.Enabled = false;

                var instance = new TelegramInstance();
                instance.OnInstanceClosed += Instance_OnInstanceClosed;
                
                instances.Add(instance);
                instance.Show();

                // Save instance info
                SettingsManager.SaveInstanceInfo(instances);

                UpdateInstancesList();
                statusLabel.Text = $"✅ New instance created: {instance.InstanceId.Substring(0, 8)}";
                statusLabel.ForeColor = Color.FromArgb(40, 167, 69);

                notifyIcon.ShowBalloonTip(3000, "Multi Telegram", 
                    $"New instance created: {instance.InstanceId.Substring(0, 8)}", 
                    ToolTipIcon.Info);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Instance creation error: {ex.Message}\n\n" +
                    "Make sure Microsoft Edge WebView2 is installed.", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                statusLabel.Text = "❌ Instance creation failed";
                statusLabel.ForeColor = Color.FromArgb(220, 53, 69);
            }
            finally
            {
                createInstanceButton.Enabled = true;
            }
        }

        private void RestoreInstancesButton_Click(object sender, EventArgs e)
        {
            try
            {
                var savedInstances = settings.SavedInstances;
                if (savedInstances.Count == 0)
                {
                    statusLabel.Text = "ℹ️ No saved instances found";
                    statusLabel.ForeColor = Color.FromArgb(102, 102, 102);
                    return;
                }

                var result = MessageBox.Show(
                    $"Found {savedInstances.Count} saved instances. Restore them?",
                    "Restore Instances",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    statusLabel.Text = "🔄 Restoring saved instances...";
                    statusLabel.ForeColor = Color.FromArgb(255, 193, 7);

                    foreach (var instanceInfo in savedInstances)
                    {
                        try
                        {
                            var instance = new TelegramInstance(instanceInfo);
                            instance.OnInstanceClosed += Instance_OnInstanceClosed;
                            instances.Add(instance);
                            
                            if (instanceInfo.IsVisible)
                            {
                                instance.Show();
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Error restoring instance {instanceInfo.InstanceId}: {ex.Message}");
                        }
                    }

                    UpdateInstancesList();
                    statusLabel.Text = $"✅ Restored {instances.Count} instances";
                    statusLabel.ForeColor = Color.FromArgb(40, 167, 69);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error restoring instances: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CloseAllButton_Click(object sender, EventArgs e)
        {
            if (instances.Count == 0) return;

            var result = MessageBox.Show(
                $"Are you sure you want to close all {instances.Count} instances?\n\n" +
                "Their data will be preserved for future restoration.",
                "Close All Instances",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                statusLabel.Text = "🔄 Closing all instances...";
                statusLabel.ForeColor = Color.FromArgb(255, 193, 7);

                var instancesToClose = instances.ToList();
                foreach (var instance in instancesToClose)
                {
                    instance.Close();
                }
                instances.Clear();
                UpdateInstancesList();
                statusLabel.Text = "✅ All instances closed";
                statusLabel.ForeColor = Color.FromArgb(40, 167, 69);
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
                    if (instance.InstanceForm.Visible)
                    {
                        instance.Hide();
                    }
                    else
                    {
                        instance.Show();
                    }
                    UpdateInstancesList();
                }
            }
        }

        private void ShowInstance_Click(object sender, EventArgs e)
        {
            if (instancesListView.SelectedItems.Count > 0)
            {
                var selectedIndex = instancesListView.SelectedItems[0].Index;
                instances[selectedIndex].Show();
                UpdateInstancesList();
            }
        }

        private void HideInstance_Click(object sender, EventArgs e)
        {
            if (instancesListView.SelectedItems.Count > 0)
            {
                var selectedIndex = instancesListView.SelectedItems[0].Index;
                instances[selectedIndex].Hide();
                UpdateInstancesList();
            }
        }

        private void OpenDevTools_Click(object sender, EventArgs e)
        {
            if (instancesListView.SelectedItems.Count > 0)
            {
                var selectedIndex = instancesListView.SelectedItems[0].Index;
                var instance = instances[selectedIndex];
                
                try
                {
                    if (instance.WebView?.CoreWebView2 != null)
                    {
                        instance.WebView.CoreWebView2.OpenDevToolsWindow();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error opening DevTools: {ex.Message}", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void CloseInstance_Click(object sender, EventArgs e)
        {
            if (instancesListView.SelectedItems.Count > 0)
            {
                var selectedIndex = instancesListView.SelectedItems[0].Index;
                var instance = instances[selectedIndex];
                
                var result = MessageBox.Show(
                    $"Close instance {instance.InstanceId.Substring(0, 8)}?\n\n" +
                    "The session data will be preserved.",
                    "Close Instance",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    instance.Close();
                }
            }
        }

        private void Instance_OnInstanceClosed(TelegramInstance instance)
        {
            instances.Remove(instance);
            SettingsManager.SaveInstanceInfo(instances);
            UpdateInstancesList();
            statusLabel.Text = $"ℹ️ Instance {instance.InstanceId.Substring(0, 8)} closed";
            statusLabel.ForeColor = Color.FromArgb(102, 102, 102);
        }

        private void UpdateInstancesList()
        {
            instancesListView.Items.Clear();

            for (int i = 0; i < instances.Count; i++)
            {
                var instance = instances[i];
                var sessionSize = GetSessionSize(instance.UserDataFolder);
                
                var item = new ListViewItem(new[]
                {
                    instance.InstanceId.Substring(0, 8),
                    instance.CreatedAt.ToString("MM/dd HH:mm"),
                    GetBrowserName(instance.UserAgent),
                    instance.InstanceForm.Visible ? "🟢 Active" : "🔴 Hidden",
                    $"{instance.InstanceForm.Location.X},{instance.InstanceForm.Location.Y}",
                    sessionSize,
                    "Right-click →"
                });

                // Color coding based on status
                if (instance.InstanceForm.Visible)
                {
                    item.ForeColor = Color.FromArgb(40, 167, 69);
                }
                else
                {
                    item.ForeColor = Color.FromArgb(108, 117, 125);
                }

                instancesListView.Items.Add(item);
            }

            // Update statistics
            totalInstancesLabel.Text = $"Active instances: {instances.Count}";
            var visibleCount = instances.Count(i => i.InstanceForm.Visible);
            memoryUsageLabel.Text = $"Estimated memory usage: ~{instances.Count * 120} MB ({visibleCount} visible)";
            
            // Update button states
            restoreInstancesButton.Enabled = settings.SavedInstances.Count > 0;
            closeAllButton.Enabled = instances.Count > 0;
        }

        private string GetBrowserName(string userAgent)
        {
            if (userAgent.Contains("Firefox")) return "Firefox";
            if (userAgent.Contains("Edge")) return "Edge";
            if (userAgent.Contains("Chrome")) return "Chrome";
            return "Unknown";
        }

        private string GetSessionSize(string userDataFolder)
        {
            try
            {
                if (!System.IO.Directory.Exists(userDataFolder))
                    return "0 MB";

                var dirInfo = new System.IO.DirectoryInfo(userDataFolder);
                var size = dirInfo.EnumerateFiles("*", System.IO.SearchOption.AllDirectories)
                    .Sum(file => file.Length);
                
                return $"{size / (1024 * 1024):F1} MB";
            }
            catch
            {
                return "N/A";
            }
        }

        private void UpdateTimer_Tick(object sender, EventArgs e)
        {
            // Update statistics and save instance info periodically
            if (instances.Count > 0)
            {
                SettingsManager.SaveInstanceInfo(instances);
                
                // Update memory usage
                var visibleCount = instances.Count(i => i.InstanceForm.Visible);
                memoryUsageLabel.Text = $"Estimated memory usage: ~{instances.Count * 120} MB ({visibleCount} visible)";
            }
        }

        private void RestoreSavedInstances()
        {
            if (settings.AutoStartInstances && settings.SavedInstances.Count > 0)
            {
                // Auto-restore instances on startup if enabled
                foreach (var instanceInfo in settings.SavedInstances.Take(5)) // Limit to 5 on startup
                {
                    try
                    {
                        var instance = new TelegramInstance(instanceInfo);
                        instance.OnInstanceClosed += Instance_OnInstanceClosed;
                        instances.Add(instance);
                        
                        // Don't show windows on startup - let user control this
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error auto-restoring instance: {ex.Message}");
                    }
                }
                
                if (instances.Count > 0)
                {
                    UpdateInstancesList();
                    statusLabel.Text = $"✅ Auto-restored {instances.Count} instances";
                    statusLabel.ForeColor = Color.FromArgb(40, 167, 69);
                }
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            updateTimer?.Stop();
            
            // Save current state
            SettingsManager.SaveInstanceInfo(instances);
            
            if (instances.Count > 0)
            {
                var result = MessageBox.Show(
                    $"You have {instances.Count} Telegram instances.\n\n" +
                    "✅ YES - Close all instances and exit\n" +
                    "❌ NO - Minimize to system tray\n" +
                    "⭕ CANCEL - Stay open",
                    "Exit Confirmation",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }

                if (result == DialogResult.No)
                {
                    e.Cancel = true;
                    this.Hide();
                    notifyIcon.Visible = true;
                    notifyIcon.ShowBalloonTip(3000, "Multi Telegram", 
                        "Application minimized to system tray", ToolTipIcon.Info);
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

            notifyIcon?.Dispose();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // Hotkeys
            if (keyData == (Keys.Control | Keys.N))
            {
                CreateInstanceButton_Click(null, null);
                return true;
            }

            if (keyData == (Keys.Control | Keys.R))
            {
                RestoreInstancesButton_Click(null, null);
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace MultiTelegram
{
    public class AppSettings
    {
        public List<InstanceInfo> SavedInstances { get; set; } = new List<InstanceInfo>();
        public bool AutoStartInstances { get; set; } = true;
        public bool MinimizeToTray { get; set; } = true;
        public int MaxInstances { get; set; } = 10;
    }

    public class InstanceInfo
    {
        public string InstanceId { get; set; }
        public string UserDataFolder { get; set; }
        public string UserAgent { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsVisible { get; set; } = true;
        public int WindowX { get; set; } = 100;
        public int WindowY { get; set; } = 100;
        public int WindowWidth { get; set; } = 420;
        public int WindowHeight { get; set; } = 700;
    }

    public static class SettingsManager
    {
        private static readonly string SettingsFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), 
            "MultiTelegram");
        
        private static readonly string SettingsFile = Path.Combine(SettingsFolder, "settings.json");

        static SettingsManager()
        {
            if (!Directory.Exists(SettingsFolder))
            {
                Directory.CreateDirectory(SettingsFolder);
            }
        }

        public static AppSettings LoadSettings()
        {
            try
            {
                if (File.Exists(SettingsFile))
                {
                    var json = File.ReadAllText(SettingsFile);
                    return JsonConvert.DeserializeObject<AppSettings>(json) ?? new AppSettings();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading settings: {ex.Message}");
            }
            
            return new AppSettings();
        }

        public static void SaveSettings(AppSettings settings)
        {
            try
            {
                var json = JsonConvert.SerializeObject(settings, Formatting.Indented);
                File.WriteAllText(SettingsFile, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving settings: {ex.Message}");
            }
        }

        public static void SaveInstanceInfo(List<TelegramInstance> instances)
        {
            try
            {
                var settings = LoadSettings();
                settings.SavedInstances.Clear();

                foreach (var instance in instances)
                {
                    settings.SavedInstances.Add(new InstanceInfo
                    {
                        InstanceId = instance.InstanceId,
                        UserDataFolder = instance.UserDataFolder,
                        UserAgent = instance.UserAgent,
                        CreatedAt = instance.CreatedAt,
                        IsVisible = instance.InstanceForm.Visible,
                        WindowX = instance.InstanceForm.Location.X,
                        WindowY = instance.InstanceForm.Location.Y,
                        WindowWidth = instance.InstanceForm.Size.Width,
                        WindowHeight = instance.InstanceForm.Size.Height
                    });
                }

                SaveSettings(settings);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving instances: {ex.Message}");
            }
        }
    }
}
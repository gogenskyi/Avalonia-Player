using System;
using System.IO;
using System.Text.Json;

namespace PlayerAvalonia.Helpers
{
    public static class SettingsHelper
    {
        private static readonly string SettingsFilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "PlayerApp",
        "settings.json"
    );
        private static AppSettings _settings;

        static SettingsHelper()
        {
            Load();
        }
        private static void Load()
        {
            try
            {
                if (File.Exists(SettingsFilePath))
                {
                    var json = File.ReadAllText(SettingsFilePath);
                    _settings = JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
                }
                else
                {
                    _settings = new AppSettings();
                }
            }
            catch
            {
                _settings = new AppSettings(); // у разі помилки — створити порожні налаштування
            }
        }

        private static void Save()
        {
            try
            {
                var dir = Path.GetDirectoryName(SettingsFilePath);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                var json = JsonSerializer.Serialize(_settings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(SettingsFilePath, json);
            }
            catch
            {
            }
        }

        public static void SaveLastFolder(string path)
        {
            _settings.LastFolder = path;
            Save();
        }

        public static string LoadLastFolder()
        {
            return _settings.LastFolder;
        }
    
}

}

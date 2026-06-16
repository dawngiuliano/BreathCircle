using System.IO;
using BreathCircle.Helpers;
using BreathCircle.Models;

namespace BreathCircle.Services
{
    /// <summary>
    /// 设置服务 — 管理 settings.json 的读写
    /// </summary>
    public class SettingsService
    {
        private readonly string _settingsFilePath;
        private UserSettings _settings;

        /// <summary>当前设置实例</summary>
        public UserSettings Settings => _settings;

        public SettingsService()
        {
            string configDir = Path.Combine(AppContext.BaseDirectory, "Config");
            Directory.CreateDirectory(configDir);
            _settingsFilePath = Path.Combine(configDir, "settings.json");
            _settings = LoadSettings();
        }

        /// <summary>
        /// 加载设置，文件不存在则创建默认设置
        /// </summary>
        private UserSettings LoadSettings()
        {
            var loaded = JsonHelper.LoadFromFile<UserSettings>(_settingsFilePath);
            if (loaded != null)
                return loaded;

            // 创建默认设置
            var defaults = new UserSettings();
            SaveSettings(defaults);
            return defaults;
        }

        /// <summary>
        /// 保存设置到文件
        /// </summary>
        public void SaveSettings(UserSettings? settings = null)
        {
            if (settings != null)
                _settings = settings;
            JsonHelper.SaveToFile(_settingsFilePath, _settings);
        }

        /// <summary>
        /// 更新当前模式并保存
        /// </summary>
        public void SetCurrentMode(BreathMode mode)
        {
            _settings.CurrentMode = mode;
            SaveSettings();
        }

        /// <summary>
        /// 更新指定设置项并保存
        /// </summary>
        public void UpdateSetting(Action<UserSettings> update)
        {
            update(_settings);
            SaveSettings();
        }
    }
}

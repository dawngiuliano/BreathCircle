using System.IO;
using System.Text.Json;

namespace BreathCircle.Helpers
{
    /// <summary>
    /// JSON 序列化/反序列化辅助类
    /// </summary>
    public static class JsonHelper
    {
        private static readonly JsonSerializerOptions DefaultOptions = new()
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true,
        };

        /// <summary>
        /// 将对象序列化到文件
        /// </summary>
        public static void SaveToFile<T>(string filePath, T obj)
        {
            var json = JsonSerializer.Serialize(obj, DefaultOptions);
            File.WriteAllText(filePath, json);
        }

        /// <summary>
        /// 从文件反序列化对象，文件不存在则返回默认值
        /// </summary>
        public static T? LoadFromFile<T>(string filePath) where T : class
        {
            if (!File.Exists(filePath))
                return null;

            var json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<T>(json, DefaultOptions);
        }
    }
}

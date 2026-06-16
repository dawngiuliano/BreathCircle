using System.Text.Json.Serialization;

namespace BreathCircle.Models
{
    /// <summary>
    /// 用户设置
    /// </summary>
    public class UserSettings
    {
        /// <summary>当前呼吸模式</summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public BreathMode CurrentMode { get; set; } = BreathMode.BoxBreathing;

        /// <summary>是否置顶</summary>
        public bool IsTopMost { get; set; } = true;

        /// <summary>是否显示阶段文字</summary>
        public bool ShowStageText { get; set; } = true;

        /// <summary>是否显示倒计时</summary>
        public bool ShowCountdown { get; set; } = true;

        /// <summary>是否启用点击穿透</summary>
        public bool ClickThrough { get; set; } = false;

        /// <summary>是否跳过安全对话框（Wim Hof）</summary>
        public bool SkipSafetyDialog { get; set; } = false;

        /// <summary>窗口X坐标</summary>
        public double WindowX { get; set; } = -1;

        /// <summary>窗口Y坐标</summary>
        public double WindowY { get; set; } = -1;

        /// <summary>Wim Hof 可调参数</summary>
        public WimHofSettings WimHof { get; set; } = new();

        /// <summary>自定义颜色（十六进制 RRGGBB），空则用默认</summary>
        public Dictionary<string, string> CustomColors { get; set; } = new();
    }
}

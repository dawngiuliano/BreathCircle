using System.Windows.Media;
using BreathCircle.Models;
using MediaColor = System.Windows.Media.Color;

namespace BreathCircle.Helpers
{
    /// <summary>
    /// 色彩主题 — 柔和精致的默认配色，支持自定义覆盖
    /// </summary>
    public static class ColorThemes
    {
        /// <summary>自定义颜色覆盖表（由设置加载）</summary>
        public static Dictionary<string, string> CustomColors { get; set; } = new();

        public static MediaColor DefaultColor(BreathMode mode) => mode switch
        {
            BreathMode.BoxBreathing => MediaColor.FromRgb(0x6B, 0xA3, 0xD6),
            BreathMode.FourSevenEight => MediaColor.FromRgb(0xA0, 0x8A, 0xD6),
            BreathMode.Resonance => MediaColor.FromRgb(0x5E, 0xBA, 0xB2),
            BreathMode.PhysiologicalSigh => MediaColor.FromRgb(0xE6, 0xAA, 0x6E),
            BreathMode.WimHof => MediaColor.FromRgb(0xEE, 0x7B, 0x52),
            _ => MediaColor.FromRgb(0x6B, 0xA3, 0xD6),
        };

        /// <summary>获取模式颜色（优先自定义）</summary>
        public static MediaColor GetBaseColor(BreathMode mode)
        {
            string key = mode.ToString();
            if (CustomColors.TryGetValue(key, out var hex) && TryParseHex(hex, out var c))
                return c;
            return DefaultColor(mode);
        }

        /// <summary>解析 RRGGBB 十六进制颜色</summary>
        public static bool TryParseHex(string hex, out MediaColor color)
        {
            color = default;
            if (string.IsNullOrWhiteSpace(hex) || hex.Length != 6) return false;
            try
            {
                color = MediaColor.FromRgb(
                    Convert.ToByte(hex.Substring(0, 2), 16),
                    Convert.ToByte(hex.Substring(2, 2), 16),
                    Convert.ToByte(hex.Substring(4, 2), 16));
                return true;
            }
            catch { return false; }
        }

        /// <summary>Color → RRGGBB 十六进制</summary>
        public static string ToHex(MediaColor c) => $"{c.R:X2}{c.G:X2}{c.B:X2}";

        public static MediaColor GetGlowColor(BreathMode mode)
        {
            MediaColor c = GetBaseColor(mode);
            return MediaColor.FromRgb(
                (byte)Math.Min(255, c.R + 40),
                (byte)Math.Min(255, c.G + 40),
                (byte)Math.Min(255, c.B + 40));
        }

        /// <summary>吸气变亮 25%</summary>
        public static MediaColor AdjustForBreath(MediaColor baseColor, double progress, bool growing)
        {
            double factor = growing
                ? 1.0 + 0.25 * progress
                : 1.0 + 0.25 * (1 - progress);
            return MediaColor.FromRgb(
                (byte)Math.Min(255, (int)(baseColor.R * factor)),
                (byte)Math.Min(255, (int)(baseColor.G * factor)),
                (byte)Math.Min(255, (int)(baseColor.B * factor)));
        }

        /// <summary>玻璃拟态圆形画刷：实心半透明 + 高光</summary>
        public static RadialGradientBrush CreateGlassCircleBrush(MediaColor color)
        {
            var brush = new RadialGradientBrush
            {
                GradientOrigin = new System.Windows.Point(0.35, 0.30),
                Center = new System.Windows.Point(0.5, 0.5),
                RadiusX = 0.6,
                RadiusY = 0.6,
            };
            brush.GradientStops.Add(new GradientStop(MediaColor.FromArgb(0xF0, color.R, color.G, color.B), 0.0));
            brush.GradientStops.Add(new GradientStop(MediaColor.FromArgb(0xD0, color.R, color.G, color.B), 0.4));
            brush.GradientStops.Add(new GradientStop(MediaColor.FromArgb(0x90, color.R, color.G, color.B), 0.8));
            brush.GradientStops.Add(new GradientStop(MediaColor.FromArgb(0x40, color.R, color.G, color.B), 1.0));
            return brush;
        }

        /// <summary>发光层画刷</summary>
        public static RadialGradientBrush CreateGlowBrush(MediaColor color)
        {
            var brush = new RadialGradientBrush
            {
                GradientOrigin = new System.Windows.Point(0.5, 0.5),
                Center = new System.Windows.Point(0.5, 0.5),
                RadiusX = 0.5,
                RadiusY = 0.5,
            };
            brush.GradientStops.Add(new GradientStop(MediaColor.FromArgb(0x50, color.R, color.G, color.B), 0.0));
            brush.GradientStops.Add(new GradientStop(MediaColor.FromArgb(0x10, color.R, color.G, color.B), 0.5));
            brush.GradientStops.Add(new GradientStop(MediaColor.FromArgb(0x00, color.R, color.G, color.B), 1.0));
            return brush;
        }
    }
}

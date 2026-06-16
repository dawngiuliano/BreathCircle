using System;
using System.Drawing;
using System.IO;
using System.Reflection;

namespace BreathCircle.Helpers
{
    /// <summary>
    /// 应用图标 — 优先从嵌入资源加载，失败则运行时生成
    /// </summary>
    public static class IconGenerator
    {
        private static Icon? _cachedIcon;

        /// <summary>
        /// 获取应用图标（System.Drawing.Icon，用于托盘）
        /// </summary>
        public static Icon GetIcon()
        {
            if (_cachedIcon != null) return _cachedIcon;

            // 优先从嵌入资源加载
            try
            {
                using var stream = Assembly.GetExecutingAssembly()
                    .GetManifestResourceStream("BreathCircle.Assets.BreathCircle.ico");
                if (stream != null)
                {
                    _cachedIcon = new Icon(stream);
                    return _cachedIcon;
                }
            }
            catch { }

            // 回退：运行时生成
            _cachedIcon = GenerateIcon();
            return _cachedIcon;
        }

        /// <summary>
        /// 确保图标文件存在并返回路径（用于 WPF Window.Icon）
        /// </summary>
        public static string GetIconFilePath()
        {
            // 单文件发布时嵌入资源在内存中，需要先解出到临时文件
            string tempPath = Path.Combine(Path.GetTempPath(), "BreathCircle_AppIcon.ico");
            try
            {
                if (!File.Exists(tempPath))
                {
                    using var icon = GetIcon();
                    using var fs = new FileStream(tempPath, FileMode.Create);
                    icon.Save(fs);
                }
            }
            catch { }
            return tempPath;
        }

        private static Icon GenerateIcon()
        {
            using var bitmap = new Bitmap(32, 32);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);

                using var outerPen = new Pen(Color.FromArgb(90, 180, 210), 1.8f)
                { Alignment = System.Drawing.Drawing2D.PenAlignment.Center };
                g.DrawEllipse(outerPen, 2, 2, 28, 28);

                using var innerBrush = new System.Drawing.Drawing2D.LinearGradientBrush(
                    new Point(8, 4), new Point(24, 28),
                    Color.FromArgb(160, 200, 220), Color.FromArgb(80, 140, 180));
                g.FillEllipse(innerBrush, 5, 5, 22, 22);

                using var highlightBrush = new SolidBrush(Color.FromArgb(120, 255, 255, 255));
                g.FillEllipse(highlightBrush, 10, 7, 8, 6);
            }

            IntPtr hIcon = bitmap.GetHicon();
            return (Icon)Icon.FromHandle(hIcon).Clone();
        }
    }
}

using System.Runtime.InteropServices;

namespace BreathCircle.Helpers
{
    /// <summary>
    /// Win32 API 辅助类 — 热键注册、窗口透明穿透、窗口扩展样式
    /// </summary>
    public static class Win32Helper
    {
        // 窗口扩展样式常量
        public const int WS_EX_TRANSPARENT = 0x00000020;
        public const int WS_EX_TOOLWINDOW = 0x00000080;
        public const int WS_EX_APPWINDOW = 0x00040000;

        // 窗口样式常量
        public const int GWL_EXSTYLE = -20;

        // 热键相关常量
        public const int WM_HOTKEY = 0x0312;
        public const int MOD_CONTROL = 0x0002;
        public const int MOD_ALT = 0x0001;

        [DllImport("user32.dll")]
        public static extern int GetWindowLong(IntPtr hwnd, int index);

        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);

        [DllImport("user32.dll")]
        public static extern bool RegisterHotKey(IntPtr hwnd, int id, int fsModifiers, int vk);

        [DllImport("user32.dll")]
        public static extern bool UnregisterHotKey(IntPtr hwnd, int id);

        /// <summary>
        /// 启用点击穿透（鼠标事件透传）
        /// </summary>
        public static void EnableClickThrough(IntPtr hwnd)
        {
            int exStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
            SetWindowLong(hwnd, GWL_EXSTYLE, exStyle | WS_EX_TRANSPARENT);
        }

        /// <summary>
        /// 禁用点击穿透
        /// </summary>
        public static void DisableClickThrough(IntPtr hwnd)
        {
            int exStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
            SetWindowLong(hwnd, GWL_EXSTYLE, exStyle & ~WS_EX_TRANSPARENT);
        }

        /// <summary>
        /// 判断当前是否启用了点击穿透
        /// </summary>
        public static bool IsClickThroughEnabled(IntPtr hwnd)
        {
            int exStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
            return (exStyle & WS_EX_TRANSPARENT) != 0;
        }

        /// <summary>
        /// 确保窗口显示在任务栏（修正工具窗口标志）
        /// </summary>
        public static void EnsureAppWindow(IntPtr hwnd)
        {
            int exStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
            SetWindowLong(hwnd, GWL_EXSTYLE, (exStyle | WS_EX_APPWINDOW) & ~WS_EX_TOOLWINDOW);
        }
    }
}

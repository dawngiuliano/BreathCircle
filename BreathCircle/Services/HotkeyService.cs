using System.Runtime.InteropServices;
using System.Windows.Interop;
using BreathCircle.Helpers;

namespace BreathCircle.Services
{
    /// <summary>
    /// 全局热键服务 — 注册系统级快捷键
    /// </summary>
    public class HotkeyService : IDisposable
    {
        private readonly HwndSource _hwndSource;
        private readonly Dictionary<int, Action> _hotkeyActions = new();

        // 热键ID常量
        public const int HOTKEY_START_STOP = 1;
        public const int HOTKEY_NEXT_MODE = 2;
        public const int HOTKEY_WIM_HOF = 3;
        public const int HOTKEY_HIDE_SHOW = 4;
        public const int HOTKEY_EXIT = 5;

        // 虚拟键码
        private const int VK_R = 0x52;
        private const int VK_N = 0x4E;
        private const int VK_W = 0x57;
        private const int VK_H = 0x48;
        private const int VK_Q = 0x51;

        // 事件
        public event Action? StartStopRequested;
        public event Action? NextModeRequested;
        public event Action? WimHofRequested;
        public event Action? HideShowRequested;
        public event Action? ExitRequested;

        public HotkeyService(IntPtr windowHandle)
        {
            _hwndSource = HwndSource.FromHwnd(windowHandle)!;
            _hwndSource.AddHook(WndProc);
        }

        /// <summary>
        /// 注册所有默认热键
        /// </summary>
        public void RegisterAll()
        {
            RegisterHotkey(HOTKEY_START_STOP, Win32Helper.MOD_CONTROL | Win32Helper.MOD_ALT, VK_R,
                () => StartStopRequested?.Invoke());
            RegisterHotkey(HOTKEY_NEXT_MODE, Win32Helper.MOD_CONTROL | Win32Helper.MOD_ALT, VK_N,
                () => NextModeRequested?.Invoke());
            RegisterHotkey(HOTKEY_WIM_HOF, Win32Helper.MOD_CONTROL | Win32Helper.MOD_ALT, VK_W,
                () => WimHofRequested?.Invoke());
            RegisterHotkey(HOTKEY_HIDE_SHOW, Win32Helper.MOD_CONTROL | Win32Helper.MOD_ALT, VK_H,
                () => HideShowRequested?.Invoke());
            RegisterHotkey(HOTKEY_EXIT, Win32Helper.MOD_CONTROL | Win32Helper.MOD_ALT, VK_Q,
                () => ExitRequested?.Invoke());
        }

        /// <summary>
        /// 注销所有热键
        /// </summary>
        public void UnregisterAll()
        {
            foreach (int id in _hotkeyActions.Keys.ToList())
            {
                Win32Helper.UnregisterHotKey(_hwndSource.Handle, id);
            }
            _hotkeyActions.Clear();
        }

        private void RegisterHotkey(int id, int modifiers, int vk, Action action)
        {
            bool success = Win32Helper.RegisterHotKey(_hwndSource.Handle, id, modifiers, vk);
            if (success)
            {
                _hotkeyActions[id] = action;
            }
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == Win32Helper.WM_HOTKEY)
            {
                int hotkeyId = wParam.ToInt32();
                if (_hotkeyActions.TryGetValue(hotkeyId, out var action))
                {
                    action.Invoke();
                    handled = true;
                }
            }
            return IntPtr.Zero;
        }

        public void Dispose()
        {
            UnregisterAll();
            _hwndSource.RemoveHook(WndProc);
        }
    }
}

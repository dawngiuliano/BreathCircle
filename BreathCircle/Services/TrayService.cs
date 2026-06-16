using System.Windows;
using System.Windows.Controls;
using BreathCircle.Helpers;

namespace BreathCircle.Services
{
    /// <summary>
    /// 系统托盘服务 — 管理托盘图标和右键菜单
    /// </summary>
    public class TrayService : IDisposable
    {
        private System.Windows.Forms.NotifyIcon? _notifyIcon;
        private System.Windows.Forms.ContextMenuStrip? _contextMenu;
        private readonly Window _window;

        // 菜单项
        private System.Windows.Forms.ToolStripMenuItem? _modeSubMenu;
        private System.Windows.Forms.ToolStripMenuItem? _boxModeItem;
        private System.Windows.Forms.ToolStripMenuItem? _fourSevenEightItem;
        private System.Windows.Forms.ToolStripMenuItem? _resonanceItem;
        private System.Windows.Forms.ToolStripMenuItem? _physiologicalSighItem;
        private System.Windows.Forms.ToolStripMenuItem? _wimHofItem;

        // 回调委托
        public event Action? ShowWindowRequested;
        public event Action? HideWindowRequested;
        public event Action? ExitRequested;
        public event Action<Models.BreathMode>? ModeChangeRequested;
        public event Action<bool>? TopMostChanged;
        public event Action<bool>? ClickThroughChanged;
        public event Action<bool>? StageTextToggled;
        public event Action? WimHofSettingsRequested;
        public event Action? ColorSettingsRequested;
        public event Action<bool>? CountdownToggled;

        public TrayService(Window window)
        {
            _window = window;
        }

        /// <summary>
        /// 初始化托盘图标和菜单
        /// </summary>
        public void Initialize()
        {
            CreateContextMenu();

            // 创建托盘图标
            _notifyIcon = new System.Windows.Forms.NotifyIcon
            {
                Text = "BreathCircle - 呼吸圆圈",
                ContextMenuStrip = _contextMenu,
                Visible = true,
            };

            _notifyIcon.Icon = IconGenerator.GetIcon();
            _notifyIcon.DoubleClick += (_, _) => ShowWindowRequested?.Invoke();
        }

        /// <summary>
        /// 更新托盘提示文字
        /// </summary>
        public void UpdateToolTip(string text)
        {
            if (_notifyIcon != null)
                _notifyIcon.Text = text;
        }

        /// <summary>
        /// 更新模式选中状态
        /// </summary>
        public void UpdateModeCheckState(Models.BreathMode currentMode)
        {
            if (_boxModeItem != null) _boxModeItem.Checked = currentMode == Models.BreathMode.BoxBreathing;
            if (_fourSevenEightItem != null) _fourSevenEightItem.Checked = currentMode == Models.BreathMode.FourSevenEight;
            if (_resonanceItem != null) _resonanceItem.Checked = currentMode == Models.BreathMode.Resonance;
            if (_physiologicalSighItem != null) _physiologicalSighItem.Checked = currentMode == Models.BreathMode.PhysiologicalSigh;
            if (_wimHofItem != null) _wimHofItem.Checked = currentMode == Models.BreathMode.WimHof;
        }

        private void CreateContextMenu()
        {
            _contextMenu = new System.Windows.Forms.ContextMenuStrip();

            // 显示/隐藏
            var showHideItem = new System.Windows.Forms.ToolStripMenuItem("显示/隐藏窗口");
            showHideItem.Click += (_, _) =>
            {
                if (_window.Visibility == Visibility.Visible)
                    HideWindowRequested?.Invoke();
                else
                    ShowWindowRequested?.Invoke();
            };
            _contextMenu.Items.Add(showHideItem);

            _contextMenu.Items.Add(new System.Windows.Forms.ToolStripSeparator());

            // 模式切换子菜单
            _modeSubMenu = new System.Windows.Forms.ToolStripMenuItem("切换模式");
            _boxModeItem = CreateModeItem("盒式呼吸", Models.BreathMode.BoxBreathing);
            _fourSevenEightItem = CreateModeItem("4-7-8 呼吸", Models.BreathMode.FourSevenEight);
            _resonanceItem = CreateModeItem("共鸣呼吸", Models.BreathMode.Resonance);
            _physiologicalSighItem = CreateModeItem("生理性叹息", Models.BreathMode.PhysiologicalSigh);
            _wimHofItem = CreateModeItem("Wim Hof", Models.BreathMode.WimHof);
            _modeSubMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[]
            {
                _boxModeItem, _fourSevenEightItem, _resonanceItem, _physiologicalSighItem, _wimHofItem
            });
            _contextMenu.Items.Add(_modeSubMenu);

            // Wim Hof 设置
            var wimHofSettingsItem = new System.Windows.Forms.ToolStripMenuItem("Wim Hof 设置");
            wimHofSettingsItem.Click += (_, _) => WimHofSettingsRequested?.Invoke();
            _contextMenu.Items.Add(wimHofSettingsItem);

            // 颜色设置
            var colorSettingsItem = new System.Windows.Forms.ToolStripMenuItem("颜色设置");
            colorSettingsItem.Click += (_, _) => ColorSettingsRequested?.Invoke();
            _contextMenu.Items.Add(colorSettingsItem);

            _contextMenu.Items.Add(new System.Windows.Forms.ToolStripSeparator());

            // 显示选项
            var stageTextItem = new System.Windows.Forms.ToolStripMenuItem("显示阶段文字");
            stageTextItem.CheckOnClick = true;
            stageTextItem.Checked = true;
            stageTextItem.CheckedChanged += (s, _) =>
            {
                if (s is System.Windows.Forms.ToolStripMenuItem item)
                    StageTextToggled?.Invoke(item.Checked);
            };
            _contextMenu.Items.Add(stageTextItem);

            var countdownItem = new System.Windows.Forms.ToolStripMenuItem("显示倒计时");
            countdownItem.CheckOnClick = true;
            countdownItem.Checked = true;
            countdownItem.CheckedChanged += (s, _) =>
            {
                if (s is System.Windows.Forms.ToolStripMenuItem item)
                    CountdownToggled?.Invoke(item.Checked);
            };
            _contextMenu.Items.Add(countdownItem);

            var topMostItem = new System.Windows.Forms.ToolStripMenuItem("置顶");
            topMostItem.CheckOnClick = true;
            topMostItem.Checked = true;
            topMostItem.CheckedChanged += (s, _) =>
            {
                if (s is System.Windows.Forms.ToolStripMenuItem item)
                    TopMostChanged?.Invoke(item.Checked);
            };
            _contextMenu.Items.Add(topMostItem);

            var clickThroughItem = new System.Windows.Forms.ToolStripMenuItem("点击穿透");
            clickThroughItem.CheckOnClick = true;
            clickThroughItem.Checked = false;
            clickThroughItem.CheckedChanged += (s, _) =>
            {
                if (s is System.Windows.Forms.ToolStripMenuItem item)
                    ClickThroughChanged?.Invoke(item.Checked);
            };
            _contextMenu.Items.Add(clickThroughItem);

            _contextMenu.Items.Add(new System.Windows.Forms.ToolStripSeparator());

            // 退出
            var exitItem = new System.Windows.Forms.ToolStripMenuItem("退出");
            exitItem.Click += (_, _) => ExitRequested?.Invoke();
            _contextMenu.Items.Add(exitItem);
        }

        private System.Windows.Forms.ToolStripMenuItem CreateModeItem(string text, Models.BreathMode mode)
        {
            var item = new System.Windows.Forms.ToolStripMenuItem(text);
            item.Click += (_, _) => ModeChangeRequested?.Invoke(mode);
            return item;
        }

        public void Dispose()
        {
            _notifyIcon?.Dispose();
            _contextMenu?.Dispose();
        }

    }
}

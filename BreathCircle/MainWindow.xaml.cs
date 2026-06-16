using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using BreathCircle.Helpers;
using MediaColor = System.Windows.Media.Color;
using BreathCircle.Models;
using BreathCircle.Services;
using BreathCircle.ViewModels;

namespace BreathCircle
{
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel;
        private readonly BreathingService _breathing;
        private readonly SettingsService _settings;
        private readonly TrayService _tray;
        private HotkeyService? _hotkey;
        private bool _isClosing;

        // 拖拽/点击区分
        private System.Windows.Point _mouseDownPos;
        private bool _isDragging;

        // 颜色画刷
        private RadialGradientBrush _circleBrush = new();
        private RadialGradientBrush _glowBrush = new();
        private readonly SolidColorBrush _ringBrush = new SolidColorBrush();

        public MainWindow()
        {
            InitializeComponent();

            // 创建服务
            var animService = new AnimationService();
            _settings = new SettingsService();
            _breathing = new BreathingService(animService);

            // 加载自定义颜色
            ColorThemes.CustomColors = _settings.Settings.CustomColors;

            // 加载 Wim Hof 设置
            _breathing.WimHofSettings = _settings.Settings.WimHof;

            // 创建 ViewModel
            _viewModel = new MainViewModel(_breathing, _settings);
            _viewModel.SetWindow(this);
            DataContext = _viewModel;

            // 创建托盘服务
            _tray = new TrayService(this);
            _tray.ShowWindowRequested += ShowWindow;
            _tray.ExitRequested += ExitApp;
            _tray.ModeChangeRequested += OnModeChanged;
            _tray.TopMostChanged += OnTopMostChanged;
            _tray.ClickThroughChanged += OnClickThroughChanged;
            _tray.StageTextToggled += OnStageTextToggled;
            _tray.CountdownToggled += OnCountdownToggled;
            _tray.WimHofSettingsRequested += () => Dispatcher.Invoke(() =>
            {
                var dlg = new Views.Dialogs.WimHofSettingsDialog(_settings.Settings.WimHof) { Owner = this };
                if (dlg.ShowDialog() == true)
                {
                    _settings.Settings.WimHof = dlg.Result;
                    _settings.SaveSettings();
                    _breathing.WimHofSettings = dlg.Result;
                }
            });
            _tray.ColorSettingsRequested += () => Dispatcher.Invoke(() =>
            {
                var dlg = new Views.Dialogs.ColorSettingsDialog(_settings.Settings.CustomColors) { Owner = this };
                if (dlg.ShowDialog() == true)
                {
                    _settings.Settings.CustomColors = dlg.Result;
                    _settings.SaveSettings();
                    ColorThemes.CustomColors = dlg.Result;
                    UpdateColorsForMode(_breathing.CurrentMode, 0, true);
                }
            });
            _tray.Initialize();

            // 恢复窗口位置
            double savedX = _settings.Settings.WindowX;
            double savedY = _settings.Settings.WindowY;
            if (savedX >= 0 && savedY >= 0)
            {
                var screen = System.Windows.Forms.Screen.PrimaryScreen;
                if (screen != null && savedX < screen.Bounds.Width && savedY < screen.Bounds.Height)
                {
                    Left = savedX;
                    Top = savedY;
                }
            }

            // 恢复模式
            _breathing.SetMode(_settings.Settings.CurrentMode);
            _viewModel.UpdateModeFromService();

            // 应用穿透设置
            if (_settings.Settings.ClickThrough)
            {
                var handle = new WindowInteropHelper(this).Handle;
                Win32Helper.EnableClickThrough(handle);
            }

            // 用动态画刷替换静态画刷
            CircleLayer.Fill = _circleBrush;
            GlowLayer.Fill = _glowBrush;
            RingLayer.Stroke = _ringBrush;

            // 初始颜色 & 菜单状态
            UpdateColorsForMode(_breathing.CurrentMode, 0, true);
            UpdateModeCheckmarks();
            SyncMenuCheckStates();

            // 设置窗口图标（Alt+Tab 和左上角显示）
            SetWindowIcon();

            // 启动动画循环
            CompositionTarget.Rendering += OnRendering;
        }

        private void OnRendering(object? sender, EventArgs e)
        {
            if (_breathing.IsRunning)
                _breathing.Update();

            _viewModel.CircleScale = _breathing.CircleScale;
            _viewModel.GlowOpacity = _breathing.GlowOpacity;

            if (_breathing.IsRunning)
            {
                _viewModel.CenterText = _breathing.CenterText;
                _viewModel.RefreshDisplayText();
            }

            UpdateCircleColorForBreath();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            var handle = new WindowInteropHelper(this).Handle;
            _hotkey = new HotkeyService(handle);
            _hotkey.StartStopRequested += OnHotkeyStartStop;
            _hotkey.NextModeRequested += OnHotkeyNextMode;
            _hotkey.WimHofRequested += OnHotkeyWimHof;
            _hotkey.HideShowRequested += OnHotkeyHideShow;
            _hotkey.ExitRequested += OnHotkeyExit;
            _hotkey.RegisterAll();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (!_isClosing)
            {
                e.Cancel = true;
                Hide();
                return;
            }
            base.OnClosing(e);
        }

        protected override void OnClosed(EventArgs e)
        {
            _settings.UpdateSetting(s =>
            {
                s.WindowX = Left;
                s.WindowY = Top;
            });

            CompositionTarget.Rendering -= OnRendering;
            _hotkey?.Dispose();
            _tray.Dispose();

            base.OnClosed(e);
        }

        // ===== 鼠标交互 =====

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_settings.Settings.ClickThrough) return;
            _mouseDownPos = e.GetPosition(this);
            _isDragging = false;
            ((Grid)sender).CaptureMouse();
        }

        private void Window_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!((Grid)sender).IsMouseCaptured) return;
            System.Windows.Point current = e.GetPosition(this);
            if (!_isDragging &&
                (Math.Abs(current.X - _mouseDownPos.X) > 3 ||
                 Math.Abs(current.Y - _mouseDownPos.Y) > 3))
            {
                _isDragging = true;
            }
            if (_isDragging)
            {
                Left += current.X - _mouseDownPos.X;
                Top += current.Y - _mouseDownPos.Y;
            }
        }

        private void Window_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ((Grid)sender).ReleaseMouseCapture();
            if (!_isDragging)
                _breathing.Toggle();
        }

        // ===== 右键菜单 =====

        private void OnStartStopClick(object sender, RoutedEventArgs e) => _breathing.Toggle();

        private void OnStopClick(object sender, RoutedEventArgs e) => _breathing.Stop();

        private void OnSetModeClick(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem item && item.Tag is string modeName)
            {
                var mode = Enum.Parse<BreathMode>(modeName);
                _breathing.SetMode(mode);
                _settings.SetCurrentMode(mode);
                _viewModel.UpdateModeFromService();
                _tray.UpdateModeCheckState(mode);
                UpdateColorsForMode(mode, 0, true);
                UpdateModeCheckmarks();
            }
        }

        private void OnToggleSettingClick(object sender, RoutedEventArgs e)
        {
            if (sender is not MenuItem item || item.Tag is not string tag) return;

            switch (tag)
            {
                case "TopMost":
                    _viewModel.ToggleTopMostCommand.Execute(null);
                    break;
                case "ClickThrough":
                    _viewModel.ToggleClickThroughCommand.Execute(null);
                    break;
                case "StageText":
                    _viewModel.ToggleStageTextCommand.Execute(null);
                    break;
                case "Countdown":
                    _viewModel.ToggleCountdownCommand.Execute(null);
                    break;
            }
            SyncMenuCheckStates();
        }

        private void OnExitClick(object sender, RoutedEventArgs e) => ExitApp();

        private void OnColorSettingsClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new Views.Dialogs.ColorSettingsDialog(_settings.Settings.CustomColors)
                {
                    Owner = this
                };
                if (dialog.ShowDialog() == true)
                {
                    _settings.Settings.CustomColors = dialog.Result;
                    _settings.SaveSettings();
                    ColorThemes.CustomColors = dialog.Result;
                    UpdateColorsForMode(_breathing.CurrentMode, 0, true);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(this, $"颜色设置出错：{ex.Message}",
                    "错误", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private void OnWimHofSettingsClick(object sender, RoutedEventArgs e)
        {
            var dialog = new Views.Dialogs.WimHofSettingsDialog(_settings.Settings.WimHof)
            {
                Owner = this
            };
            if (dialog.ShowDialog() == true)
            {
                _settings.Settings.WimHof = dialog.Result;
                _settings.SaveSettings();
                _breathing.WimHofSettings = dialog.Result;
            }
        }

        // ===== 热键处理 =====

        private void OnHotkeyStartStop() => Dispatcher.Invoke(() => _breathing.Toggle());

        private void OnHotkeyNextMode()
        {
            Dispatcher.Invoke(() =>
            {
                _breathing.NextMode();
                _settings.SetCurrentMode(_breathing.CurrentMode);
                _viewModel.UpdateModeFromService();
                _tray.UpdateModeCheckState(_breathing.CurrentMode);
                UpdateColorsForMode(_breathing.CurrentMode, 0, true);
                UpdateModeCheckmarks();
            });
        }

        private void OnHotkeyWimHof()
        {
            Dispatcher.Invoke(() =>
            {
                if (!_settings.Settings.SkipSafetyDialog)
                {
                    var dialog = new Views.Dialogs.WimHofSafetyDialog
                    {
                        Owner = this,
                        WindowStartupLocation = WindowStartupLocation.CenterScreen
                    };
                    if (dialog.ShowDialog() != true) return;
                    if (dialog.DontShowAgain)
                        _settings.UpdateSetting(s => s.SkipSafetyDialog = true);
                }

                _breathing.SetMode(BreathMode.WimHof);
                _settings.SetCurrentMode(BreathMode.WimHof);
                _breathing.Start();
                _viewModel.UpdateModeFromService();
                _tray.UpdateModeCheckState(BreathMode.WimHof);
                UpdateColorsForMode(BreathMode.WimHof, 0, true);
                UpdateModeCheckmarks();
            });
        }

        private void OnHotkeyHideShow()
        {
            Dispatcher.Invoke(() =>
            {
                if (Visibility == Visibility.Visible) Hide(); else Show();
            });
        }

        private void OnHotkeyExit() => Dispatcher.Invoke(ExitApp);

        // ===== 托盘回调 =====

        private void ShowWindow()
        {
            Dispatcher.Invoke(() => { Show(); Activate(); });
        }

        private void ExitApp()
        {
            _isClosing = true;
            _settings.UpdateSetting(s => { s.WindowX = Left; s.WindowY = Top; });
            System.Windows.Application.Current.Shutdown();
        }

        private void OnModeChanged(BreathMode mode)
        {
            _breathing.SetMode(mode);
            _settings.SetCurrentMode(mode);
            _viewModel.UpdateModeFromService();
            UpdateColorsForMode(mode, 0, true);
            UpdateModeCheckmarks();
        }

        private void OnTopMostChanged(bool value)
        {
            Topmost = value;
            _settings.UpdateSetting(s => s.IsTopMost = value);
            _viewModel.IsTopMost = value;
        }

        private void OnClickThroughChanged(bool value)
        {
            var handle = new WindowInteropHelper(this).Handle;
            if (value) Win32Helper.EnableClickThrough(handle);
            else Win32Helper.DisableClickThrough(handle);
            _settings.UpdateSetting(s => s.ClickThrough = value);
            _viewModel.IsClickThrough = value;
        }

        private void OnStageTextToggled(bool value)
        {
            _settings.UpdateSetting(s => s.ShowStageText = value);
            _viewModel.ShowStageText = value;
        }

        private void OnCountdownToggled(bool value)
        {
            _settings.UpdateSetting(s => s.ShowCountdown = value);
            _viewModel.ShowCountdown = value;
        }

        /// <summary>
        /// 生成并设置窗口图标（Alt+Tab、左上角可见）
        /// </summary>
        private void SetWindowIcon()
        {
            try
            {
                string iconPath = IconGenerator.GetIconFilePath();
                if (!string.IsNullOrEmpty(iconPath) && System.IO.File.Exists(iconPath))
                {
                    Icon = new System.Windows.Media.Imaging.BitmapImage(
                        new Uri(iconPath, UriKind.Absolute));
                }
            }
            catch { }
        }

        // ===== 菜单状态同步 =====

        /// <summary>同步模式选择的对号</summary>
        private void UpdateModeCheckmarks()
        {
            var mode = _breathing.CurrentMode;
            ModeBox.IsChecked = mode == BreathMode.BoxBreathing;
            Mode478.IsChecked = mode == BreathMode.FourSevenEight;
            ModeResonance.IsChecked = mode == BreathMode.Resonance;
            ModeSigh.IsChecked = mode == BreathMode.PhysiologicalSigh;
            ModeWimHof.IsChecked = mode == BreathMode.WimHof;
        }

        /// <summary>同步设置菜单的勾选状态</summary>
        private void SyncMenuCheckStates()
        {
            MenuStageText.IsChecked = _viewModel.ShowStageText;
            MenuCountdown.IsChecked = _viewModel.ShowCountdown;
            MenuTopMost.IsChecked = _viewModel.IsTopMost;
            MenuClickThrough.IsChecked = _viewModel.IsClickThrough;
        }

        // ===== 颜色管理 =====

        private void UpdateColorsForMode(BreathMode mode, double progress, bool growing)
        {
            MediaColor baseColor = ColorThemes.GetBaseColor(mode);
            MediaColor glowColor = ColorThemes.GetGlowColor(mode);
            MediaColor adjusted = ColorThemes.AdjustForBreath(baseColor, progress, growing);

            _circleBrush = ColorThemes.CreateGlassCircleBrush(adjusted);
            CircleLayer.Fill = _circleBrush;
            _glowBrush = ColorThemes.CreateGlowBrush(glowColor);
            GlowLayer.Fill = _glowBrush;
            _ringBrush.Color = MediaColor.FromArgb(0x80,
                (byte)Math.Min(255, baseColor.R + 80),
                (byte)Math.Min(255, baseColor.G + 80),
                (byte)Math.Min(255, baseColor.B + 80));
        }

        private void UpdateCircleColorForBreath()
        {
            if (!_breathing.IsRunning) return;

            MediaColor baseColor = ColorThemes.GetBaseColor(_breathing.CurrentMode);
            double progress = 0.5;
            bool growing = true;

            switch (_breathing.CurrentStage)
            {
                case BreathStage.Inhale:
                    progress = _breathing.CircleScale;
                    growing = true;
                    break;
                case BreathStage.Exhale:
                    progress = 1.0 - _breathing.CircleScale;
                    growing = false;
                    break;
                case BreathStage.Hold:
                case BreathStage.WimHofRetention:
                case BreathStage.WimHofRecovery:
                    growing = true;
                    progress = 1.0;
                    break;
                case BreathStage.WimHofBreathing:
                    growing = _breathing.CenterText.Contains("吸气");
                    progress = growing ? _breathing.CircleScale : 1.0 - _breathing.CircleScale;
                    break;
            }

            var adjusted = ColorThemes.AdjustForBreath(baseColor, progress, growing);
            _circleBrush = ColorThemes.CreateGlassCircleBrush(adjusted);
            CircleLayer.Fill = _circleBrush;
            _ringBrush.Color = MediaColor.FromArgb(0x80,
                (byte)Math.Min(255, baseColor.R + 80),
                (byte)Math.Min(255, baseColor.G + 80),
                (byte)Math.Min(255, baseColor.B + 80));
        }
    }
}

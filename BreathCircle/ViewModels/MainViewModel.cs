using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BreathCircle.Models;
using BreathCircle.Services;
using System.Windows;

namespace BreathCircle.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly BreathingService _breathing;
        private readonly SettingsService _settings;
        private Window? _window;

        public MainViewModel(BreathingService breathing, SettingsService settings)
        {
            _breathing = breathing;
            _settings = settings;

            IsTopMost = settings.Settings.IsTopMost;
            IsClickThrough = settings.Settings.ClickThrough;
            ShowStageText = settings.Settings.ShowStageText;
            ShowCountdown = settings.Settings.ShowCountdown;

            _breathing.StateChanged += OnBreathingStateChanged;
            _breathing.ModeChanged += OnBreathingModeChanged;

            UpdateFromBreathingService();
        }

        public void SetWindow(Window window)
        {
            _window = window;
            if (_window != null)
            {
                _window.Topmost = IsTopMost;
                if (IsClickThrough)
                    Helpers.Win32Helper.EnableClickThrough(
                        new System.Windows.Interop.WindowInteropHelper(_window).Handle);
            }
        }

        // ===== 绑定属性 =====

        [ObservableProperty]
        private double _circleScale = 0.6;

        [ObservableProperty]
        private double _glowOpacity;

        [ObservableProperty]
        private string _centerText = "准备";

        /// <summary>最终显示的文字（受显示开关影响）</summary>
        [ObservableProperty]
        private string _displayCenterText = "准备";

        /// <summary>文字可见性</summary>
        [ObservableProperty]
        private Visibility _centerTextVisible = Visibility.Visible;

        [ObservableProperty]
        private string _modeDisplayName = "盒式呼吸";

        [ObservableProperty]
        private string _stageDisplayName = "空闲";

        [ObservableProperty]
        private bool _isRunning;

        [ObservableProperty]
        private bool _isTopMost = true;

        [ObservableProperty]
        private bool _isClickThrough;

        [ObservableProperty]
        private bool _showStageText = true;

        [ObservableProperty]
        private bool _showCountdown = true;

        [ObservableProperty]
        private bool _isWimHofMode;

        [ObservableProperty]
        private string _wimHofRoundDisplay = "";

        // 当 ShowStageText / ShowCountdown 变化时刷新显示
        partial void OnShowStageTextChanged(bool value) => RefreshDisplayText();
        partial void OnShowCountdownChanged(bool value) => RefreshDisplayText();

        // ===== 命令 =====

        [RelayCommand]
        private void ToggleStartStop() => _breathing.Toggle();

        [RelayCommand]
        private void NextMode()
        {
            _breathing.NextMode();
            _settings.SetCurrentMode(_breathing.CurrentMode);
            UpdateModeDisplay();
        }

        [RelayCommand]
        private void SetMode(string modeName)
        {
            if (Enum.TryParse<BreathMode>(modeName, out var mode))
            {
                _breathing.SetMode(mode);
                _settings.SetCurrentMode(mode);
                UpdateModeDisplay();
            }
        }

        [RelayCommand]
        private void StartWimHof()
        {
            _breathing.SetMode(BreathMode.WimHof);
            _settings.SetCurrentMode(BreathMode.WimHof);
            _breathing.Start();
            UpdateModeDisplay();
        }

        [RelayCommand]
        private void ShowHideWindow()
        {
            if (_window == null) return;
            if (_window.Visibility == Visibility.Visible)
                _window.Hide();
            else
                _window.Show();
        }

        [RelayCommand]
        private void ExitApplication()
        {
            if (_window != null)
            {
                _settings.UpdateSetting(s =>
                {
                    s.WindowX = _window.Left;
                    s.WindowY = _window.Top;
                });
            }
            System.Windows.Application.Current.Shutdown();
        }

        [RelayCommand]
        private void ToggleTopMost()
        {
            IsTopMost = !IsTopMost;
            if (_window != null)
                _window.Topmost = IsTopMost;
            _settings.UpdateSetting(s => s.IsTopMost = IsTopMost);
        }

        [RelayCommand]
        private void ToggleClickThrough()
        {
            IsClickThrough = !IsClickThrough;
            if (_window != null)
            {
                var handle = new System.Windows.Interop.WindowInteropHelper(_window).Handle;
                if (IsClickThrough)
                    Helpers.Win32Helper.EnableClickThrough(handle);
                else
                    Helpers.Win32Helper.DisableClickThrough(handle);
            }
            _settings.UpdateSetting(s => s.ClickThrough = IsClickThrough);
        }

        [RelayCommand]
        private void ToggleStageText()
        {
            ShowStageText = !ShowStageText;
            _settings.UpdateSetting(s => s.ShowStageText = ShowStageText);
        }

        [RelayCommand]
        private void ToggleCountdown()
        {
            ShowCountdown = !ShowCountdown;
            _settings.UpdateSetting(s => s.ShowCountdown = ShowCountdown);
        }

        // ===== 辅助方法 =====

        private void OnBreathingStateChanged()
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                UpdateFromBreathingService();
            });
        }

        private void OnBreathingModeChanged()
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                UpdateModeDisplay();
            });
        }

        private void UpdateFromBreathingService()
        {
            CircleScale = _breathing.CircleScale;
            GlowOpacity = _breathing.GlowOpacity;
            IsRunning = _breathing.IsRunning;
            IsWimHofMode = _breathing.CurrentMode == BreathMode.WimHof;

            UpdateStageDisplay();
            UpdateModeDisplay();

            if (!IsRunning && !_breathing.IsPaused)
                CenterText = $"{ModeDisplayName}\n点击开始";
            else
                CenterText = _breathing.CenterText; // 暂停或运行

            RefreshDisplayText();

            if (_breathing.CurrentMode == BreathMode.WimHof)
            {
                WimHofRoundDisplay = $"第 {_breathing.WimHofCurrentRoundDisplay}/{_breathing.WimHofSettings.Rounds} 轮";
            }
            else
            {
                WimHofRoundDisplay = "";
            }
        }

        /// <summary>
        /// 根据显示开关刷新最终显示文字。
        /// 阶段文字和倒计时独立控制：
        ///   阶段ON  + 倒计时ON  → "屏息 4s"
        ///   阶段ON  + 倒计时OFF → "屏息"
        ///   阶段OFF + 倒计时ON  → "4s"
        ///   阶段OFF + 倒计时OFF → ""
        /// </summary>
        public void RefreshDisplayText()
        {
            string raw = CenterText;

            if (!ShowStageText && !ShowCountdown)
            {
                DisplayCenterText = "";
                CenterTextVisible = Visibility.Collapsed;
                return;
            }

            CenterTextVisible = Visibility.Visible;

            if (ShowStageText && ShowCountdown)
            {
                // 全部显示
                DisplayCenterText = raw;
            }
            else if (ShowStageText && !ShowCountdown)
            {
                // 只显示阶段名，去掉所有数字
                var stripped = System.Text.RegularExpressions.Regex.Replace(raw, @"\d+", "");
                stripped = stripped.Replace("s", "").Trim();
                stripped = System.Text.RegularExpressions.Regex.Replace(stripped, @"\s+", " ").Trim();
                DisplayCenterText = stripped;
            }
            else if (!ShowStageText && ShowCountdown)
            {
                // 只显示倒计时 — 匹配 "4s" 或纯数字 "30"
                var match = System.Text.RegularExpressions.Regex.Match(raw, @"(\d+)s?");
                if (match.Success)
                    DisplayCenterText = match.Groups[1].Value + "s";
                else
                    DisplayCenterText = "";
            }
        }

        private void UpdateStageDisplay()
        {
            StageDisplayName = _breathing.CurrentStage switch
            {
                BreathStage.Idle => "空闲",
                BreathStage.Inhale => "吸气",
                BreathStage.Exhale => "呼气",
                BreathStage.Hold => "屏息",
                BreathStage.WimHofBreathing => "快速呼吸",
                BreathStage.WimHofRetention => "屏息保持",
                BreathStage.WimHofRecovery => "恢复呼吸",
                BreathStage.Finished => "完成",
                _ => "空闲"
            };
        }

        private void UpdateModeDisplay()
        {
            ModeDisplayName = _breathing.CurrentMode switch
            {
                BreathMode.BoxBreathing => "盒式呼吸",
                BreathMode.FourSevenEight => "4-7-8 呼吸",
                BreathMode.Resonance => "共鸣呼吸",
                BreathMode.PhysiologicalSigh => "生理性叹息",
                BreathMode.WimHof => "Wim Hof",
                _ => "盒式呼吸"
            };
        }

        public void UpdateModeFromService()
        {
            UpdateModeDisplay();
            UpdateStageDisplay();
            if (!IsRunning && !_breathing.IsPaused)
                CenterText = $"{ModeDisplayName}\n点击开始";
            RefreshDisplayText();
        }

        // 设计时构造
        public MainViewModel()
            : this(new BreathingService(new AnimationService()), new SettingsService())
        { }
    }
}

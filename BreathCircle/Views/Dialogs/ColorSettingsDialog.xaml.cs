using System;
using System.Collections.Generic;
using System.Windows;
using BreathCircle.Helpers;
using BreathCircle.Models;
using MediaColor = System.Windows.Media.Color;
using WinTextBox = System.Windows.Controls.TextBox;
using WinListBoxItem = System.Windows.Controls.ListBoxItem;

namespace BreathCircle.Views.Dialogs
{
    public partial class ColorSettingsDialog : Window
    {
        private readonly Dictionary<string, MediaColor> _colors = new();
        private string _currentMode = "BoxBreathing";
        private bool _updating;

        public Dictionary<string, string> Result { get; private set; } = new();

        public ColorSettingsDialog(Dictionary<string, string> currentColors)
        {
            InitializeComponent();

            // 加载颜色（优先自定义，否则默认）
            foreach (var mode in new[] { "BoxBreathing", "FourSevenEight", "Resonance", "PhysiologicalSigh", "WimHof" })
            {
                if (currentColors.TryGetValue(mode, out var hex) && ColorThemes.TryParseHex(hex, out var c))
                    _colors[mode] = c;
                else
                    _colors[mode] = ColorThemes.DefaultColor(Enum.Parse<BreathMode>(mode));
            }

            // --- 手动绑定事件（避免 InitializeComponent 期间提前触发） ---
            ModeList.SelectionChanged += OnModeSelected;
            SliderR.ValueChanged += OnSliderChanged;
            SliderG.ValueChanged += OnSliderChanged;
            SliderB.ValueChanged += OnSliderChanged;
            BoxR.TextChanged += OnRgbTextChanged;
            BoxG.TextChanged += OnRgbTextChanged;
            BoxB.TextChanged += OnRgbTextChanged;
            HexBox.TextChanged += OnHexTextChanged;

            // 初始选中
            ModeList.SelectedIndex = 0;
            if (_colors.TryGetValue("BoxBreathing", out var initColor))
                LoadColorToUI(initColor);
        }

        // ===== 事件 =====

        private void OnModeSelected(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (ModeList?.SelectedItem is WinListBoxItem item && item.Tag is string tag)
            {
                _currentMode = tag;
                if (_colors.TryGetValue(tag, out var color))
                    LoadColorToUI(color);
            }
        }

        private void OnSliderChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_updating) return;
            SyncFromSliders();
        }

        private void OnRgbTextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (_updating) return;
            if (sender is WinTextBox box && int.TryParse(box.Text, out int v) && v >= 0 && v <= 255)
            {
                _updating = true;
                switch (box.Tag as string)
                {
                    case "R": SliderR.Value = v; break;
                    case "G": SliderG.Value = v; break;
                    case "B": SliderB.Value = v; break;
                }
                _updating = false;
                SyncFromSliders();
            }
        }

        private void OnHexTextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (_updating) return;
            string hex = HexBox?.Text?.Trim().ToUpper() ?? "";
            if (hex.Length == 6 && ColorThemes.TryParseHex(hex, out var c))
            {
                _updating = true;
                SliderR.Value = c.R;
                SliderG.Value = c.G;
                SliderB.Value = c.B;
                _updating = false;
                if (!string.IsNullOrEmpty(_currentMode))
                {
                    _colors[_currentMode] = c;
                    UpdatePreview(c);
                }
            }
        }

        // ===== 按钮 =====

        private void OnAllDefaultsClick(object sender, RoutedEventArgs e)
        {
            foreach (var mode in _colors.Keys)
                _colors[mode] = ColorThemes.DefaultColor(Enum.Parse<BreathMode>(mode));
            if (_colors.TryGetValue(_currentMode, out var defColor))
                LoadColorToUI(defColor);
        }

        private void OnSaveClick(object sender, RoutedEventArgs e)
        {
            Result = new Dictionary<string, string>();
            foreach (var (mode, color) in _colors)
                Result[mode] = ColorThemes.ToHex(color);
            DialogResult = true;
            Close();
        }

        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        // ===== 内部 =====

        private void SyncFromSliders()
        {
            byte r = (byte)SliderR.Value;
            byte g = (byte)SliderG.Value;
            byte b = (byte)SliderB.Value;
            var c = MediaColor.FromRgb(r, g, b);
            _colors[_currentMode] = c;
            UpdatePreview(c);

            _updating = true;
            BoxR.Text = r.ToString();
            BoxG.Text = g.ToString();
            BoxB.Text = b.ToString();
            HexBox.Text = ColorThemes.ToHex(c);
            _updating = false;
        }

        private void LoadColorToUI(MediaColor c)
        {
            _updating = true;
            SliderR.Value = c.R;
            SliderG.Value = c.G;
            SliderB.Value = c.B;
            BoxR.Text = c.R.ToString();
            BoxG.Text = c.G.ToString();
            BoxB.Text = c.B.ToString();
            HexBox.Text = ColorThemes.ToHex(c);
            _updating = false;
            UpdatePreview(c);
        }

        private void UpdatePreview(MediaColor c)
        {
            if (PreviewSwatch != null)
                PreviewSwatch.Fill = new System.Windows.Media.SolidColorBrush(c);
            if (PreviewHex != null)
                PreviewHex.Text = $"#{c.R:X2}{c.G:X2}{c.B:X2}";
        }
    }
}

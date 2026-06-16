using System.Windows;
using BreathCircle.Models;

namespace BreathCircle.Views.Dialogs
{
    public partial class WimHofSettingsDialog : Window
    {
        public WimHofSettings Result { get; private set; } = new();

        public WimHofSettingsDialog(WimHofSettings current)
        {
            InitializeComponent();
            Owner = System.Windows.Application.Current.MainWindow;

            RoundsBox.Text = current.Rounds.ToString();
            BreathCountBox.Text = current.BreathCount.ToString();
            InhaleBox.Text = current.InhaleSeconds.ToString("0.0");
            ExhaleBox.Text = current.ExhaleSeconds.ToString("0.0");
            BaseRetentionBox.Text = current.BaseRetentionSeconds.ToString();
            IncrementBox.Text = current.RetentionIncrementSeconds.ToString();
            RecoveryBox.Text = current.RecoveryHoldSeconds.ToString("0");
            ExhaleBufferBox.Text = current.RecoveryExhaleSeconds.ToString("0.0");
        }

        private void OnSaveClick(object sender, RoutedEventArgs e)
        {
            if (!ValidateInputs()) return;

            Result.Rounds = int.Parse(RoundsBox.Text);
            Result.BreathCount = int.Parse(BreathCountBox.Text);
            Result.InhaleSeconds = double.Parse(InhaleBox.Text);
            Result.ExhaleSeconds = double.Parse(ExhaleBox.Text);
            Result.BaseRetentionSeconds = int.Parse(BaseRetentionBox.Text);
            Result.RetentionIncrementSeconds = int.Parse(IncrementBox.Text);
            Result.RecoveryHoldSeconds = double.Parse(RecoveryBox.Text);
            Result.RecoveryExhaleSeconds = double.Parse(ExhaleBufferBox.Text);

            DialogResult = true;
            Close();
        }

        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void OnDefaultsClick(object sender, RoutedEventArgs e)
        {
            RoundsBox.Text = "4";
            BreathCountBox.Text = "30";
            InhaleBox.Text = "1.0";
            ExhaleBox.Text = "1.0";
            BaseRetentionBox.Text = "30";
            IncrementBox.Text = "15";
            RecoveryBox.Text = "15";
            ExhaleBufferBox.Text = "3.0";
        }

        private bool ValidateInputs()
        {
            if (!int.TryParse(RoundsBox.Text, out int r) || r < 1 || r > 10)
            { ShowError("轮次需为 1~10"); return false; }
            if (!int.TryParse(BreathCountBox.Text, out int b) || b < 1 || b > 100)
            { ShowError("呼吸次数需为 1~100"); return false; }
            if (!double.TryParse(InhaleBox.Text, out double inh) || inh < 0.3 || inh > 5.0)
            { ShowError("吸气时长需为 0.3~5.0 秒"); return false; }
            if (!double.TryParse(ExhaleBox.Text, out double exh) || exh < 0.3 || exh > 5.0)
            { ShowError("呼气时长需为 0.3~5.0 秒"); return false; }
            if (!int.TryParse(BaseRetentionBox.Text, out int baseRet) || baseRet < 10 || baseRet > 300)
            { ShowError("屏息时长需为 10~300 秒"); return false; }
            if (!int.TryParse(IncrementBox.Text, out int inc) || inc < 0 || inc > 120)
            { ShowError("递增需为 0~120 秒"); return false; }
            if (!double.TryParse(RecoveryBox.Text, out double rec) || rec < 5 || rec > 60)
            { ShowError("恢复屏息需为 5~60 秒"); return false; }
            if (!double.TryParse(ExhaleBufferBox.Text, out double eb) || eb < 1 || eb > 30)
            { ShowError("吐气缓冲需为 1~30 秒"); return false; }
            return true;
        }

        private void ShowError(string msg)
        {
            System.Windows.MessageBox.Show(this, msg, "输入错误",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
        }
    }
}

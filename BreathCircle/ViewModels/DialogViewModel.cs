using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BreathCircle.ViewModels
{
    /// <summary>
    /// Wim Hof 安全对话框 ViewModel
    /// </summary>
    public partial class DialogViewModel : ObservableObject
    {
        [ObservableProperty]
        private bool _dontShowAgain;

        [ObservableProperty]
        private bool _userAccepted;

        /// <summary>
        /// 对话框标题
        /// </summary>
        public string Title => "⚠ Wim Hof 安全提示";

        /// <summary>
        /// 安全警告文本
        /// </summary>
        public string WarningText =>
            "Wim Hof 呼吸法包含强力呼吸和长时间屏息，\n" +
            "可能导致头晕、四肢麻木或意识模糊。\n\n" +
            "请勿在以下情况练习：\n" +
            "• 驾驶或操作机械时\n" +
            "• 游泳或靠近水域时\n" +
            "• 患有心脏病、高血压或癫痫\n" +
            "• 怀孕期间\n\n" +
            "屏息时间到达120秒将自动进入下一阶段。\n" +
            "请始终在安全环境中练习。";

        [RelayCommand]
        private void Accept()
        {
            UserAccepted = true;
        }

        [RelayCommand]
        private void Cancel()
        {
            UserAccepted = false;
        }
    }
}

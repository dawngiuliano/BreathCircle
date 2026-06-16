using System.Windows;

namespace BreathCircle.Views.Dialogs
{
    /// <summary>
    /// WimHofSafetyDialog.xaml 的交互逻辑
    /// </summary>
    public partial class WimHofSafetyDialog : Window
    {
        /// <summary>用户是否勾选"不再显示"</summary>
        public bool DontShowAgain => DontShowAgainCheckBox.IsChecked == true;

        public WimHofSafetyDialog()
        {
            InitializeComponent();
        }

        private void OnAcceptClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}

using System.Windows;
using WpfApplication = System.Windows.Application;

namespace BreathCircle
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// 应用程序入口：创建主窗口，管理应用生命周期
    /// </summary>
    public partial class App : WpfApplication
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 创建并显示主窗口
            var mainWindow = new MainWindow();
            mainWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
        }
    }
}

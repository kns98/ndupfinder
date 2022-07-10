using System.Windows;

namespace deduper.wpf
{
    public partial class App : Application
    {
        private void OnApplicationStartup(object sender, StartupEventArgs args)
        {
            var mainWindow = new MainWindow();
            mainWindow.Show();
        }
    }
}
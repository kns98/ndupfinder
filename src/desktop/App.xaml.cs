using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using deduper.console;
using deduper.core;

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
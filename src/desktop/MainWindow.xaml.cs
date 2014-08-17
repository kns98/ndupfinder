using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Forms;
using deduper.console;
using deduper.core;

namespace deduper.wpf
{
    public sealed partial class MainWindow : Window
    {
        public DuplicateGroupCollection DuplicateGroups;

        public MainWindow()
        {
            InitializeComponent();
            DuplicateGroups = (DuplicateGroupCollection) this.DataContext;
        }

        private async void OnImagesDirChangeClick(object sender, RoutedEventArgs e)
        {
            btnChange.IsEnabled = false;
            await DuplicateGroups.UpdateAsync(
                new MyDispatcher(Dispatcher), 
                new Directory(ImagesDir.Text));
            
            btnChange.IsEnabled = true;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            ImagesDir.Text = Environment.CurrentDirectory + "\\images";
        }

        private void BtnBrowse_OnClick(object sender, RoutedEventArgs e)
        {
            var dialog = new FolderBrowserDialog();
            dialog.SelectedPath = ImagesDir.Text;
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ImagesDir.Text = dialog.SelectedPath;
            }
        }
    }
}
// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

using System;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using deduper.core;

namespace deduper.win8store
{
    /// <summary>
    ///     An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public DuplicateGroupCollection DuplicateGroups;
        private StorageFolder _root;

        public MainPage()
        {
            InitializeComponent();
            DuplicateGroups = (DuplicateGroupCollection) DataContext;
        }

        private async void OnImagesDirChangeClick(object sender, RoutedEventArgs e)
        {
            btnChange.IsEnabled = false;
            DuplicateGroups.Bc = new BitMapCreator(_root, 200);
            await DuplicateGroups.Update(
                //new MyDispatcher(Dispatcher), 
                new Directory(_root));

            btnChange.IsEnabled = true;
        }


        private async void BtnBrowse_OnClick(object sender, RoutedEventArgs e)
        {
            var picker = new FolderPicker();
            picker.FileTypeFilter.Add(".jpg");
            _root = await picker.PickSingleFolderAsync();

            if (_root != null) ImagesDir.Text = _root.Path;
        }
    }
}
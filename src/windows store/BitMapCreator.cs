using System;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using deduper.core;

namespace deduper.win8store
{
    internal class BitMapCreator : IBitMapCreator
    {
        private readonly StorageFolder _root;
        private readonly uint _size;

        public BitMapCreator(StorageFolder root, uint size)
        {
            _root = root;
            _size = size;
        }

        public async Task<object> Create(string path)
        {
            if (DesignMode.DesignModeEnabled)
            {
                return new BitmapImage(new Uri("ms-appx:///Assets/DesignTime.png"));
            }
            
            StorageFile f = await File.GetFromRoot(path, _root);
            StorageItemThumbnail t = await f.GetScaledImageAsThumbnailAsync(ThumbnailMode.SingleItem, _size);
            var image = new BitmapImage();
            await image.SetSourceAsync(t);
            return image;
        }
    }
}
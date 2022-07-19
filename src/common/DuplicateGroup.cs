using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;

namespace deduper.core
{
    public class DuplicateGroup : ObservableCollection<Duplicate>
    {
        private readonly IBitMapCreator _bc;
        private long _size;
        private bool _triedToSetThumb;

        public DuplicateGroup(IBitMapCreator bc)
        {
            _bc = bc;
        }

        public long FileSize
        {
            get => _size;

            private set
            {
                _size = value;
                OnPropertyChanged(new PropertyChangedEventArgs("FileSize"));
            }
        }

        public object Thumb { get; private set; }

        private async void SetThumb(string path)
        {
            try
            {
                if (!_triedToSetThumb)
                {
                    var ext = Path.GetExtension(path).ToUpper();

                    if (
                        ext == ".JPG" ||
                        ext == ".JPEG" ||
                        ext == ".PNG" ||
                        ext == ".GIF" ||
                        ext == ".BMP" ||
                        false
                    )
                    {
                        Thumb = await _bc.Create(path);
                        OnPropertyChanged(new PropertyChangedEventArgs("Thumb"));
                    }
                }
            }
            catch
            {
            }
            finally
            {
                _triedToSetThumb = true;
            }
        }

        protected override void RemoveItem(int index)
        {
            FileSize -= this[index].FileSize;
            base.RemoveItem(index);
            if (Count == 0)
            {
                Thumb = null;
                _triedToSetThumb = false;
            }
        }

        protected override void ClearItems()
        {
            Thumb = null;
            _triedToSetThumb = false;
            FileSize = 0;
            base.ClearItems();
        }

        protected override void InsertItem(int index, Duplicate item)
        {
            SetThumb(item.Path);
            FileSize += item.FileSize;
            base.InsertItem(index, item);
        }
    }
}
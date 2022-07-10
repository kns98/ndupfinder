using System;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using deduper.core;

namespace deduper.wpf
{
    internal class BitMapCreator : IBitMapCreator
    {
        //fake async to keep common intrface
        public async Task<object> Create(string path)
        {
            return BitmapFrame.Create(new Uri(path));
        }
    }
}
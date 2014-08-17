using System;
using System.Globalization;
using System.Windows.Data;

namespace deduper.wpf
{
    [ValueConversion(typeof (long), typeof (String))]
    public class FileSizeConverter : IValueConverter
    {
        public object Convert(object value,
            Type targetType, object parameter,
            CultureInfo culture)
        {
            var size = (long) value;
            string sizedisplay = size + " Bytes";

            if (size > 1000)
            {
                sizedisplay = string.Format("{0:0.#}", (size/1000d)) + " KB";
            }

            if (size > 1000000)
            {
                sizedisplay = string.Format("{0:0.#}", (size/1000000d)) + " MB";
            }

            if (size > 1000000000)
            {
                sizedisplay = string.Format("{0:0.#}", (size/1000000000d)) + " GB";
            }
            return sizedisplay;
        }

        public object ConvertBack(object value,
            Type targetType, object parameter,
            CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
using System.ComponentModel;
using System.Windows;
using deduper.console;
using deduper.core;

namespace deduper.wpf
{
    internal class DuplicateViewModel : DuplicateGroupCollection
    {
        public DuplicateViewModel()
        {
            Bc = new BitMapCreator();
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                var dff = new DuplicateFileFinder(
                    null,
                    new Directory(@"C:\Users\Kevin\OneDrive\duptest"));
                dff.FindDuplicates();

                var fm = dff._duplicates;

                foreach (var item in fm)
                foreach (var file in item.Value)
                    dff_OnDuplicateFound(item.Key, file.Path, file.GetSize());
            }
        }
    }
}
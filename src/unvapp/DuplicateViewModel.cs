using Windows.ApplicationModel;
using deduper.core;

namespace deduper.win8store
{
    internal class DuplicateViewModel : DuplicateGroupCollection
    {
        public DuplicateViewModel()
        {
            if (DesignMode.DesignModeEnabled)
            {
                Bc = new BitMapCreator(null, 200);
                dff_OnDuplicateFound("a0e1f4abe70237f335ee9320901dcd1b",
                    @"C:\Users\Kevin\Pictures\duptest\New Text Document - Copy (2).txt", 18);
                dff_OnDuplicateFound("a0e1f4abe70237f335ee9320901dcd1b",
                    @"C:\Users\Kevin\Pictures\duptest\New Text Document - Copy.txt", 18);
                dff_OnDuplicateFound("a0e1f4abe70237f335ee9320901dcd1b",
                    @"C:\Users\Kevin\Pictures\duptest\New Text Document.txt", 18);
                dff_OnDuplicateFound("4db9b228e4626704df284622f17fcca4",
                    @"C:\Users\Kevin\Pictures\duptest\New Text Document (2) - Copy.txt", 25);
                dff_OnDuplicateFound("4db9b228e4626704df284622f17fcca4",
                    @"C:\Users\Kevin\Pictures\duptest\New Text Document (2).txt", 25);
                dff_OnDuplicateFound("4db9b228e4626704df284622f17fcca4",
                    @"C:\Users\Kevin\Pictures\duptest\New Text Document (3) - Copy.txt", 25);
                dff_OnDuplicateFound("3bb49eb29922e520d67903a58dbaeaf0",
                    @"C:\Users\Kevin\Pictures\duptest\Duplicate_Test\IMG_1481 - Copy.PNG", 844090);
                dff_OnDuplicateFound("3bb49eb29922e520d67903a58dbaeaf0",
                    @"C:\Users\Kevin\Pictures\duptest\Duplicate_Test\IMG_1481.PNG", 844090);
                dff_OnDuplicateFound("509d4bd9ade7003a14a0cd7407b8f2a5",
                    @"C:\Users\Kevin\Pictures\duptest\Duplicate_Test\WP_20140607_18_48_17_Pro - Copy.jpg", 2750853);
                dff_OnDuplicateFound("509d4bd9ade7003a14a0cd7407b8f2a5",
                    @"C:\Users\Kevin\Pictures\duptest\Duplicate_Test\WP_20140607_18_48_17_Pro.jpg", 2750853);
            }
        }
    }
}
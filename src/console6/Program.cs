using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace deduper.console
{
    internal class DupViewModel : List<KeyValuePair<long, string>>
    {
        public IOrderedEnumerable<KeyValuePair<long, string>> OrderBySizeDescending()
        {
            List<KeyValuePair<long, string>> b = this;

            return b.OrderByDescending(l => l.Key);
        }
    }

    internal class MainClass
    {
        private static readonly DupViewModel Dups = new DupViewModel();
        private static string logpath;

        private static void Main()
        {
            Console.WriteLine("Welcome to Find Duplicates by cloudada(tm)!");
            Console.WriteLine("");
            Console.Write("Please enter the full path of the folder you wish to search :");
            var sourcepath = Console.ReadLine();


            var writesuccess = false;

            while (!writesuccess)
                try
                {
                    Console.Write("Please enter the path in which the results should be written to: ");
                    logpath = Console.ReadLine();
                    logpath = Path.Combine(logpath, "DuplicateFileList.txt");

                    using (var outfile = new StreamWriter(logpath))
                    {
                        outfile.Write("Starting to find duplicates in " + sourcepath);
                        writesuccess = true;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    Console.WriteLine(
                        "Got exception opening log file. Please check the path and your privelges. Please try another location");
                }

            //we know this is fake async
            //but keep the async model for code consistency

            Task.WaitAll(FindDups(sourcepath));

            //dff.RemoveDups();

            Console.WriteLine("Press any key to continue...");
            Console.Read();
        }


        private static void removeEmptyFiles(DirectoryInfo dir)
        {
            IEnumerable<FileInfo> fileList = dir.GetFiles("*.*", SearchOption.AllDirectories);

            IEnumerable<FileInfo> fileQuery =
                from file in fileList
                where file.Length == 0
                orderby file.Name
                select file;

            foreach (FileInfo file in fileQuery)
            {
                Console.WriteLine("Deleting Zero Byte File : " + file.FullName);
                file.Delete();
            }
        }

        private static async Task FindDups(string sourcepath)
        {
            var dff = new DuplicateFileFinder(
                null,
                new Directory(sourcepath)
            );


            dff.OnFileReadError += DffOnOnFileReadError;
            dff.OnPlannedDelete += DffOnOnPlannedDelete;
            dff.OnDuplicateFound += dff_OnDuplicateFound;

            dff.OnHashProgress += dff_OnHashProgress;

            await dff.FindDuplicates();

            LogDups();
            removeEmptyDirs(sourcepath);
            removeEmptyFiles( new DirectoryInfo(sourcepath)  );
        }

        private static void dff_OnHashProgress(string filepath, float percent_done)
        {
            if (percent_done < 100)
                Console.WriteLine("Begining Hash for: " + filepath);
            else
                Console.WriteLine("Completed Hash for:" + filepath);
        }

        private static void removeEmptyDirs(string startLocation)
        {
            foreach (var directory in System.IO.Directory.GetDirectories(startLocation))
            {
                removeEmptyDirs(directory);
                if (System.IO.Directory.GetFiles(directory).Length == 0 &&
                    System.IO.Directory.GetDirectories(directory).Length == 0)
                {
                    Console.WriteLine("Deleting empty dir :" + directory);
                    System.IO.Directory.Delete(directory, false);
                }
            }
        }

        private static void dff_OnDuplicateFound(string hashcode, string filepath, long size)
        {
            Console.WriteLine("Found Dup:" + filepath);

            if (!filepath.StartsWith("0-"))
            {
                Console.WriteLine("Deleing " + filepath);
                File.Delete(filepath.Substring(2));
            }

            Dups.Add(new KeyValuePair<long, string>(size, hashcode + " : " + filepath));
        }

        private static void DffOnOnPlannedDelete(object sender, EventArgs eventArgs)
        {
            if (sender is IFile)
            {
                var s = sender as IFile;
                Console.WriteLine("Planning to Delete " + s.Path);
            }
        }

        private static void DffOnOnFileReadError(string path, Exception e)
        {
            Console.WriteLine(e.Message + " : Error Reading [" + path + "]");
        }

        private static void LogDups()
        {
            var b = new StringBuilder();

            foreach (var fii in Dups.OrderBySizeDescending()) b.Append(fii.Key + " :" + fii.Value + "\n");

            using (var outfile = new StreamWriter(logpath))
            {
                outfile.Write(b);
            }
        }
    }
}
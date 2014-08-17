using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace deduper.core
{
    public class FileMap<T> : Dictionary<T, List<IFile>>
    {
    }

    public class FileMapByString : FileMap<string>
    {
    }

    public class FileMapBySize : FileMap<long>
    {
        private readonly IDispatcher dispatcher;

        private FileMapBySize(IDispatcher dispatcher)
        {
            this.dispatcher = dispatcher;
        }

        private event DuplicateFileFinder.FileReadError OnFileReadError;
        private event EventHandler OnFileScanned;

        //we use a static GetNew instead of a public constructor
        //as some things like waiting on async operations
        //and setting event handlers cannot be done in the 
        //constructor. the real constructor is kept private
        //and only ever called from this static GetNew method

        public static async Task<FileMapBySize> GetNew(
            IDispatcher d,
            IDirectory dir,
            DuplicateFileFinder.FileReadError error,
            EventHandler onscanned)
        {
            var fileMapBySize = new FileMapBySize(d);
            fileMapBySize.OnFileReadError = error;
            fileMapBySize.OnFileScanned = onscanned;
            await fileMapBySize.Populate(dir);
            return fileMapBySize;
        }

        public IEnumerable<IFile> GetPotentialDuplicates()
        {
            //if we wanted to implement a size or type filter
            //we could do it here

            IEnumerable<KeyValuePair<long, List<IFile>>> potentialdups = this.Where(
                l => l.Value.Count > 1 // two files having the same file size are potentially duplicates
                     && l.Key != 0 // ignore zero byte (empty) files 
                );

            foreach (var potentialdupgroup in potentialdups)
            {
                foreach (IFile file in potentialdupgroup.Value)
                {
                    yield return file;
                }
            }
        }

        private async Task Populate(IDirectory dir)
        {
            try
            {
                foreach (IFile file in await dir.GetFiles())
                {
                    try
                    {
                        List<IFile> files;
                        long size = file.GetSize();


                        if (!TryGetValue(size, out files))
                        {
                            files = new List<IFile>();
                            Add(size, files);
                        }

                        files.Add(file);


                        NotifyFileScanned(file);
                    }
                    catch (Exception exception)
                    {
                        NotifyFileReadError(file.Path, exception);
                    }
                }

                foreach (IDirectory directory in await dir.GetSubDirectories())
                {
                    await Populate(directory);
                }
            }
            catch (Exception exception)
            {
                NotifyFileReadError(dir.Path, exception);
            }
        }

        private void NotifyFileScanned(IFile file)
        {
            EventHandler onFileScanned = OnFileScanned;
            Action action = () => onFileScanned(file, new EventArgs());

            if (onFileScanned != null)
            {
                if (dispatcher != null)
                {
                    dispatcher.Execute(action);
                }
                else
                {
                    action();
                }
            }
        }

        private void NotifyFileReadError(string path, Exception e)
        {
            DuplicateFileFinder.FileReadError onFileReadError = OnFileReadError;
            if (onFileReadError != null)
            {
                Action action = () => onFileReadError(path, e);
                if (dispatcher != null)
                {
                    dispatcher.Execute(action);
                }
                else
                {
                    action();
                }
            }
        }
    }
}
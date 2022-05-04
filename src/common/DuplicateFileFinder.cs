using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace deduper.core
{
    public class DuplicateFileFinder
    {
        #region Event Handler Definitions

        //
        //EVENTS
        //The events listed below are used to communcate with the user interface
        //Currently both a console interface as well as XAML interface is supported
        //

        public delegate void DuplicateFound(string hashcode, string filepath, long size);

        public delegate void FileReadError(string filepath, Exception exception);

        public event DuplicateFound OnDuplicateFound;

        public event HashProgress OnHashProgress;

        public event FileReadError OnFileReadError;

        public event EventHandler OnFileScanned;
        public event EventHandler OnPlannedDelete;

        #endregion

        public readonly FileMapByString _duplicates = new FileMapByString();
        private readonly IDirectory _rootDirectory;
        private readonly IDispatcher dispatcher;

        public DuplicateFileFinder(IDispatcher dispatcher, IDirectory rootDirectory)
        {
            this.dispatcher = dispatcher;
            _rootDirectory = rootDirectory;
        }

        #region Notify Events Helper Methods

        private void NotifyFileReadError(IFile file, Exception e)
        {
            FileReadError filereaderror = OnFileReadError;
            if (filereaderror != null)
            {
                filereaderror(file.Path, e);
            }
        }

        private void NotifyDuplicateFound(List<IFile> filelist, string hash, IFile file)
        {
            DuplicateFound onDuplicateFound = OnDuplicateFound;
            if (onDuplicateFound != null)
            {
                // Since each duplicate file is notified via an
                // individial event, we need to fire two events
                // when the first duplicate is detected
                if (filelist.Count == 2)
                {
                    foreach (IFile ff in filelist)
                    {
                        Action action = () => onDuplicateFound(hash, filelist.IndexOf(ff) + "-" + ff.Path, ff.GetSize());

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

                // After the third or greater is duplicate file
                // is found, we already know that the prior duplicate files
                // have had events fired for them, so we only need to
                // fire one event for the latest file found

                if (filelist.Count > 2)
                {
                    Action action = () => onDuplicateFound(hash, filelist.IndexOf(file) + "-" + file.Path, file.GetSize());

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

        #endregion

        public async Task FindDuplicates()
        {
            FileMapBySize fileMapBySize = await FileMapBySize.GetNew(
                dispatcher,
                _rootDirectory,
                OnFileReadError,
                OnFileScanned);

            await BuildDuplicatesList(fileMapBySize);
        }

        private async Task BuildDuplicatesList(FileMapBySize fileMapBySize)
        {
            var fileMapByHashCode = new FileMapByString();

            foreach (IFile file in fileMapBySize.GetPotentialDuplicates())
            {
                try
                {
                    string hash = await file.GetUniqueHash(dispatcher, OnHashProgress);

                    List<IFile> list;
                    if (!fileMapByHashCode.TryGetValue(hash, out list))
                    {
                        list = new List<IFile>();
                        fileMapByHashCode.Add(hash, list);
                    }

                    list.Add(file);

                    // Once we have two files with the same hashcode
                    // we know that a duplicate has been found
                    if (list.Count > 1)
                    {
                        if (!_duplicates.ContainsKey(hash))
                        {
                            _duplicates.Add(hash, list);
                        }

                        NotifyDuplicateFound(list, hash, file);
                    }
                }
                catch (Exception e)
                {
                    NotifyFileReadError(file, e);
                }
            }
        }

        public void RemoveDups()
        {
            foreach (var fii in _duplicates)
            {
                int counter = 1;
                foreach (IFile fi in fii.Value)
                {
                    if (counter > 1)
                    {
                        EventHandler onPlannedDelete = OnPlannedDelete;
                        if (onPlannedDelete != null)
                        {
                            onPlannedDelete(this, new EventArgs());
                        }
                    }
                    counter++;
                }
            }
        }
    }
}

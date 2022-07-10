using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;
using deduper.core;

namespace deduper.win8store
{
    internal class Directory : IDirectory
    {
        private readonly StorageFolder m_root;

        private List<IDirectory> m_dirs;
        private List<IFile> m_files;
        private StorageFolder m_folder;

        private string m_path;

        public Directory(StorageFolder folder, StorageFolder root)
        {
            m_folder = folder;
            m_root = root;

            m_path = folder.Path;
        }

        public Directory(StorageFolder folder)
        {
            m_folder = folder;
            m_root = folder;
            m_path = folder.Path;
        }

        public async Task<IEnumerable<IFile>> GetFiles()
        {
            if (m_files == null)
            {
                m_files = new List<IFile>();
                foreach (StorageFile file in await m_folder.GetFilesAsync())
                    //don't ping the network
                    //make the configurable later
                    if (!file.Attributes.HasFlag(FileAttributes.LocallyIncomplete))
                    {
                        IFile f = await File.GetNew(file, m_root);
                        m_files.Add(f);
                    }
            }

            //we want to clear out the StorageFolder so that it can be 
            //garbage collected after both the files and directories
            //have been retrieved
            if (m_dirs != null) m_folder = null;

            return m_files;
        }

        public async Task<IEnumerable<IDirectory>> GetSubDirectories()
        {
            if (m_dirs == null)
            {
                m_dirs = new List<IDirectory>();
                foreach (StorageFolder dir in await m_folder.GetFoldersAsync()) m_dirs.Add(new Directory(dir, m_root));
            }

            //we want to clear out the StorageFolder so that it can be 
            //garbage collected after both the files and directories
            //have been retrieved
            if (m_files != null) m_folder = null;

            return m_dirs;
        }

        public string Path => m_folder.Path;
    }
}
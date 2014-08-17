using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using deduper.core;

namespace deduper.console
{
    public class Directory : IDirectory
    {
        private readonly string m_path;

        public Directory(string path)
        {
            m_path = path;
        }

        //fake async to keep to interface
        public async Task<IEnumerable<IFile>> GetFiles()
        {
            var l = new List<IFile>();
            string[] files = System.IO.Directory.GetFiles(m_path);

            foreach (string file in files)
            {
                var i = new FileInfo(file);

                if (!i.Attributes.HasFlag(FileAttributes.SparseFile))
                    //don't attempt to get files that are on the skydrive but not downloaded
                {
                    var f = new File(file);
                    l.Add(f);
                }
            }
            return l;
        }

        //fake async to keep to interface
        public async Task<IEnumerable<IDirectory>> GetSubDirectories()
        {
            var l = new List<IDirectory>();
            foreach (string dir in System.IO.Directory.GetDirectories(m_path))
            {
                l.Add(new Directory(dir));
            }
            return l;
        }

        public string Path
        {
            get { return m_path; }
        }
    }
}
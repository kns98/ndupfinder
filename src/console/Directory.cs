using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using deduper.core;

namespace deduper.console
{
    public class Directory : IDirectory
    {
        public Directory(string path)
        {
            Path = path;
        }

        public string Path { get; }

        //fake async to keep to interface
        public async Task<IEnumerable<IFile>> GetFiles()
        {
            var l = new List<IFile>();
            var files = System.IO.Directory.GetFiles(Path);

            foreach (var file in files)
            {
                var i = new FileInfo(file);

                if (!i.Attributes.HasFlag(FileAttributes.SparseFile))
                    //don't attempt to get files that are on the skydrive but not downloaded
                {
                    var f = new DeDupFile(file);
                    l.Add(f);
                }
            }

            return l;
        }

        //fake async to keep to interface
        public async Task<IEnumerable<IDirectory>> GetSubDirectories()
        {
            var l = new List<IDirectory>();
            foreach (var dir in System.IO.Directory.GetDirectories(Path)) l.Add(new Directory(dir));
            return l;
        }
    }
}
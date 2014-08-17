namespace deduper.core
{
    public class Duplicate
    {
        public Duplicate(string path, long size)
        {
            Path = path;
            FileName = System.IO.Path.GetFileName(path);
            FileSize = size;
        }

        public string FileName { get; private set; }
        public long FileSize { get; private set; }
        public string Path { get; private set; }
    }
}
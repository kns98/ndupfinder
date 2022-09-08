namespace deduper.wpf
{
    public class Duplicate
    {
        public Duplicate(string path, long size)
        {
            Path = path;
            FileName = System.IO.Path.GetFileName(path);
            FileSize = size;
        }

        public string FileName { get; }
        public long FileSize { get; }
        public string Path { get; }
    }
}
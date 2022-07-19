using System;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage;
using Windows.Storage.Streams;
using deduper.core;

namespace deduper.win8store
{
    internal class File : IFile
    {
        private static readonly HashAlgorithmProvider m_alg = HashAlgorithmProvider.OpenAlgorithm("MD5");

        private readonly StorageFolder m_root;
        private string m_hash;
        private long m_size;

        private File(string path, StorageFolder root)
        {
            Path = path;
            m_root = root;
        }

        public bool IsHashed()
        {
            return m_hash != null;
        }

        public string Path { get; }

        public long GetSize()
        {
            return m_size;
        }

        public async Task<string> GetUniqueHash(
            IDispatcher d,
            HashProgress notifier)
        {
            if (m_hash == null) await CalculateHash(d, notifier);
            return m_hash;
        }

        public async Task Delete()
        {
            var file = await GetFromRoot(Path, m_root);
            await file.DeleteAsync();
        }

        public async Task Write(StringBuilder b)
        {
            var file = await GetFromRoot(Path, m_root);

            var s = await file.OpenAsync(FileAccessMode.ReadWrite);
            var s1 = s.GetOutputStreamAt(0);
            var d = new DataWriter(s1);
            d.WriteString(b.ToString());
            await d.StoreAsync();
            var success = await s.FlushAsync();
            if (!success) throw new Exception("FlushAsync failed!");
        }

        // only store a pointer to root 
        // because if we keep pointers to all the storage file
        // objects the broker eats up too much memory

        public static async Task<File> GetNew(StorageFile file, StorageFolder root)
        {
            var f = new File(file.Path, root);
            await f.CalculateFileSize(file);
            return f;
        }

        public static async Task<StorageFile> GetFromRoot(string mPath, StorageFolder mRoot)
        {
            //TODO - exception handling
            //Think how best to handle bad code or badly formatted paths
            //or paths not formatted as expected
            //is there any way to gracefully handle this?
            //return a null storage object?

            var rootDirPath = mRoot.Path;
            var fileFullPath = mPath;
            var s = fileFullPath.Substring(0, rootDirPath.Length);

            if (s == rootDirPath)
            {
                var fileSubPath = fileFullPath.Substring(rootDirPath.Length);

                var fileName = System.IO.Path.GetFileName(fileSubPath);
                var dirSubPath = System.IO.Path.GetDirectoryName(fileSubPath);

                var cur = mRoot;

                if (dirSubPath.Length > 1) //assumes the separator is a single character - should improve this
                {
                    var sep = fileSubPath.Substring(dirSubPath.Length, 1);
                    var separator = sep.ToCharArray();
                    var dirs = dirSubPath.Split(separator);

                    foreach (var dir in dirs)
                        if (dir != "")
                            cur = await cur.GetFolderAsync(dir);
                }

                var file = await cur.GetFileAsync(fileName);
                return file;
            }

            throw new Exception("Path is not sub dir of root path");
        }

        private async Task CalculateFileSize(StorageFile file)
        {
            var p = await file.GetBasicPropertiesAsync();
            m_size = (long)p.Size;
        }

        private async Task CalculateHash(
            IDispatcher d,
            HashProgress notifier)
        {
            if (notifier != null)
            {
                Action action = () => notifier(Path, 0);
                if (d == null)
                    action();
                else
                    d.Execute(action);
            }

            //TODO - exception handling
            var file = await GetFromRoot(Path, m_root);
            var num_chunks = (int)(GetSize() / Chunker.chunk_size) + 1;
            var hash_size = num_chunks * 32;
            float current_chunk = 0;
            var hash_builder = new StringBuilder(hash_size);
            m_hash = "";

            var chunker = new Chunker(GetSize());

            foreach (var chunk in chunker.GetChunks())
            {
                using (var inputStream = await file.OpenAsync(FileAccessMode.Read))
                {
                    using (var dataReader = new DataReader(inputStream.GetInputStreamAt(chunk.Start)))
                    {
                        await dataReader.LoadAsync(chunk.Length);
                        var buf = dataReader.ReadBuffer(chunk.Length);
                        var hashed = m_alg.HashData(buf);
                        hash_builder.Append(CryptographicBuffer.EncodeToHexString(hashed));
                    }
                }

                current_chunk++;


                if (notifier != null)
                {
                    var percent_done = current_chunk / num_chunks;
                    Action action = () => notifier(Path, percent_done * 100);
                    if (d == null)
                        action();
                    else
                        d.Execute(action);
                }
            }

            m_hash = hash_builder.ToString();

            if (hash_size > 32) //hash the hash 
            {
                // Convert the string to UTF8 binary data.
                var hashbuf = CryptographicBuffer.ConvertStringToBinary(m_hash, BinaryStringEncoding.Utf8);
                var hashed = m_alg.HashData(hashbuf);
                m_hash = CryptographicBuffer.EncodeToHexString(hashed);
            }

            if (notifier != null)
            {
                Action action = () => notifier(Path, 100);
                if (d == null)
                    action();
                else
                    d.Execute(action);
            }
        }
    }
}
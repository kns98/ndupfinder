using System;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;
using deduper.core;

namespace deduper.win8store
{
    internal class File : IFile
    {
        private static readonly HashAlgorithmProvider m_alg = HashAlgorithmProvider.OpenAlgorithm("MD5");
        private readonly string m_path;

        private readonly StorageFolder m_root;
        private string m_hash;
        private long m_size;

        private File(string path, StorageFolder root)
        {
            m_path = path;
            m_root = root;
        }

        public bool IsHashed()
        {
            return m_hash != null;
        }

        public string Path
        {
            get { return m_path; }
        }

        public long GetSize()
        {
            return m_size;
        }

        public async Task<string> GetUniqueHash(
            IDispatcher d,
            HashProgress notifier)
        {
            if (m_hash == null)
            {
                await CalculateHash(d, notifier);
            }
            return m_hash;
        }

        public async Task Delete()
        {
            StorageFile file = await GetFromRoot(m_path, m_root);
            await file.DeleteAsync();
        }

        public async Task Write(StringBuilder b)
        {
            StorageFile file = await GetFromRoot(m_path, m_root);

            IRandomAccessStream s = await file.OpenAsync(FileAccessMode.ReadWrite);
            IOutputStream s1 = s.GetOutputStreamAt(0);
            var d = new DataWriter(s1);
            d.WriteString(b.ToString());
            await d.StoreAsync();
            bool success = await s.FlushAsync();
            if (!success)
            {
                throw new Exception("FlushAsync failed!");
            }
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

            string rootDirPath = mRoot.Path;
            string fileFullPath = mPath;
            string s = fileFullPath.Substring(0, rootDirPath.Length);

            if (s == rootDirPath)
            {
                string fileSubPath = fileFullPath.Substring(rootDirPath.Length);

                string fileName = System.IO.Path.GetFileName(fileSubPath);
                string dirSubPath = System.IO.Path.GetDirectoryName(fileSubPath);

                StorageFolder cur = mRoot;

                if (dirSubPath.Length > 1) //assumes the separator is a single character - should improve this
                {
                    string sep = fileSubPath.Substring(dirSubPath.Length, 1);
                    char[] separator = sep.ToCharArray();
                    string[] dirs = dirSubPath.Split(separator);

                    foreach (string dir in dirs)
                    {
                        if (dir != "")
                        {
                            cur = await cur.GetFolderAsync(dir);
                        }
                    }
                }
                StorageFile file = await cur.GetFileAsync(fileName);
                return file;
            }
            throw new Exception("Path is not sub dir of root path");
        }

        private async Task CalculateFileSize(StorageFile file)
        {
            BasicProperties p = await file.GetBasicPropertiesAsync();
            m_size = (long) p.Size;
        }

        private async Task CalculateHash(
            IDispatcher d,
            HashProgress notifier)
        {
            if (notifier != null)
            {
                Action action = () => notifier(Path, 0);
                if (d == null)
                {
                    action();
                }
                else
                {
                    d.Execute(action);
                }
            }

            //TODO - exception handling
            StorageFile file = await GetFromRoot(m_path, m_root);
            int num_chunks = (int) (GetSize()/Chunker.chunk_size) + 1;
            int hash_size = num_chunks*32;
            float current_chunk = 0;
            var hash_builder = new StringBuilder(hash_size);
            m_hash = "";

            var chunker = new Chunker(GetSize());

            foreach (Chunk chunk in chunker.GetChunks())
            {
                using (IRandomAccessStream inputStream = await file.OpenAsync(FileAccessMode.Read))
                {
                    using (var dataReader = new DataReader(inputStream.GetInputStreamAt(chunk.Start)))
                    {
                        await dataReader.LoadAsync(chunk.Length);
                        IBuffer buf = dataReader.ReadBuffer(chunk.Length);
                        IBuffer hashed = m_alg.HashData(buf);
                        hash_builder.Append(CryptographicBuffer.EncodeToHexString(hashed));
                    }
                }
                current_chunk++;


                if (notifier != null)
                {
                    float percent_done = current_chunk/num_chunks;
                    Action action = () => notifier(Path, percent_done*100);
                    if (d == null)
                    {
                        action();
                    }
                    else
                    {
                        d.Execute(action);
                    }
                }
            }

            m_hash = hash_builder.ToString();

            if (hash_size > 32) //hash the hash 
            {
                // Convert the string to UTF8 binary data.
                IBuffer hashbuf = CryptographicBuffer.ConvertStringToBinary(m_hash, BinaryStringEncoding.Utf8);
                IBuffer hashed = m_alg.HashData(hashbuf);
                m_hash = CryptographicBuffer.EncodeToHexString(hashed);
            }

            if (notifier != null)
            {
                Action action = () => notifier(Path, 100);
                if (d == null)
                {
                    action();
                }
                else
                {
                    d.Execute(action);
                }
            }
        }
    }
}
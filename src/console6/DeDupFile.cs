using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;


namespace deduper.console
{
    public class DeDupFile : IFile
    {
        private readonly long m_size;
        private string m_hash;

        public DeDupFile(string path)
        {
            Path = path;
            m_size = new FileInfo(Path).Length;
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

        //fake async to keep to interface
        public async Task<string> GetUniqueHash(
            IDispatcher dispatcher,
            HashProgress notifier)
        {
            if (m_hash == null)
            {
                NotifyHashBegin(dispatcher, notifier);

                using (var md5 = MD5.Create())
                {
                    using (var stream = File.OpenRead(Path))
                    {
                        m_hash = BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToLower();
                    }
                }

                NotifyHashEnd(dispatcher, notifier);
            }

            return m_hash;
        }

        //fake async to keep to interface
        public async Task Delete()
        {
            File.Delete(Path);
        }

        //fake async to keep to interface
        public async Task Write(StringBuilder b)
        {
            using (var outfile = new StreamWriter(Path))
            {
                outfile.Write(b);
            }
        }

        private void NotifyHashEnd(IDispatcher dispatcher, HashProgress notifier)
        {
            if (notifier != null)
            {
                Action action = () => notifier(Path, 100);
                if (dispatcher != null)
                    dispatcher.Execute(action);
                else
                    action();
            }
        }

        private void NotifyHashBegin(IDispatcher dispatcher, HashProgress notifier)
        {
            if (notifier != null)
            {
                Action action = () => notifier(Path, 0);
                if (dispatcher != null)
                    dispatcher.Execute(action);
                else
                    action();
            }
        }
    }
}
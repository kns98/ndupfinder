using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace deduper.wpf
{
    public delegate void HashProgress(string filepath, float percent_done);

    public interface IDispatcher
    {
        void Execute(Action method);
        //i dont want to call this invoke to avoid confusing it  with the real dispatcher
        //the implementation can choose whether to use invoke or begininvoke
    }

    public interface IFile
    {
        string Path { get; }
        long GetSize();

        Task<string> GetUniqueHash(
            IDispatcher d,
            HashProgress notifier);

        Task Delete();
        Task Write(StringBuilder b);
        bool IsHashed();
    }

    public interface IDirectory
    {
        string Path { get; }
        Task<IEnumerable<IFile>> GetFiles();
        Task<IEnumerable<IDirectory>> GetSubDirectories();
    }

    public interface IBitMapCreator
    {
        Task<object> Create(string path);
    }
}
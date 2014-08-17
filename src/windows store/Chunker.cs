using System.Collections.Generic;

namespace deduper.win8store
{
    internal class Chunker
    {
        public static readonly uint chunk_size = 1000*1000;
        //private static readonly uint chunk_size = 5;

        private readonly long size;

        public Chunker(long size)
        {
            this.size = size;
        }

        public IEnumerable<Chunk> GetChunks()
        {
            long size_left = size;
            ulong start = 0;
            int index = 0;

            while (size_left > chunk_size)
            {
                size_left -= chunk_size;
                yield return new Chunk(start, chunk_size);

                start += chunk_size;
                index++;
            }

            if (size_left != 0) // will be zero is file size is an exact multiple of chunk size
            {
                yield return new Chunk(start, (uint) size_left);
            }
        }
    }
}
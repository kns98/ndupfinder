namespace deduper.win8store
{
    internal class Chunk
    {
        private readonly uint length;
        private readonly ulong start;

        public Chunk(ulong start, uint length)
        {
            this.start = start;
            this.length = length;
        }

        public ulong Start
        {
            get { return start; }
        }

        public uint Length
        {
            get { return length; }
        }
    }
}
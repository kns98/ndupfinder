namespace deduper.win8store
{
    internal class Chunk
    {
        public Chunk(ulong start, uint length)
        {
            this.Start = start;
            this.Length = length;
        }

        public ulong Start { get; }

        public uint Length { get; }
    }
}
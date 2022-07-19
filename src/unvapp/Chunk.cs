namespace deduper.win8store
{
    internal class Chunk
    {
        public Chunk(ulong start, uint length)
        {
            Start = start;
            Length = length;
        }

        public ulong Start { get; }

        public uint Length { get; }
    }
}
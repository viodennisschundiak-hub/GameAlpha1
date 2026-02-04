using System.Collections.Generic;

namespace Bobo.Simulation
{
    public sealed class SparseWorld
    {
        public readonly Dictionary<ChunkCoord, Chunk> Chunks = new();
        private readonly int _chunkSize;

        public SparseWorld(int chunkSize)
        {
            _chunkSize = chunkSize;
        }

        public bool TryGetChunk(ChunkCoord coord, out Chunk chunk) => Chunks.TryGetValue(coord, out chunk);

        public Chunk GetOrCreateChunk(ChunkCoord coord)
        {
            if (!Chunks.TryGetValue(coord, out Chunk chunk))
            {
                chunk = new Chunk(_chunkSize);
                Chunks.Add(coord, chunk);
            }

            return chunk;
        }

        public void RemoveChunk(ChunkCoord coord)
        {
            if (Chunks.ContainsKey(coord))
            {
                Chunks.Remove(coord);
            }
        }
    }
}

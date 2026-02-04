using System;

namespace Bobo.Simulation
{
    [Serializable]
    public sealed class Chunk
    {
        public readonly int Size;
        public readonly byte[] Alive;
        public readonly float[] Energy;
        public readonly int[] Age;

        public Chunk(int size)
        {
            Size = size;
            int count = size * size;
            Alive = new byte[count];
            Energy = new float[count];
            Age = new int[count];
        }

        public bool HasLivingCells()
        {
            for (int i = 0; i < Alive.Length; i++)
            {
                if (Alive[i] == 1)
                {
                    return true;
                }
            }

            return false;
        }

        public int Index(int x, int y) => x + y * Size;
    }
}

using System.Collections.Generic;
using UnityEngine;

namespace Bobo.Simulation
{
    public static class Presets
    {
        public static readonly Vector2Int[] Glider =
        {
            new Vector2Int(1, 0),
            new Vector2Int(2, 1),
            new Vector2Int(0, 2),
            new Vector2Int(1, 2),
            new Vector2Int(2, 2)
        };

        public static readonly Vector2Int[] Blinker =
        {
            new Vector2Int(0, 0),
            new Vector2Int(1, 0),
            new Vector2Int(2, 0)
        };

        public static IEnumerable<Vector2Int> Random(int seed, int count, int spread)
        {
            System.Random random = new System.Random(seed);
            for (int i = 0; i < count; i++)
            {
                yield return new Vector2Int(random.Next(-spread, spread), random.Next(-spread, spread));
            }
        }
    }
}

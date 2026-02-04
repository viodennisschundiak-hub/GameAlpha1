using System;
using System.Collections.Generic;
using UnityEngine;

namespace Bobo.Simulation
{
    public sealed class SimulationCore
    {
        private readonly SimulationSettings _settings;
        private readonly SparseWorld _world;
        private readonly HashSet<ChunkCoord> _dirtyChunks = new();

        public event Action OnWorldChanged;

        public int TickCount { get; private set; }
        public int Population { get; private set; }

        public SimulationCore(SimulationSettings settings)
        {
            _settings = settings;
            _world = new SparseWorld(settings.chunkSize);
        }

        public SparseWorld World => _world;

        public void MarkDirty(ChunkCoord coord)
        {
            _dirtyChunks.Add(coord);
        }

        public void Reset()
        {
            _world.Chunks.Clear();
            _dirtyChunks.Clear();
            TickCount = 0;
            Population = 0;
            OnWorldChanged?.Invoke();
        }

        public void SetCellAlive(int worldX, int worldY, float energy, int age)
        {
            ChunkCoord coord = WorldToChunkCoord(worldX, worldY);
            int localX = Mod(worldX, _settings.chunkSize);
            int localY = Mod(worldY, _settings.chunkSize);
            Chunk chunk = _world.GetOrCreateChunk(coord);
            int index = chunk.Index(localX, localY);
            chunk.Alive[index] = 1;
            chunk.Energy[index] = energy;
            chunk.Age[index] = age;
            MarkDirty(coord);
            RecalculatePopulation();
        }

        public void SetCellDead(int worldX, int worldY)
        {
            ChunkCoord coord = WorldToChunkCoord(worldX, worldY);
            if (!_world.TryGetChunk(coord, out Chunk chunk))
            {
                return;
            }

            int localX = Mod(worldX, _settings.chunkSize);
            int localY = Mod(worldY, _settings.chunkSize);
            int index = chunk.Index(localX, localY);
            chunk.Alive[index] = 0;
            chunk.Energy[index] = 0f;
            chunk.Age[index] = 0;
            MarkDirty(coord);
            if (!chunk.HasLivingCells())
            {
                _world.RemoveChunk(coord);
            }

            RecalculatePopulation();
        }

        public bool IsAlive(int worldX, int worldY)
        {
            ChunkCoord coord = WorldToChunkCoord(worldX, worldY);
            if (!_world.TryGetChunk(coord, out Chunk chunk))
            {
                return false;
            }

            int localX = Mod(worldX, _settings.chunkSize);
            int localY = Mod(worldY, _settings.chunkSize);
            return chunk.Alive[chunk.Index(localX, localY)] == 1;
        }

        public float GetEnergy(int worldX, int worldY)
        {
            ChunkCoord coord = WorldToChunkCoord(worldX, worldY);
            if (!_world.TryGetChunk(coord, out Chunk chunk))
            {
                return 0f;
            }

            int localX = Mod(worldX, _settings.chunkSize);
            int localY = Mod(worldY, _settings.chunkSize);
            return chunk.Energy[chunk.Index(localX, localY)];
        }

        public int GetAge(int worldX, int worldY)
        {
            ChunkCoord coord = WorldToChunkCoord(worldX, worldY);
            if (!_world.TryGetChunk(coord, out Chunk chunk))
            {
                return 0;
            }

            int localX = Mod(worldX, _settings.chunkSize);
            int localY = Mod(worldY, _settings.chunkSize);
            return chunk.Age[chunk.Index(localX, localY)];
        }

        public void Tick()
        {
            HashSet<ChunkCoord> activeChunks = CollectActiveChunks();
            Dictionary<ChunkCoord, Chunk> nextChunks = new(_world.Chunks);
            HashSet<ChunkCoord> newDirty = new();

            foreach (ChunkCoord coord in activeChunks)
            {
                Chunk nextChunk = new Chunk(_settings.chunkSize);
                bool chunkHasLife = false;
                bool chunkChanged = false;

                for (int y = 0; y < _settings.chunkSize; y++)
                {
                    for (int x = 0; x < _settings.chunkSize; x++)
                    {
                        int worldX = coord.X * _settings.chunkSize + x;
                        int worldY = coord.Y * _settings.chunkSize + y;
                        bool isAlive = IsAlive(worldX, worldY);
                        int liveNeighbors = CountLiveNeighbors(worldX, worldY);
                        int free = 8 - liveNeighbors;

                        if (isAlive)
                        {
                            float energy = GetEnergy(worldX, worldY);
                            energy += _settings.gainFree * free - _settings.costAlive - _settings.costCrowd * liveNeighbors;
                            if (energy > 0f && (liveNeighbors == 2 || liveNeighbors == 3))
                            {
                                int index = nextChunk.Index(x, y);
                                nextChunk.Alive[index] = 1;
                                nextChunk.Energy[index] = energy;
                                nextChunk.Age[index] = GetAge(worldX, worldY) + 1;
                                chunkHasLife = true;
                                if (!isAlive || Math.Abs(energy - GetEnergy(worldX, worldY)) > 0.0001f)
                                {
                                    chunkChanged = true;
                                }
                            }
                            else
                            {
                                chunkChanged = true;
                            }
                        }
                        else
                        {
                            if (liveNeighbors == 3 && free >= _settings.freeThreshold)
                            {
                                if (HasNeighborWithEnergy(worldX, worldY, _settings.reproduceThreshold))
                                {
                                    int index = nextChunk.Index(x, y);
                                    nextChunk.Alive[index] = 1;
                                    nextChunk.Energy[index] = _settings.newbornEnergy;
                                    nextChunk.Age[index] = 0;
                                    chunkHasLife = true;
                                    chunkChanged = true;
                                }
                            }
                        }
                    }
                }

                if (chunkHasLife)
                {
                    nextChunks[coord] = nextChunk;
                }
                else if (nextChunks.ContainsKey(coord))
                {
                    nextChunks.Remove(coord);
                    chunkChanged = true;
                }

                if (chunkChanged)
                {
                    newDirty.Add(coord);
                }
            }

            _world.Chunks.Clear();
            foreach (KeyValuePair<ChunkCoord, Chunk> pair in nextChunks)
            {
                _world.Chunks.Add(pair.Key, pair.Value);
            }

            _dirtyChunks.Clear();
            foreach (ChunkCoord coord in newDirty)
            {
                _dirtyChunks.Add(coord);
            }

            if (_settings.enableMovement)
            {
                ApplyMovement();
            }

            TickCount++;
            RecalculatePopulation();
#if UNITY_EDITOR
            ValidateWorld();
#endif
            OnWorldChanged?.Invoke();
        }

        public IEnumerable<ChunkCoord> GetActiveChunks() => _world.Chunks.Keys;

        private void ApplyMovement()
        {
            Dictionary<Vector2Int, MoveCandidate> moves = new();
            HashSet<ChunkCoord> movementDirty = new();

            foreach (KeyValuePair<ChunkCoord, Chunk> pair in _world.Chunks)
            {
                ChunkCoord coord = pair.Key;
                for (int y = 0; y < _settings.chunkSize; y++)
                {
                    for (int x = 0; x < _settings.chunkSize; x++)
                    {
                        int worldX = coord.X * _settings.chunkSize + x;
                        int worldY = coord.Y * _settings.chunkSize + y;
                        if (!IsAlive(worldX, worldY))
                        {
                            continue;
                        }

                        int stress = CountLiveNeighbors(worldX, worldY);
                        if (stress < _settings.moveStressThreshold)
                        {
                            continue;
                        }

                        MoveTarget best = FindBestMove(worldX, worldY);
                        if (!best.IsValid)
                        {
                            continue;
                        }

                        Vector2Int target = new Vector2Int(best.WorldX, best.WorldY);
                        float energy = GetEnergy(worldX, worldY);
                        int age = GetAge(worldX, worldY);
                        int hash = StableHash(worldX, worldY);
                        MoveCandidate candidate = new MoveCandidate(worldX, worldY, energy, age, hash);

                        if (moves.TryGetValue(target, out MoveCandidate existing))
                        {
                            if (candidate.IsBetterThan(existing))
                            {
                                moves[target] = candidate;
                            }
                        }
                        else
                        {
                            moves[target] = candidate;
                        }
                    }
                }
            }

            foreach (KeyValuePair<Vector2Int, MoveCandidate> move in moves)
            {
                Vector2Int target = move.Key;
                MoveCandidate candidate = move.Value;
                if (IsAlive(target.x, target.y))
                {
                    continue;
                }

                SetCellDead(candidate.WorldX, candidate.WorldY);
                SetCellAlive(target.x, target.y, candidate.Energy, candidate.Age);
                movementDirty.Add(WorldToChunkCoord(candidate.WorldX, candidate.WorldY));
                movementDirty.Add(WorldToChunkCoord(target.x, target.y));
            }

            foreach (ChunkCoord coord in movementDirty)
            {
                MarkDirty(coord);
            }
        }

        private MoveTarget FindBestMove(int worldX, int worldY)
        {
            MoveTarget best = MoveTarget.Invalid;
            for (int dy = -1; dy <= 1; dy++)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    if (dx == 0 && dy == 0)
                    {
                        continue;
                    }

                    int targetX = worldX + dx;
                    int targetY = worldY + dy;
                    if (IsAlive(targetX, targetY))
                    {
                        continue;
                    }

                    int free = 8 - CountLiveNeighbors(targetX, targetY);
                    if (!best.IsValid || free > best.Free)
                    {
                        best = new MoveTarget(targetX, targetY, free);
                    }
                }
            }

            return best;
        }

        private HashSet<ChunkCoord> CollectActiveChunks()
        {
            HashSet<ChunkCoord> active = new();
            foreach (ChunkCoord coord in _dirtyChunks)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        active.Add(new ChunkCoord(coord.X + dx, coord.Y + dy));
                    }
                }
            }

            if (active.Count == 0)
            {
                foreach (ChunkCoord coord in _world.Chunks.Keys)
                {
                    active.Add(coord);
                }
            }

            return active;
        }

        private int CountLiveNeighbors(int worldX, int worldY)
        {
            int count = 0;
            for (int dy = -1; dy <= 1; dy++)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    if (dx == 0 && dy == 0)
                    {
                        continue;
                    }

                    if (IsAlive(worldX + dx, worldY + dy))
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        private bool HasNeighborWithEnergy(int worldX, int worldY, float threshold)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    if (dx == 0 && dy == 0)
                    {
                        continue;
                    }

                    if (IsAlive(worldX + dx, worldY + dy) && GetEnergy(worldX + dx, worldY + dy) >= threshold)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private void RecalculatePopulation()
        {
            int count = 0;
            foreach (Chunk chunk in _world.Chunks.Values)
            {
                for (int i = 0; i < chunk.Alive.Length; i++)
                {
                    if (chunk.Alive[i] == 1)
                    {
                        count++;
                    }
                }
            }

            Population = count;
        }

        private void ValidateWorld()
        {
            foreach (Chunk chunk in _world.Chunks.Values)
            {
                for (int i = 0; i < chunk.Alive.Length; i++)
                {
                    if (chunk.Alive[i] == 1 && chunk.Energy[i] < 0f)
                    {
                        chunk.Energy[i] = 0f;
                    }
                }
            }
        }

        private ChunkCoord WorldToChunkCoord(int worldX, int worldY)
        {
            int chunkX = Mathf.FloorToInt(worldX / (float)_settings.chunkSize);
            int chunkY = Mathf.FloorToInt(worldY / (float)_settings.chunkSize);
            return new ChunkCoord(chunkX, chunkY);
        }

        private static int Mod(int value, int mod)
        {
            int result = value % mod;
            return result < 0 ? result + mod : result;
        }

        private static int StableHash(int x, int y)
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + x;
                hash = hash * 31 + y;
                return hash;
            }
        }

        private readonly struct MoveTarget
        {
            public static readonly MoveTarget Invalid = new MoveTarget(0, 0, -1);

            public readonly int WorldX;
            public readonly int WorldY;
            public readonly int Free;

            public MoveTarget(int worldX, int worldY, int free)
            {
                WorldX = worldX;
                WorldY = worldY;
                Free = free;
            }

            public bool IsValid => Free >= 0;
        }

        private readonly struct MoveCandidate
        {
            public readonly int WorldX;
            public readonly int WorldY;
            public readonly float Energy;
            public readonly int Age;
            public readonly int Hash;

            public MoveCandidate(int worldX, int worldY, float energy, int age, int hash)
            {
                WorldX = worldX;
                WorldY = worldY;
                Energy = energy;
                Age = age;
                Hash = hash;
            }

            public bool IsBetterThan(MoveCandidate other)
            {
                if (Energy > other.Energy)
                {
                    return true;
                }

                if (Math.Abs(Energy - other.Energy) > 0.0001f)
                {
                    return false;
                }

                return Hash > other.Hash;
            }
        }
    }
}

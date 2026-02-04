# Bobo Simulator (Unity 2022 LTS+)

## Overview
Bobo Simulator is a 2D, chunked, sparse-world simulation inspired by Conway’s Game of Life with energy, reproduction constraints, optional movement, and a performance-first rendering pipeline designed for a huge logical world (100000x100000) and a small viewport (1080x720).

## Build & Run
1. Open the project folder in **Unity 2022 LTS** or newer.
2. Open the following scenes (create if missing, or wire in your own UI prefabs):
   - `MainMenu`
   - `WorldCreator`
   - `GameScene`
3. Press Play.

> Note: The scripts are organized for a multi-scene flow. Hook up the UI prefabs to the corresponding controllers if you create your own UI Toolkit or Canvas UI.

## How to Play
- Use the **World Creator** to pick a world name, seed, preset, and default parameters.
- In-game, use **Play/Pause**, **Step**, and **Sim Speed** to control tick rate.
- Use the **Brush** to paint live cells.
- Pan/Zoom to explore the world. The renderer aggregates cells when zoomed out.

## TECH DESIGN
### World Model & Chunking
- The world uses a **sparse dictionary of chunks** keyed by `(chunkX, chunkY)`.
- Each chunk is **256x256 cells**, storing:
  - `alive` state as `byte` (0/1)
  - `energy` (float)
  - `age` (int)
- Only active chunks are stored; empty chunks are removed.

### Update Phases
1. **Dirty Chunk Collection**: On any edit or tick update, mark a chunk as dirty.
2. **Simulation Tick** (operates only on dirty chunks + neighbors):
   - Conway rules applied with energy and reproduction constraints.
   - Energy update per tick:
     - `stress = live neighbors`
     - `free = 8 - stress`
     - `E += gain_free*free - cost_alive - cost_crowd*stress`
   - Death if `E <= 0`.
   - Birth only if **Conway birth** AND at least one neighbor has `E >= reproduce_threshold` AND the target cell has `free >= free_threshold`.
3. **Movement Phase** (optional):
   - Cells with `stress >= move_stress_threshold` can move to the neighboring free cell with the highest `free` score.
   - Conflicts are resolved deterministically using energy and a stable hash tiebreaker.

### Performance Strategy
- Only **dirty chunks + neighbors** are evaluated each tick.
- Rendering is **Texture2D-based** and only updates the current viewport region.
- When zoomed out, the renderer aggregates multiple cells into one pixel intensity.

### Rendering Strategy
- The renderer builds a `Texture2D` that matches the viewport resolution.
- `cellsPerPixel` increases as you zoom out, enabling aggregation.
- Alive cell density maps to brightness (greyscale) for distant zoom levels.

### Save Format
- JSON (`SaveMetadata`) stores world settings, tick, parameters, and achievements.
- Chunk data is stored in a companion binary file using a simple RLE-like encoding.
- Save versioning is tracked via `saveVersion`.

### Achievement Data Model
- Achievements are defined as **ScriptableObjects** with:
  - `id`, `displayName`, `description`, `daysRequired`, `medalName`
- The system is data-driven and expandable.

### Presets
- `Glider`, `Blinker`, `Random`, and `Empty` are included.

## Notes / Defaults
- Default constants are provided in `SimulationSettings` and can be tuned via UI at runtime.
- If something is unclear, sensible defaults are chosen and documented in code.

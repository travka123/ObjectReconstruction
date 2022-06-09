using System.Numerics;

namespace VoxelColoring;

public record VoxelCellInfo(Vector3 start, Vector3i size, float cellSize) { }

using System.Numerics;

namespace VoxelColoring;

public struct GridInfo
{
    public Vector3 start;
    public Vector3i size;
    public float cellSize;

    public GridInfo(Vector3 start, Vector3i size, float cellSize)
    {
        this.start = start;
        this.size = size;
        this.cellSize = cellSize;
    }
}

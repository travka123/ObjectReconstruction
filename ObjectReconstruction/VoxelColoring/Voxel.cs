using System.Numerics;

namespace VoxelColoring;

public struct Voxel<T>
{
    public Vector3 position;
    public T color;

    public Voxel(Vector3 position, T color)
    {
        this.position = position;
        this.color = color;
    }
}

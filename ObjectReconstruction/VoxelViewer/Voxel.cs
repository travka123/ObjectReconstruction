using OpenTK.Mathematics;

namespace VoxelViewer;

public struct Voxel
{
    public Vector3 position;
    public Vector3 color;

    public Voxel(Vector3 position, Vector3 color)
    {
        this.position = position;
        this.color = color;
    }
}

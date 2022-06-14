using System.Numerics;

namespace VoxelColoring;

public struct Vector3i
{
    public int X;
    public int Y;
    public int Z;

    public Vector3i(int x, int y, int z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public static Vector3 operator *(Vector3i left, float right)
    {
        return new Vector3(left.X * right, left.Y * right, left.Z * right);
    }
}

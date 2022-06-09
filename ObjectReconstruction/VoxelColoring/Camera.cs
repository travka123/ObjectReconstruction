using System.Numerics;

namespace VoxelColoring;

public record Camera(float fov, Vector3 position, Vector3 up, Vector3 direction) { }

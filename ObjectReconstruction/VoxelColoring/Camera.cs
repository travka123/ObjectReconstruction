using System.Numerics;

namespace VoxelColoring;

public struct Camera
{
    private static readonly Vector3 _up = new Vector3(0.0f, 1.0f, 0.0f);

    private float _fovy;
    private Vector3 _position;
    private Vector3 _direction;

    private Matrix4x4 _view;
    private Matrix4x4 _projection;

    private Matrix4x4 _VP;

    public Camera(float fovy, float aspect, Vector3 position, float pitch, float yaw)
    {
        _fovy = fovy;
        _position = position;

        _direction = position + Vector3.Normalize(new Vector3(
            (float)(Math.Cos(yaw) * Math.Cos(pitch)),
            (float)(Math.Sin(pitch)),
            (float)(Math.Sin(yaw) * Math.Cos(pitch))
        ));

        _view = Matrix4x4.CreateLookAt(position, new Vector3(0, 0, 0), _up);
        _projection = CreatePerspectiveFieldOfView(fovy, aspect);
        _VP = _view * _projection;

        //var test = Project(new Vector3(0.5f, 0.5f, 0.5f));
    }

    private static Matrix4x4 CreatePerspectiveFieldOfView(float fovy, float aspect)
    {
        const float NEAR = 0.1f;
        const float FAR = 100.0f;

        Matrix4x4 projection = new Matrix4x4();
        projection.M11 = (float)(1 / Math.Tan(fovy / 2) / aspect);
        projection.M22 = (float)(1 / Math.Tan(fovy / 2));
        projection.M33 = (NEAR + FAR) / (NEAR - FAR);
        projection.M34 = -1.0f;
        projection.M43 = -2 * FAR * NEAR / (FAR - NEAR);

        return projection;
    }

    public Vector3 Transform(Vector3 position)
    {
        var cliped = Vector4.Transform(new Vector4(position, 1.0f), _VP);
        return new Vector3(cliped.X, cliped.Y, cliped.Z) / cliped.W;
    }

    public Vector2 Project(Vector3 position)
    {
        var transformed = Transform(position);
        return new Vector2((transformed.X + 1) / 2, 1 - (transformed.Y + 1) / 2);
    }
}

using OpenTK.Mathematics;

namespace VoxelViewer;

internal class Camera
{
    private static readonly Vector3 _up = new Vector3(0.0f, 1.0f, 0.0f);

    private Vector3 _target = new Vector3(0.5f);
    public float Fovy { get; }
    public float Aspect { get; }

    public float Near { get; }
    public float Far { get; }

    private float _angleXZ = MathHelper.DegreesToRadians(90);
    private float _angleXY = MathHelper.DegreesToRadians(0);
    private float _radius = (float)Math.Sqrt(1.2f * 1.2f + 1.2f * 1.2f);

    private Matrix4 _view;
    private Matrix4 _projection;

    public Matrix4 VP { get; private set; }

    public Camera(float fovy, float aspect)
    {
        _view = Matrix4.LookAt(GetPosition(), _target, _up);

        Fovy = fovy;
        Aspect = aspect;
        Near = 0.1f;
        Far = 100.0f;
        Matrix4.CreatePerspectiveFieldOfView(fovy, aspect, Near, Far, out _projection);

        VP = _view * _projection;
    }

    public void Rotate(float angleX, float angleY)
    {
        _angleXZ += angleX;
        _angleXY += angleY;

        if (_angleXY > (float)Math.PI / 2 - 0.1f)
        {
            _angleXY = (float)Math.PI / 2 - 0.1f;
        }
        else if (_angleXY < -(float)Math.PI / 2 + 0.1f)
        {
            _angleXY = -(float)Math.PI / 2 + 0.1f;
        }
    }

    public void ChangeScale(float scaleDelta)
    {
        _radius += scaleDelta;

        if (_radius < 0.1f)
        {
            _radius = 0.1f;
        }
    }

    public void Update()
    {
        _view = Matrix4.LookAt(GetPosition(), _target, _up);
        VP = _view * _projection;
    }

    private Vector3 GetPosition()
    {
        return new Vector3(
           (float)(Math.Cos(_angleXZ) * Math.Cos(_angleXY)),
           (float)(Math.Sin(_angleXY)),
           (float)(Math.Sin(_angleXZ) * Math.Cos(_angleXY))
        ) * _radius + _target;
    }
}

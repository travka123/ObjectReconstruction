using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace VoxelViewer;

public class Viewer : GameWindow
{
    private Camera _camera;

    private int _VAO, _VBO;

    ShaderProgram shaderProgram = null!;

    float[] _vertices;

    private static Vector3[,,] corners = {
        {
            {
                new Vector3(-1,  1, -1),
                new Vector3(-1,  1,  1),
                new Vector3( 1,  1,  1),
            },
            {
                new Vector3( 1,  1,  1),
                new Vector3( 1,  1, -1),
                new Vector3(-1,  1, -1),
            }
        },
        {
            {
                new Vector3(-1,  1,  1),
                new Vector3( 1,  1,  1),
                new Vector3( 1, -1,  1),
            },
            {
                new Vector3( 1, -1,  1),
                new Vector3(-1, -1,  1),
                new Vector3(-1,  1,  1),
            }
        },
        {
            {
                new Vector3( 1,  1,  1),
                new Vector3( 1,  1, -1),
                new Vector3( 1, -1, -1),
            },
            {
                new Vector3( 1, -1, -1),
                new Vector3( 1, -1,  1),
                new Vector3( 1,  1,  1),
            }
        },
        {
            {
                new Vector3(-1,  1, -1),
                new Vector3(-1,  1,  1),
                new Vector3(-1, -1,  1),
            },
            {
                new Vector3(-1, -1,  1),
                new Vector3(-1, -1, -1),
                new Vector3(-1,  1, -1),
            }
        },
        {
            {
                new Vector3(-1,  1, -1),
                new Vector3( 1,  1, -1),
                new Vector3( 1, -1, -1),
            },
            {
                new Vector3( 1, -1, -1),
                new Vector3(-1, -1, -1),
                new Vector3(-1,  1, -1),
            }
        },
        {
            {
                new Vector3(-1, -1, -1),
                new Vector3( 1, -1, -1),
                new Vector3( 1, -1,  1),
            },
            {
                new Vector3( 1, -1,  1),
                new Vector3(-1, -1,  1),
                new Vector3(-1, -1, -1),
            }
        }
    };

    public Viewer(Voxel[] voxels, float voxelSize) : base(GameWindowSettings.Default, new NativeWindowSettings { Size = new Vector2i(720, 720) })
    {
        _camera = new Camera(MathHelper.DegreesToRadians(70), Size.X / Size.Y);

        const int squares = 6;
        const int trianglesInSquare = 2;
        const int verticesInTriangle = 3;
        const int floatsInVertices = 6;
        const int voxelOffset = squares * trianglesInSquare * verticesInTriangle * floatsInVertices;
        _vertices = new float[voxels.Length * voxelOffset];

        const int iOffset = voxelOffset;
        for (int i = 0; i < voxels.Length; i++)
        {
            const int jOffset = iOffset / squares;
            for (int j = 0; j < corners.GetLength(0); j++)
            {
                const int kOffset = jOffset / trianglesInSquare;
                for (int k = 0; k < corners.GetLength(1); k++)
                {
                    const int lOffset = kOffset / verticesInTriangle;
                    for (int l = 0; l < corners.GetLength(2); l++)
                    {
                        var position = voxels[i].position + (corners[j, k, l] * (voxelSize / 2));

                        _vertices[i * iOffset + j * jOffset + k * kOffset + l * lOffset + 0] = position.X;
                        _vertices[i * iOffset + j * jOffset + k * kOffset + l * lOffset + 1] = position.Y;
                        _vertices[i * iOffset + j * jOffset + k * kOffset + l * lOffset + 2] = position.Z;
                                 
                        _vertices[i * iOffset + j * jOffset + k * kOffset + l * lOffset + 3] = voxels[i].color.X;
                        _vertices[i * iOffset + j * jOffset + k * kOffset + l * lOffset + 4] = voxels[i].color.Y;
                        _vertices[i * iOffset + j * jOffset + k * kOffset + l * lOffset + 5] = voxels[i].color.Z;
                    }
                }
            }
        }
        Title = $"{voxels.Length} voxels";
    }

    protected override void OnLoad()
    {
        _VAO = GL.GenVertexArray();
        _VBO = GL.GenBuffer();

        GL.BindVertexArray(_VAO);
        GL.BindBuffer(BufferTarget.ArrayBuffer, _VBO);
        GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);

        GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));
        GL.EnableVertexAttribArray(1);

        shaderProgram = new ShaderProgram("..\\..\\..\\shaders\\shader.vert", "..\\..\\..\\shaders\\shader.frag");

        shaderProgram.Use();

        var MVP = _camera.VP;
        GL.UniformMatrix4(GL.GetUniformLocation(shaderProgram.Handle, "uMVP"), false, ref MVP);

        GL.Enable(EnableCap.DepthTest);

        GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);

        CursorState = CursorState.Grabbed;

        base.OnLoad();
    }

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        ProcessInput();

        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        var MVPAttribute = GL.GetUniformLocation(shaderProgram.Handle, "uMVP");
        var MVP = _camera.VP;
        GL.UniformMatrix4(MVPAttribute, false, ref MVP);

        GL.DrawArrays(PrimitiveType.Triangles, 0, _vertices.Length / 6);

        Context.SwapBuffers();

        base.OnUpdateFrame(args);
    }

    const float ROTATION_SPEED = 0.005f;
    const float SCROLL_SPEED = 0.05f;

    private void ProcessInput()
    {
        if (IsKeyPressed(OpenTK.Windowing.GraphicsLibraryFramework.Keys.Escape)) 
        {
            Close();
        }

        _camera.Rotate(MouseState.Delta.X * ROTATION_SPEED, MouseState.Delta.Y * ROTATION_SPEED);
        _camera.ChangeScale(-MouseState.ScrollDelta.Y * SCROLL_SPEED);
        _camera.Update();
    }

    protected override void OnUnload()
    {
        GL.BindVertexArray(0);

        GL.DeleteVertexArray(_VAO);
        GL.DeleteBuffer(_VBO);

        base.OnUnload();
    }

    protected override void OnResize(ResizeEventArgs e)
    {
        GL.Viewport(0, 0, e.Width, e.Height);

        base.OnResize(e);
    }
}
using OpenTK.Graphics.OpenGL;
using System.Text;

namespace VoxelViewer;

internal class ShaderProgram
{
    public readonly int Handle;

    public ShaderProgram(string vertexPath, string fragmentPath)
    {

        string VertexShaderSource;

        using (StreamReader reader = new StreamReader(vertexPath, Encoding.UTF8))
        {
            VertexShaderSource = reader.ReadToEnd();
        }

        string FragmentShaderSource;

        using (StreamReader reader = new StreamReader(fragmentPath, Encoding.UTF8))
        {
            FragmentShaderSource = reader.ReadToEnd();
        }

        int VertexShader = GL.CreateShader(ShaderType.VertexShader);
        GL.ShaderSource(VertexShader, VertexShaderSource);

        int FragmentShader = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(FragmentShader, FragmentShaderSource);

        GL.CompileShader(VertexShader);

        string infoLogVert = GL.GetShaderInfoLog(VertexShader);
        if (infoLogVert != String.Empty)
            Console.WriteLine(infoLogVert);

        GL.CompileShader(FragmentShader);

        string infoLogFrag = GL.GetShaderInfoLog(FragmentShader);

        if (infoLogFrag != String.Empty)
            Console.WriteLine(infoLogFrag);

        Handle = GL.CreateProgram();

        GL.AttachShader(Handle, VertexShader);
        GL.AttachShader(Handle, FragmentShader);

        GL.LinkProgram(Handle);

        GL.DetachShader(Handle, VertexShader);
        GL.DetachShader(Handle, FragmentShader);
        GL.DeleteShader(FragmentShader);
        GL.DeleteShader(VertexShader);
    }

    public void Use()
    {
        GL.UseProgram(Handle);
    }

    public void Dispose()
    {
        GL.DeleteProgram(Handle);
        GC.SuppressFinalize(this);
    }

    ~ShaderProgram()
    {
        GL.DeleteProgram(Handle);
    }
}
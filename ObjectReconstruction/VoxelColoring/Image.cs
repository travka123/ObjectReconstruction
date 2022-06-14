namespace VoxelColoring;

public struct Image<T>
{
    public int width;
    public int height;
    public Camera camera;
    public Func<int, int, T> getColorOf;

    public Image(int width, int height, Camera camera, Func<int, int, T> getColorOf)
    {
        this.width = width;
        this.height = height;
        this.camera = camera;
        this.getColorOf = getColorOf;
    }
}
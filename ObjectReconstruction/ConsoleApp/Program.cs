using Newtonsoft.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Numerics;
using VoxelColoring;
using VoxelViewer;

ObjectReconstructor.ConsistencyCheck<Rgb24> consistencyCheck = (ReadOnlySpan<Rgb24> colors) =>
{
    Span<Vector3> vcolors = stackalloc Vector3[colors.Length];
    Vector3 avarage = new Vector3();

    for (int i = 0; i < colors.Length; i++)
    {
        vcolors[i] = new Vector3(colors[i].R, colors[i].G, colors[i].B);
        avarage += vcolors[i];
    }

    avarage /= colors.Length;

    for (int i = 0; i < colors.Length; i++)
    {
        const int tolerance = 10;
        if ((Math.Abs(avarage.X - vcolors[i].X) > tolerance) ||
            (Math.Abs(avarage.Y - vcolors[i].Y) > tolerance) ||
            (Math.Abs(avarage.Z - vcolors[i].Z) > tolerance))
        {
            return false;
        }
    }

    return true;
};

Console.WriteLine("Hello, World!");
Console.Write("path to .conf file: ");
FileInfo fileInfo = new FileInfo(Console.ReadLine()!);

var jsonText = File.ReadAllText(fileInfo.FullName);
dynamic? set = JsonConvert.DeserializeObject(jsonText);

if (set is not null)
{
    var gridStart = new Vector3((float)set.grid.start[0], (float)set.grid.start[1], (float)set.grid.start[2]);
    var gridSize = new Vector3i((int)set.grid.size[0], (int)set.grid.size[1], (int)set.grid.size[2]);
    float voxelSize = (float)set.grid.voxelSize;

    var images = new List<VoxelColoring.Image<Rgb24>>();
    foreach (var imageInfo in set.images)
    {
        var image = Image.Load<Rgb24>(fileInfo.DirectoryName + "\\" + imageInfo.path);
        float fovy = imageInfo.fovy * (float)Math.PI / 180.0f;
        float aspect = (float)imageInfo.aspect;
        Vector3 position = new Vector3((float)imageInfo.position[0], (float)imageInfo.position[1], (float)imageInfo.position[2]);

        Func<int, int, Rgb24> getColor = (x, y) => image[x, y];
        images.Add(new VoxelColoring.Image<Rgb24>(image.Width, image.Height,
            new Camera(fovy, aspect, position, 0, 0), getColor));
    }

    var voxels = ObjectReconstructor.Reconstruct(new GridInfo(gridStart, gridSize, voxelSize), images.ToArray(), consistencyCheck);

    Console.WriteLine(voxels.Count);

    var tkVoxels = voxels.Select(v => new Voxel(
        new OpenTK.Mathematics.Vector3(v.position.X, v.position.Y, v.position.Z),
        new OpenTK.Mathematics.Vector3(v.color.R / (float)256, v.color.G / (float)256, v.color.B / (float)256))
    ).ToArray();

    using (Viewer viewer = new Viewer(tkVoxels, voxelSize))
    {
        viewer.VSync = OpenTK.Windowing.Common.VSyncMode.On;
        viewer.Run();
    }
}
else
{
    throw new FileNotFoundException();
}
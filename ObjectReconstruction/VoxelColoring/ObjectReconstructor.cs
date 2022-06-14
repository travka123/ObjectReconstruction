using System.Numerics;

namespace VoxelColoring;

public static class ObjectReconstructor
{
    public delegate bool ConsistencyCheck<T>(ReadOnlySpan<T> colors);

    public static List<Voxel<T>> Reconstruct<T>(GridInfo gridInfo, Image<T>[] images, ConsistencyCheck<T> consistencyCheck)
    {
        var voxels = new List<Voxel<T>>();

        var covered = new bool[images.Length][,];

        for (int i = 0; i < images.Length; i++)
        {
            covered[i] = new bool[images[i].width, images[i].height];
        }

        Vector3 position = new Vector3();
        int[] usedImages = new int[images.Length];
        T[] positionColors = new T[images.Length];

        float gridHeight = gridInfo.size.Y * gridInfo.cellSize;
        float gridZWidth = gridInfo.size.Z * gridInfo.cellSize;
        float gridXWidth = gridInfo.size.X * gridInfo.cellSize;
        float halfCellSize = gridInfo.cellSize / 2;

        for (position.Y = gridHeight - halfCellSize; position.Y > 0; position.Y -= gridInfo.cellSize)
        {
            for (position.Z = halfCellSize; position.Z < gridZWidth; position.Z += gridInfo.cellSize)
            {
                for (position.X = halfCellSize; position.X < gridXWidth; position.X += gridInfo.cellSize)
                {
                    int suitableImagesNumber = 0;

                    var modelPosition = position + gridInfo.start;

                    for (int i = 0; i < images.Length; i++)
                    {
                        Vector2 projection = images[i].camera.Project(modelPosition);

                        int pixelX = (int)(projection.X * images[i].width);
                        int pixelY = (int)(projection.Y * images[i].height);

                        if (!covered[i][pixelX, pixelY])
                        {
                            usedImages[suitableImagesNumber] = i;
                            positionColors[suitableImagesNumber] = images[i].getColorOf(pixelX, pixelY);
                            suitableImagesNumber++;
                        }
                    }

                    var positionColorsSlice = ((ReadOnlySpan<T>)positionColors).Slice(0, suitableImagesNumber);

                    if ((suitableImagesNumber > 1) && consistencyCheck(positionColorsSlice))
                    {
                        voxels.Add(new Voxel<T>(position, positionColors[0]));

                        for (int i = 0; i < suitableImagesNumber; i++)
                        {
                            var image = images[usedImages[i]];

                            var imageSize = new Vector2(image.width, image.height);

                            Vector2 P000 = image.camera.Project(modelPosition + new Vector3(-halfCellSize, -halfCellSize, -halfCellSize)) * imageSize;
                            Vector2 P001 = image.camera.Project(modelPosition + new Vector3(-halfCellSize, -halfCellSize,  halfCellSize)) * imageSize;
                            Vector2 P010 = image.camera.Project(modelPosition + new Vector3(-halfCellSize,  halfCellSize, -halfCellSize)) * imageSize;
                            Vector2 P011 = image.camera.Project(modelPosition + new Vector3(-halfCellSize,  halfCellSize,  halfCellSize)) * imageSize;
                            Vector2 P100 = image.camera.Project(modelPosition + new Vector3( halfCellSize, -halfCellSize, -halfCellSize)) * imageSize;
                            Vector2 P101 = image.camera.Project(modelPosition + new Vector3( halfCellSize, -halfCellSize,  halfCellSize)) * imageSize;
                            Vector2 P110 = image.camera.Project(modelPosition + new Vector3( halfCellSize,  halfCellSize, -halfCellSize)) * imageSize;
                            Vector2 P111 = image.camera.Project(modelPosition + new Vector3( halfCellSize,  halfCellSize,  halfCellSize)) * imageSize;

                            bool[,] imageCover = covered[usedImages[i]];
                            FillTriangle(imageCover, P000, P010, P001);
                            FillTriangle(imageCover, P011, P010, P001);
                            FillTriangle(imageCover, P011, P101, P001);
                            FillTriangle(imageCover, P100, P101, P111);
                            FillTriangle(imageCover, P100, P110, P111);
                            FillTriangle(imageCover, P100, P110, P000);
                            FillTriangle(imageCover, P100, P110, P010);
                            FillTriangle(imageCover, P011, P110, P010);
                            FillTriangle(imageCover, P111, P110, P010);
                        }
                    }
                }
            }
        }

        return voxels;
    }


    // TODO: redo completely
    // TODO_START

    private static void FillTriangle(bool[,] field, Vector2 p1, Vector2 p2, Vector2 p3)
    {
        int top = (int)(Math.Min(Math.Min(p1.Y, p2.Y), p3.Y));
        int left = (int)(Math.Min(Math.Min(p1.X, p2.X), p3.X));
        int bottom = (int)(Math.Max(Math.Max(p1.Y, p2.Y), p3.Y));
        int right = (int)(Math.Max(Math.Max(p1.X, p2.X), p3.X));

        for (int y = top; y < bottom; y++)
        {
            for (int x = left; x < right; x++)
            {
                //if (PointInTriangle(new Vector2(x, y), p1, p2, p3))
                //{
                    field[x, y] = true;
                //}
            }
        }
    }

    private static bool PointInTriangle(Vector2 pt, Vector2 v1, Vector2 v2, Vector2 v3)
    {
        float d1, d2, d3;
        bool has_neg, has_pos;

        d1 = sign(pt, v1, v2);
        d2 = sign(pt, v2, v3);
        d3 = sign(pt, v3, v1);

        has_neg = (d1 < 0) || (d2 < 0) || (d3 < 0);
        has_pos = (d1 > 0) || (d2 > 0) || (d3 > 0);

        return !(has_neg && has_pos);
    }

    private static float sign(Vector2 p1, Vector2 p2, Vector2 p3)
    {
        return (p1.X - p3.X) * (p2.Y - p3.Y) - (p2.X - p3.X) * (p1.Y - p3.Y);
    }

    // TODO_END
    
}

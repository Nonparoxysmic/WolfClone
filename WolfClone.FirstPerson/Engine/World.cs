using System;
using Microsoft.Xna.Framework;
using Vector3 = Microsoft.Xna.Framework.Vector3;

namespace WolfClone.FirstPerson.Engine;

internal class World
{
    public Player Player { get; }
    public GridTile Level { get; private set; }

    public Color[][] PlaceholderTextures { get; set; }

    public World()
    {
        Player = new(1.5f, 1.5f);

        int width = 16;
        int height = 16;
        Level = new GridTile(0, 0, null, 4);
        Random rand = new();
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (rand.NextDouble() < 0.5)
                {
                    Level[x, y] = new EmptyTile(x, y, Level);
                }
                else if (x == 0 || y == 0 || x == width - 1 || y == height - 1)
                {
                    Level[x, y] = new BlockTile(x, y, Level, rand.Next(4) + 1);
                }
                else if (2 < x && x < width - 3 && 2 < y && y < height - 3)
                {
                    Level[x, y] = new BlockTile(x, y, Level, rand.Next(4) + 1);
                }
                else
                {
                    Level[x, y] = new EmptyTile(x, y, Level);
                }
            }
        }
    }

    public Color CastRay(Vector3 position, Vector3 direction, float length = 32)
    {
        float x = position.X;
        float y = position.Y;
        if (x < 0 || y < 0 || x >= Level.Width || y >= Level.Height)
        {
            // Player is outside the level
            return Color.Black;
        }
        Vector3 levelPosition = new(position.X / Level.Width, position.Y / Level.Height, position.Z);
        Tile current = Level.GetTile(levelPosition, out Vector3 tilePosition);
        return current.CastRay(tilePosition, direction, length);
    }
}

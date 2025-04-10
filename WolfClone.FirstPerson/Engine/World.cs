using System;

namespace WolfClone.FirstPerson.Engine;

internal class World
{
    public Player Player { get; }
    public int[,] Level { get; private set; }

    public World()
    {
        Player = new(1.5f, 1.5f);

        int width = 16;
        int height = 16;
        Level = new int[width, height];
        Random rand = new();
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (x == 0 || y == 0 || x == width - 1 || y == height - 1)
                {
                    Level[x, y] = rand.Next(2);
                    continue;
                }
                if (2 < x && x < width - 3 && 2 < y && y < height - 3)
                {
                    Level[x, y] = rand.Next(2);
                }
            }
        }
    }
}

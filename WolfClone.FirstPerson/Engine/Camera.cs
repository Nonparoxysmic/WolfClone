using System;
using Microsoft.Xna.Framework;
using Vector2D = Microsoft.Xna.Framework.Vector2;

namespace WolfClone.FirstPerson.Engine;

internal class Camera(World world)
{
    public float FOV { get; private set; } = 60; // Horizontal field of view in degrees
    public int Width { get; private set; } = 160; // Camera view width in pixels
    public int Height { get; private set; } = 144; // Camera view height in pixels

    public Vector2D Position => _world.Player.Position;
    public Vector2D Direction => _world.Player.Direction;

    private readonly World _world = world;

    private Vector2D _plane;
    public Vector2D Plane_DEBUG => _plane; // TEMPORARY

    public Color[] CaptureFrame()
    {
        UpdatePlaneVector();

        // TODO: do the raycasting here

        // Temporary data
        Color[] pixels = new Color[Width * Height];
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                int i = x + Width * y;
                if ((x + y) % 2 == 0)
                {
                    pixels[i] = Color.White;
                }
                else
                {
                    pixels[i] = Color.Black;
                }
            }
        }
        return pixels;
    }

    private void UpdatePlaneVector()
    {
        float magnitude = MathF.Tan(FOV / 2 * (MathF.PI / 180));
        _plane = new Vector2D(-Direction.Y * magnitude, Direction.X * magnitude);
    }
}

using System;
using Microsoft.Xna.Framework;
using Vector3 = Microsoft.Xna.Framework.Vector3;

namespace WolfClone.FirstPerson.Engine;

internal class Camera
{
    public float FOV { get; private set; } = 60; // Horizontal field of view in degrees
    public int Width { get; private set; } // Camera view width in pixels
    public int Height { get; private set; } // Camera view height in pixels

    public Vector3 Position => _world.Player.Position;
    public Vector3 Direction => _world.Player.Direction;

    private readonly World _world;

    private Vector3 _planeRight;
    private Vector3 _planeDown;
    private Color[] _pixels;

    public Camera(int width, int height, World world)
    {
        Width = width;
        Height = height;
        _world = world;
        _pixels = new Color[Width * Height];

        float vertFOV = FOV * Height / Width;
        float magnitude = MathF.Tan(vertFOV / 2 * (MathF.PI / 180));
        _planeDown = new Vector3(0, 0, magnitude);
    }

    public Color[] CaptureFrame()
    {
        UpdateHorzPlaneVector();
        for (int x = 0; x < Width; x++)
        {
            float horz = 2.0f * x / Width - 1;
            for (int y = 0; y < Height; y++)
            {
                float vert = 2.0f * y / Height - 1;
                Vector3 rayDirection = Direction + _planeRight * horz + _planeDown * vert;
                rayDirection = Vector3.Normalize(rayDirection);
                Color pixelColor = _world.CastRay(Position, rayDirection);
                _pixels[x + Width * y] = pixelColor;
            }
        }
        return _pixels;
    }

    private void UpdateHorzPlaneVector()
    {
        float magnitude = MathF.Tan(FOV / 2 * (MathF.PI / 180));
        _planeRight = new Vector3(-Direction.Y * magnitude, Direction.X * magnitude, 0);
    }
}

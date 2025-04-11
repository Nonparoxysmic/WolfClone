using System;
using Microsoft.Xna.Framework;
using Vector2D = Microsoft.Xna.Framework.Vector2;

namespace WolfClone.FirstPerson.Engine;

internal class Camera(int width, int height, World world)
{
    public float FOV { get; private set; } = 60; // Horizontal field of view in degrees
    public int Width { get; private set; } = width; // Camera view width in pixels
    public int Height { get; private set; } = height; // Camera view height in pixels

    public Vector2D Position => _world.Player.Position;
    public Vector2D Direction => _world.Player.Direction;

    // TEMPORARY
    private readonly Color[] _wallColors =
    [
        Color.Black,
        Color.Red,
        Color.MonoGameOrange,
        Color.Yellow,
        Color.Green,
        Color.Blue,
        Color.Purple,
        Color.Magenta,
    ];

    private readonly World _world = world;

    private Vector2D _plane;

    public Color[] CaptureFrame()
    {
        UpdatePlaneVector();

        Color[] pixels = new Color[Width * Height];
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height / 2; y++)
            {
                int i = x + Width * y;
                pixels[i] = Color.CornflowerBlue;
            }
            for (int y = Height / 2; y < Height; y++)
            {
                int i = x + Width * y;
                pixels[i] = Color.SaddleBrown;
            }
        }
        for (int x = 0; x < Width; x++)
        {
            float horz = 2.0f * x / Width - 1;
            Vector2D rayDirection = Direction + _plane * horz;
            Vector2D tilePosition = Vector2D.Floor(Position);
            Vector2D delta = new
            (
                (rayDirection.X == 0) ? float.MaxValue : Math.Abs(1 / rayDirection.X),
                (rayDirection.Y == 0) ? float.MaxValue : Math.Abs(1 / rayDirection.Y)
            );
            Vector2D step;
            Vector2D sideDist;
            if (rayDirection.X < 0)
            {
                step.X = -1;
                sideDist.X = (Position.X - tilePosition.X) * delta.X;
            }
            else
            {
                step.X = 1;
                sideDist.X = (tilePosition.X + 1 - Position.X) * delta.X;
            }
            if (rayDirection.Y < 0)
            {
                step.Y = -1;
                sideDist.Y = (Position.Y - tilePosition.Y) * delta.Y;
            }
            else
            {
                step.Y = 1;
                sideDist.Y = (tilePosition.Y + 1 - Position.Y) * delta.Y;
            }


            bool hit = false;
            int side = 0; // 0 for x-side hit, 1 for y-side hit
            int timeout = 100;
            int hitColorIndex = 0;
            while (!hit && timeout > 0)
            {
                timeout--;

                // Jump to next map square in either x-direction or y-direction
                if (sideDist.X < sideDist.Y)
                {
                    sideDist.X += delta.X;
                    tilePosition.X += step.X;
                    side = 0;
                }
                else
                {
                    sideDist.Y += delta.Y;
                    tilePosition.Y += step.Y;
                    side = 1;
                }

                // Check if a valid wall was hit (0 is empty and > 0 is a wall)
                if (_world.GetTile((int)tilePosition.X, (int)tilePosition.Y) > 0)
                {
                    hit = true;
                    hitColorIndex = _world.GetTile((int)tilePosition.X, (int)tilePosition.Y);
                }
            }

            if (!hit)
            {
                continue;
            }

            double perpWallDist;
            if (side == 0)
                perpWallDist = (tilePosition.X - Position.X + (1 - step.X) / 2.0) / rayDirection.X;
            else
                perpWallDist = (tilePosition.Y - Position.Y + (1 - step.Y) / 2.0) / rayDirection.Y;

            double fovRad = FOV * Math.PI / 180.0;
            double projDistance = Width / (2.0 * Math.Tan(fovRad / 2));
            int lineHeight = (int)(projDistance / perpWallDist);
            int drawStart = -lineHeight / 2 + Height / 2;
            if (drawStart < 0) drawStart = 0;
            int drawEnd = lineHeight / 2 + Height / 2;
            if (drawEnd >= Height) drawEnd = Height - 1;

            // Debug color for the wall
            Color wallColor = _wallColors[hitColorIndex];
            if (side == 1)
                wallColor = new(wallColor * 0.667f, 1.0f);

            // Draw the vertical stripe for the wall slice.
            for (int y = drawStart; y <= drawEnd; y++)
            {
                int i = x + Width * y;
                pixels[i] = wallColor;
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

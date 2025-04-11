using System;
using Microsoft.Xna.Framework;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace WolfClone.FirstPerson.Engine;

internal class Camera(int width, int height, World world)
{
    public float FOV { get; private set; } = 60; // Horizontal field of view in degrees
    public int Width { get; private set; } = width; // Camera view width in pixels
    public int Height { get; private set; } = height; // Camera view height in pixels

    public Vector2 Position => _world.Player.Position;
    public Vector2 Direction => _world.Player.Direction;

    private readonly World _world = world;

    private Vector2 _plane;

    public Color[] CaptureFrame()
    {
        UpdatePlaneVector();

        // Clear the screen
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

        // Render the view
        for (int x = 0; x < Width; x++)
        {
            float horz = 2.0f * x / Width - 1;
            Vector2 rayDirection = Direction + _plane * horz;
            Vector2 tilePosition = Vector2.Floor(Position);
            Vector2 delta = new
            (
                (rayDirection.X == 0) ? float.MaxValue : Math.Abs(1 / rayDirection.X),
                (rayDirection.Y == 0) ? float.MaxValue : Math.Abs(1 / rayDirection.Y)
            );
            Vector2 step;
            Vector2 sideDist;
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

            // Texture Mapping:
            
            // Retrieve texture for this tile index.
            Color[] wallTexture = _world.PlaceholderTextures[hitColorIndex];
            int texWidth = 64;
            int texHeight = 64;

            // Calculate the exact point where the wall was hit.
            double wallX;
            if (side == 0)
                wallX = Position.Y + perpWallDist * rayDirection.Y;
            else
                wallX = Position.X + perpWallDist * rayDirection.X;
            wallX -= Math.Floor(wallX);

            // Determine the horizontal coordinate on the texture.
            int texX = (int)(wallX * texWidth);
            if ((side == 0 && rayDirection.X > 0) || (side == 1 && rayDirection.Y < 0))
            {
                texX = texWidth - texX - 1;
            }

            // How much to increase the texture coordinate per screen pixel in the stripe.
            double texStep = (double)texHeight / lineHeight;
            // Starting texture coordinate for the current stripe.
            double texPos = (drawStart - Height / 2 + lineHeight / 2) * texStep;

            // Iterate over each pixel of the stripe and sample from the texture.
            for (int y = drawStart; y <= drawEnd; y++)
            {
                int texY = (int)texPos;
                if (texY < 0) texY = 0;
                if (texY >= texHeight) texY = texHeight - 1;
                texPos += texStep;
                Color color = wallTexture[texX + 64 * texY];
                if (side == 1)
                    color = new Color(color * 0.667f, 1.0f);
                int i = x + Width * y;
                pixels[i] = color;
            }
        }
        return pixels;
    }

    private void UpdatePlaneVector()
    {
        float magnitude = MathF.Tan(FOV / 2 * (MathF.PI / 180));
        _plane = new Vector2(-Direction.Y * magnitude, Direction.X * magnitude);
    }
}

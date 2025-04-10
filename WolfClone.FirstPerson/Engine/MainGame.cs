using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace WolfClone.FirstPerson.Engine;

public class MainGame : Game
{
    private World _world;
    private Camera _camera;

    private readonly GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    private Texture2D _pixel_DEBUG; // TEMPORARY

    public MainGame()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        Window.ClientSizeChanged += new EventHandler<EventArgs>(OnWindowClientSizeChanged);
        Window.AllowUserResizing = true;
        Window.Title = "WolfClone development";
    }

    protected override void Initialize()
    {
        _world = new();
        _camera = new(_world);

        _pixel_DEBUG = new Texture2D(_graphics.GraphicsDevice, 1, 1);
        _pixel_DEBUG.SetData([Color.White]);

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
    }

    protected override void Update(GameTime gameTime)
    {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        var directionalInput = Vector2.Zero;
        KeyboardState keyboardState = Keyboard.GetState();
        if (keyboardState.IsKeyDown(Keys.Up) || keyboardState.IsKeyDown(Keys.W))
        {
            directionalInput.Y -= 1;
        }
        if (keyboardState.IsKeyDown(Keys.Down) || keyboardState.IsKeyDown(Keys.S))
        {
            directionalInput.Y += 1;
        }
        if (keyboardState.IsKeyDown(Keys.Left) || keyboardState.IsKeyDown(Keys.A))
        {
            directionalInput.X -= 1;
        }
        if (keyboardState.IsKeyDown(Keys.Right) || keyboardState.IsKeyDown(Keys.D))
        {
            directionalInput.X += 1;
        }

        _world.Player.DebugMovement(deltaTime, directionalInput);

        // TODO: Add update logic here

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        Texture2D frame = new(_graphics.GraphicsDevice, _camera.Width, _camera.Height);
        Color[] pixels = _camera.CaptureFrame();
        frame.SetData(pixels);
        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        _spriteBatch.Draw(frame, new Rectangle(0, 0, _camera.Width * 2, _camera.Height * 2), Color.White);
        _spriteBatch.End();

        // TEST RAYCASTER
        int screenWidth = Window.ClientBounds.Width;
        int screenHeight = Window.ClientBounds.Height;
        var player = _world.Player;
        int mapWidth = _world.Level.GetLength(0);
        int mapHeight = _world.Level.GetLength(1);
        _spriteBatch.Begin();
        // Loop over every vertical stripe of the screen
        for (int x = 0; x < screenWidth; x++)
        {
            // Calculate ray position and direction:
            // cameraX ranges from -1 (left side) to 1 (right side)
            double cameraX = 2 * x / (double)screenWidth - 1;
            double rayDirX = player.Direction.X + _camera.Plane_DEBUG.X * cameraX;
            double rayDirY = player.Direction.Y + _camera.Plane_DEBUG.Y * cameraX;

            // Which box of the map we're in
            int mapX = (int)player.Position.X;
            int mapY = (int)player.Position.Y;

            // Length of the ray from one x or y-side to next x or y-side
            double deltaDistX = (rayDirX == 0) ? 1e30 : Math.Abs(1 / rayDirX);
            double deltaDistY = (rayDirY == 0) ? 1e30 : Math.Abs(1 / rayDirY);

            // Calculate step direction and initial sideDist
            int stepX, stepY;
            double sideDistX, sideDistY;
            if (rayDirX < 0)
            {
                stepX = -1;
                sideDistX = (player.Position.X - mapX) * deltaDistX;
            }
            else
            {
                stepX = 1;
                sideDistX = (mapX + 1.0 - player.Position.X) * deltaDistX;
            }
            if (rayDirY < 0)
            {
                stepY = -1;
                sideDistY = (player.Position.Y - mapY) * deltaDistY;
            }
            else
            {
                stepY = 1;
                sideDistY = (mapY + 1.0 - player.Position.Y) * deltaDistY;
            }

            bool hit = false;
            int side = 0; // 0 for x-side hit, 1 for y-side hit
            while (!hit)
            {
                // Jump to next map square in either x-direction or y-direction
                if (sideDistX < sideDistY)
                {
                    sideDistX += deltaDistX;
                    mapX += stepX;
                    side = 0;
                }
                else
                {
                    sideDistY += deltaDistY;
                    mapY += stepY;
                    side = 1;
                }

                // If the ray goes outside of the map array bounds, break out of the loop.
                if (mapX < 0 || mapX >= mapWidth || mapY < 0 || mapY >= mapHeight)
                {
                    hit = false;
                    break;
                }

                // Check if a valid wall was hit (0 is empty and > 0 is a wall)
                if (_world.Level[mapX, mapY] > 0)
                {
                    hit = true;
                }
            }

            // If no wall was hit due to going out of bounds, skip drawing this column.
            if (!hit)
            {
                continue;
            }

            // Calculate distance projected on camera direction (avoid fish-eye effect)
            double perpWallDist;
            if (side == 0)
                perpWallDist = (mapX - player.Position.X + (1 - stepX) / 2.0) / rayDirX;
            else
                perpWallDist = (mapY - player.Position.Y + (1 - stepY) / 2.0) / rayDirY;

            // Calculate height of the line to draw on the screen:

            // Calculate the distance to the projection plane based on the horizontal FOV.
            double fovRad = _camera.FOV * Math.PI / 180.0;
            double projDistance = screenWidth / (2.0 * Math.Tan(fovRad / 2));

            // Then, later when rendering a wall that’s been hit with perpendicular distance "perpWallDist":
            int lineHeight = (int)(projDistance / perpWallDist);  // Each wall is 1 unit tall

            // Calculate lowest and highest pixel to fill in for current stripe
            int drawStart = -lineHeight / 2 + screenHeight / 2;
            if (drawStart < 0) drawStart = 0;
            int drawEnd = lineHeight / 2 + screenHeight / 2;
            if (drawEnd >= screenHeight) drawEnd = screenHeight - 1;

            // Debug color for the wall
            Color wallColor = Color.Red;
            if (side == 1)
                wallColor = Color.DarkRed;

            // Draw the vertical stripe for the wall slice, using a 1-pixel-wide texture scaled vertically.
            _spriteBatch.Draw(_pixel_DEBUG, new Rectangle(x, drawStart, 1, drawEnd - drawStart), wallColor);
        }
        _spriteBatch.End();

        base.Draw(gameTime);
    }

    private void OnWindowClientSizeChanged(object sender, EventArgs e)
    {
        // TODO: Make changes to handle the new window size.            
    }
}

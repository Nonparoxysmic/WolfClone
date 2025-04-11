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
        _graphics.PreferredBackBufferWidth = 640;
        _graphics.PreferredBackBufferHeight = 400;
        _graphics.ApplyChanges();

        _world = new();
        _camera = new(320, 200, _world);

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
        GraphicsDevice.Clear(Color.Gray);

        Texture2D frame = new(_graphics.GraphicsDevice, _camera.Width, _camera.Height);
        Color[] pixels = _camera.CaptureFrame();
        frame.SetData(pixels);
        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        _spriteBatch.Draw(frame, new Rectangle(0, 0, _camera.Width * 2, _camera.Height * 2), Color.White);
        _spriteBatch.End();

        base.Draw(gameTime);
    }

    private void OnWindowClientSizeChanged(object sender, EventArgs e)
    {
        // TODO: Make changes to handle the new window size.            
    }
}

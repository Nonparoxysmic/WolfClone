using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Vector3 = Microsoft.Xna.Framework.Vector3;

namespace WolfClone.FirstPerson.Engine;

public class MainGame : Game
{
    private World _world;
    private Camera _camera;
    private Texture2D _frame;

    private readonly GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    public MainGame()
    {
        _graphics = new GraphicsDeviceManager(this)
        {
            PreferredBackBufferWidth = 640,
            PreferredBackBufferHeight = 400
        };
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        Window.ClientSizeChanged += new EventHandler<EventArgs>(OnWindowClientSizeChanged);
        Window.AllowUserResizing = true;
        Window.Title = "WolfClone development";
    }

    protected override void Initialize()
    {
        _world = new();
        _camera = new(320, 200, _world);
        _frame = new(_graphics.GraphicsDevice, _camera.Width, _camera.Height);

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        _world.PlaceholderTextures = new Color[5][];

        Texture2D gray = Texture2D.FromFile(GraphicsDevice, "Assets/Textures/Placeholder/placeholder_gray.png");
        Color[] grayColors = new Color[64 * 64];
        gray.GetData(grayColors);
        _world.PlaceholderTextures[0] = grayColors;
        _world.PlaceholderTextures[1] = grayColors;

        Texture2D blue = Texture2D.FromFile(GraphicsDevice, "Assets/Textures/Placeholder/placeholder_blue.png");
        Color[] blueColors = new Color[64 * 64];
        blue.GetData(blueColors);
        _world.PlaceholderTextures[2] = blueColors;

        Texture2D wood = Texture2D.FromFile(GraphicsDevice, "Assets/Textures/Placeholder/placeholder_wood.png");
        Color[] woodColors = new Color[64 * 64];
        wood.GetData(woodColors);
        _world.PlaceholderTextures[3] = woodColors;

        Texture2D brick = Texture2D.FromFile(GraphicsDevice, "Assets/Textures/Placeholder/placeholder_brick.png");
        Color[] brickColors = new Color[64 * 64];
        brick.GetData(brickColors);
        _world.PlaceholderTextures[4] = brickColors;
    }

    protected override void Update(GameTime gameTime)
    {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        Vector3 directionalInput = Vector3.Zero;
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

        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        Color[] pixels = _camera.CaptureFrame();
        _frame.SetData(pixels);
        Rectangle destinationRectangle = new(0, 0, _camera.Width * 2, _camera.Height * 2);
        _spriteBatch.Draw(_frame, destinationRectangle, Color.White);
        _spriteBatch.End();

        base.Draw(gameTime);
    }

    private void OnWindowClientSizeChanged(object sender, EventArgs e)
    {
        // TODO: Make changes to handle the new window size.            
    }
}

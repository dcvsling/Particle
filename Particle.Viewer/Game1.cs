using Console.Core;
using Console.MonoGameUI;
using GUI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SampleGame;

public class Game1 : Game
{
    private readonly GraphicsDeviceManager _gdm;
    private SpriteBatch? _batch;
    private InGameConsoleComponent? _console;
    private SpriteFont? _font;

    public Game1()
    {
        _gdm = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }
    protected override void Initialize()
    {
        base.Initialize();
        Window.BeginScreenDeviceChange(false);
        Window.EndScreenDeviceChange(Window.ScreenDeviceName, 800, 600);
    }


    protected override void LoadContent()
    {
        _batch = new SpriteBatch(GraphicsDevice);
        Components.Add(new InGameConsoleComponent(this, Theme.Default.Font, ConsoleHost.Default));
        base.LoadContent();
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);
        base.Draw(gameTime);
    }

    public static void Main() => new Game1().Run();
}

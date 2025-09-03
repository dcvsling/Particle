using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GUI;

public abstract class Control
{
    public Rectangle Bounds { get; set; }
    public bool Visible { get; set; } = true;
    public object? Tag { get; set; }
    protected Control(Rectangle bounds) => Bounds = bounds;
    public abstract void Update(InputState input);
    public abstract void Draw(SpriteBatch spriteBatch);
}
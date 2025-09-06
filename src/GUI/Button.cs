using System;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GUI;

public class Button : Control
{
    private readonly string _text;
    public event EventHandler? Clicked;

    public Button(Rectangle bounds, string text) : base(bounds) => _text = text;

    public override void Update(InputState input)
    {
        if (!Visible) return;
        if (input.IsLeftMouseClicked && Bounds.Contains(input.MousePosition))
            Clicked?.Invoke(this, EventArgs.Empty);
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        if (!Visible) return;
        spriteBatch.Draw(Renderer.Pixel, Bounds, Theme.Default.ControlBackgroundColor);
        Renderer.Font = Theme.Default.Font.GetFont(14f);
        if (Renderer.Font is { })
        {
            Vector2 size = Renderer.Font.MeasureString(_text);
            var position = new Vector2(
                Bounds.X + (Bounds.Width - size.X) / 2,
                Bounds.Y + (Bounds.Height - size.Y) / 2);
            spriteBatch.DrawString(Renderer.Font, _text, position, Theme.Default.ControlForegroundColor);
        }
    }
}
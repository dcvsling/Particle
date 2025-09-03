using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using GUI.Events;

namespace GUI;

public class Slider : Control
{
    public string Label { get; set; }
    private readonly float _min, _max;
    private bool _drag;
    private float _value;

    public float Value
    {
        get => _value;
        set
        {
            if (_value != value)
            {
                var old = _value;
                _value = value;
                ValueChanged?.Invoke(this, new ValueChangedEventArgs<float>(old, _value));
            }
        }
    }

    public event EventHandler<ValueChangedEventArgs<float>>? ValueChanged;

    public Slider(Rectangle bounds, float min, float max, float initial, string label)
        : base(bounds)
    {
        _min = min;
        _max = max;
        _value = initial;
        Label = label;
    }

    public override void Update(InputState input)
    {
        if (!Visible) return;
        if (input.Current.LeftButton == ButtonState.Pressed && input.Previous.LeftButton == ButtonState.Released
            && Bounds.Contains(input.MousePosition))
        {
            _drag = true;
        }
        if (input.Current.LeftButton == ButtonState.Released)
        {
            _drag = false;
        }
        if (_drag)
        {
            float t = (input.MousePosition.X - Bounds.X) / (float)Bounds.Width;
            t = MathHelper.Clamp(t, 0f, 1f);
            Value = _min + t * (_max - _min);
        }
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        if (!Visible) return;
        spriteBatch.Draw(Renderer.Pixel, Bounds, Theme.Default.ControlBackgroundColor);
        float t = (_value - _min) / (_max - _min);
        var handleX = Bounds.X + (int)(t * Bounds.Width) - 5;
        var hr = new Rectangle(handleX, Bounds.Y - 5, 10, Bounds.Height + 10);
        spriteBatch.Draw(Renderer.Pixel, hr, Theme.Default.ControlForegroundColor);
        if (Renderer.Font is { })
        {
            spriteBatch.DrawString(Renderer.Font, $"{Label}:{Value:0.##}",
                new Vector2(Bounds.X, Bounds.Y - Renderer.Font.LineSpacing), Theme.Default.ControlForegroundColor);
        }
    }
}
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GUI;

public class InputState
{
    public MouseState Previous { get; set; }
    public MouseState Current { get; set; }
    public bool IsLeftMouseClicked => Current.LeftButton == ButtonState.Pressed && Previous.LeftButton == ButtonState.Released;
    public int ScrollWheelDelta => Current.ScrollWheelValue - Previous.ScrollWheelValue;
    public Point MousePosition => Current.Position;
    public KeyboardState Keyboard => Microsoft.Xna.Framework.Input.Keyboard.GetState();
    public void Update() => (Previous, Current) = (Current, Mouse.GetState());
}
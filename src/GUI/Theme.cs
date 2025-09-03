using Microsoft.Xna.Framework;

namespace GUI;

public class Theme
{
    public Color BackgroundColor { get; set; }
    public Color ForegroundColor { get; set; }
    public Color ControlBackgroundColor { get; set; }
    public Color ControlForegroundColor { get; set; }
    public static Theme Default => new Theme
    {
        BackgroundColor = Color.Transparent,
        ForegroundColor = Color.White,
        ControlBackgroundColor = Color.DarkGray,
        ControlForegroundColor = Color.White
    };
}
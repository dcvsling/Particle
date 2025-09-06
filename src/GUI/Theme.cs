using System.IO;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GUI;

public class Theme
{
    public Color BackgroundColor { get; set; }
    public Color ForegroundColor { get; set; }
    public Color ControlBackgroundColor { get; set; }
    public Color ControlForegroundColor { get; set; }
    public FontSystem Font { get; set; } = CreateFont();
    public static Theme Default => new Theme
    {
        BackgroundColor = Color.Transparent,
        ForegroundColor = Color.White,
        ControlBackgroundColor = Color.DarkGray,
        ControlForegroundColor = Color.White
    };

    private static FontSystem CreateFont()
        => Directory.GetFiles(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Fonts"))
            .Where(p => Path.GetExtension(p) == ".ttf")
            .Aggregate(new FontSystem(), (system, font) =>
            {
                system.AddFont(File.ReadAllBytes(font));
                return system;
            });
}
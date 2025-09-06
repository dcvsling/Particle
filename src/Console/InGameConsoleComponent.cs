using System;
using System.Collections.Generic;

using Console.Abstractions;
using Console.Commands;
using Console.Core;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Console.MonoGameUI;

/// <summary>
/// 可直接加入到 MonoGame 的 DrawableGameComponent，提供簡易 Console 視窗與輸入。
/// 依賴 SpriteFont，請於 Content Pipeline 建立一個 DefaultFont.spritefont 並 Load 後呼叫 LoadFont。
/// </summary>
public sealed class InGameConsoleComponent(Game game, FontSystem font, ConsoleHost host)  : DrawableGameComponent(game)
{
    private readonly ConsoleHost _host = host;
    private readonly List<string> _logs = new();
    private SpriteBatch? _batch = new SpriteBatch(game.GraphicsDevice);
    private FontSystem? _font = font;
    private string _input = string.Empty;
    private bool _isOpen = true;
    private KeyboardState _prev;

    public void Register(IConsoleCommand command) => _host.Register(command);

    public override void Update(GameTime gameTime)
    {
        var ks = Keyboard.GetState();

        if (Pressed(ks, Keys.OemTilde))
            _isOpen = !_isOpen;
        if (_isOpen)
        {
            if (Pressed(ks, Keys.Enter))
            {
                var line = _input.Trim();
                if (!string.IsNullOrEmpty(line))
                {
                    Append($"> {line}");
                    try
                    {
                        var result = _host.ExecuteLine(line);
                        if (!string.IsNullOrWhiteSpace(result)) Append(result);
                    }
                    catch (Exception ex)
                    {
                        Append($"錯誤：{ex.Message}");
                    }
                }
                _input = string.Empty;
            }
            else if (Pressed(ks, Keys.Back))
            {
                if (_input.Length > 0) _input = _input[..^1];
            }
            else
            {
                foreach (var k in ks.GetPressedKeys())
                {
                    if (!_prev.IsKeyDown(k))
                    {
                        var ch = ToChar(k, ks);
                        if (ch != '\0') _input += ch;
                        else if (k == Keys.Space) _input += ' ';
                    }
                }
            }
        }

        _prev = ks;
        base.Update(gameTime);
    }

    public override void Draw(GameTime gameTime)
    {
        if (!_isOpen || this._font is null || _batch is null) return;
        var vp = GraphicsDevice.Viewport.Bounds;
        var h = (int)(vp.Height * 0.4f);
        var rect = new Rectangle(0, 0, vp.Width, h);
        var _font = this._font.GetFont(h - 2);

        _batch.Begin();
        // 背景
        _lazyPixel ??= new Texture2D(GraphicsDevice, 1, 1);
        if (_lazyDirty) { _lazyPixel.SetData(new[] { Color.White }); _lazyDirty = false; }
        _batch.Draw(_lazyPixel, rect, new Color(0, 0, 0, 192));

        int pad = 8;
        int y = pad;
        int lineH = _font.LineHeight + 2;

        // 顯示尾端最多能容納的行數
        var maxLines = Math.Max(1, (h - pad*3 - lineH) / lineH);
        int start = Math.Max(0, _logs.Count - maxLines);
        for (int i = start; i < _logs.Count; i++)
        {
            _batch.DrawString(_font, _logs[i], new Vector2(pad, y), Color.White);
            y += lineH;
        }

        // 輸入列
        _batch.DrawString(_font, $"> {_input}", new Vector2(pad, h - lineH - pad), Color.Yellow);
        _batch.End();
    }

    private void Append(string line)
    {
        foreach (var l in line.Split('\n'))
        {
            _logs.Add(l);
            if (_logs.Count > 200) _logs.RemoveAt(0);
        }
    }

    private static char ToChar(Keys key, KeyboardState ks)
    {
        bool shift = ks.IsKeyDown(Keys.LeftShift) || ks.IsKeyDown(Keys.RightShift);
        if (key >= Keys.A && key <= Keys.Z)
        {
            var ch = (char)('a' + (key - Keys.A));
            return shift ? char.ToUpperInvariant(ch) : ch;
        }
        if (key >= Keys.D0 && key <= Keys.D9)
        {
            return (char)('0' + (key - Keys.D0));
        }
        return key switch
        {
            Keys.OemComma => ',',
            Keys.OemPeriod => '.',
            Keys.OemMinus => '-',
            Keys.OemPlus => '+',
            Keys.OemSemicolon => ';',
            Keys.OemQuotes => '"',
            Keys.OemQuestion => '?',
            _ => '\0'
        };
    }

    private bool Pressed(KeyboardState now, Keys k) => now.IsKeyDown(k) && !_prev.IsKeyDown(k);

    private Texture2D? _lazyPixel;
    private bool _lazyDirty = true;
}

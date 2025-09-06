using System;
using System.Collections.Generic;
using System.Text;

namespace Console.Core;

/// <summary>
/// 簡易命令列解析器：支援引號與跳脫字元。
/// </summary>
public static class CommandParser
{
    public static (string command, string[] args) Parse(string input)
    {
        input ??= string.Empty;
        var tokens = Tokenize(input);
        if (tokens.Count == 0) return (string.Empty, Array.Empty<string>());
        var cmd = tokens[0];
        tokens.RemoveAt(0);
        return (cmd, tokens.ToArray());
    }

    private static List<string> Tokenize(string input)
    {
        var list = new List<string>();
        var sb = new StringBuilder();
        bool inQuotes = false;
        for (int i = 0; i < input.Length; i++)
        {
            var c = input[i];
            if (c == '\\' && i + 1 < input.Length)
            {
                sb.Append(input[++i]);
                continue;
            }
            if (c == '"')
            {
                inQuotes = !inQuotes;
                continue;
            }
            if (char.IsWhiteSpace(c) && !inQuotes)
            {
                if (sb.Length > 0) { list.Add(sb.ToString()); sb.Clear(); }
            }
            else
            {
                sb.Append(c);
            }
        }
        if (sb.Length > 0) list.Add(sb.ToString());
        return list;
    }
}

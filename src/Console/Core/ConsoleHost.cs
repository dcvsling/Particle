using System;
using System.Collections.Generic;
using Console.Abstractions;
using Console.Commands;

namespace Console.Core;

/// <summary>
/// 最小可用的 Console 主機，提供註冊、列舉與執行。
/// </summary>
public sealed class ConsoleHost(params IEnumerable<IConsoleCommand> commands) : IConsoleHost
{
    public static ConsoleHost Default { get; } = new ConsoleHost();
    private readonly CommandRegistry _registry = new([new HelpCommand(), .. commands]);

    public void Register(IConsoleCommand command) => _registry.Register(command);

    public IEnumerable<CommandInfo> GetAllCommands() => _registry.AllInfos();

    public bool TryGetCommand(string name, out IConsoleCommand? command) => _registry.TryGet(name, out command);

    /// <summary>執行一行命令字串，回傳指令輸出。未知指令則回傳錯誤訊息。</summary>
    public string ExecuteLine(string line)
    {
        var (cmd, args) = CommandParser.Parse(line);
        if (string.IsNullOrWhiteSpace(cmd)) return string.Empty;

        if (TryGetCommand(cmd, out var command) && command is not null)
        {
            return command.Execute(this, args);
        }
        return $"未知指令：{cmd}（輸入 help 查看清單）";
    }
}

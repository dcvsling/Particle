using System;
using System.Collections.Generic;
using System.Linq;
using Console.Abstractions;

namespace Console.Core;

/// <summary>
/// 指令註冊表：集中管理註冊與查詢。
/// </summary>
public sealed class CommandRegistry(params IEnumerable<IConsoleCommand> commands)
{
    private readonly Dictionary<string, IConsoleCommand> _commands =
        commands.OfType<IConsoleCommand>()
            .Aggregate
                (new Dictionary<string, IConsoleCommand>(StringComparer.OrdinalIgnoreCase),
                (map, cmd) =>
                {
                    map[cmd.Name] = cmd;
                    return map;
                });

    public void Register(IConsoleCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);
        _commands[command.Name] = command;
    }

    public bool TryGet(string name, out IConsoleCommand? command)
        => _commands.TryGetValue(name, out command);

    public IEnumerable<IConsoleCommand> All() => _commands.Values.OrderBy(c => c.Name);

    public IEnumerable<CommandInfo> AllInfos() =>
        All().Select(c => new CommandInfo(c.Name, c.Description, c.Usage));
}

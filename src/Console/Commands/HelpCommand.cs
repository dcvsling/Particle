
using System.Text;
using Console.Abstractions;

namespace Console.Commands;

/// <summary>
/// help 指令：列出目前可用的所有指令，或顯示單一指令的詳情。
/// </summary>
public sealed class HelpCommand : IConsoleCommand
{
    public string Name => "help";
    public string Description => "列出目前可用的指令，或顯示某指令用法";
    public string Usage => "help [command]";

    public string Execute(IConsoleHost host, string[] args)
    {
        if (args.Length == 0)
        {
            var sb = new StringBuilder();
            sb.AppendLine("可用指令：");
            foreach (var info in host.GetAllCommands().OrderBy(c => c.Name))
            {
                sb.AppendLine($"{info.Name,-12} {info.Description}");
            }
            return sb.ToString().TrimEnd();
        }
        else
            return host.TryGetCommand(args[0], out var cmd) && cmd is not null
                ? $"{cmd.Name} - {cmd.Description}\n用法：{cmd.Usage}" 
                : $"找不到指令：{args[0]}";
        
    }
}

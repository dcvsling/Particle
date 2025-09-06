using System.Collections.Generic;

namespace Console.Abstractions;

/// <summary>
/// Console 主機能提供 Help 指令所需的資訊。
/// </summary>
public interface IConsoleHost
{
    /// <summary>列舉所有可用的指令摘要。</summary>
    IEnumerable<CommandInfo> GetAllCommands();

    /// <summary>嘗試取得單一指令（by name）。</summary>
    bool TryGetCommand(string name, out IConsoleCommand? command);
}

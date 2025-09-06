namespace Console.Abstractions;

/// <summary>
/// 遊戲內 Console 的指令介面。
/// </summary>
public interface IConsoleCommand
{
    /// <summary>指令名稱（唯一）。</summary>
    string Name { get; }
    /// <summary>指令描述（顯示於 help 列表）。</summary>
    string Description { get; }
    /// <summary>用法說明。</summary>
    string Usage { get; }

    /// <summary>執行指令。</summary>
    /// <param name="host">Console 主機（可用來取得其他資訊）。</param>
    /// <param name="args">參數陣列。</param>
    /// <returns>回傳訊息（可為空字串）。</returns>
    string Execute(IConsoleHost host, string[] args);
}

namespace Console.Abstractions;

/// <summary>
/// 用於對外顯示可用指令的摘要資訊。
/// </summary>
public sealed record CommandInfo(string Name, string Description, string Usage);

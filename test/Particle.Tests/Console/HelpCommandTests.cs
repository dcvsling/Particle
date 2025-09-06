using FluentAssertions;
using Console.Abstractions;
using Console.Commands;
using Console.Core;
using Xunit;
namespace Console.Tests;
public sealed class HelpCommandTests
{
    private sealed class DummyCommand : IConsoleCommand
    {
        public string Name => "echo";
        public string Description => "輸出文字";
        public string Usage => "echo <text...>";
        public string Execute(IConsoleHost host, string[] args) => string.Join(' ', args);
    }

    [Fact]
    public void Help_Should_List_All_Registered_Commands()
    {
        var host = new ConsoleHost();
        host.Register(new DummyCommand());

        var output = host.ExecuteLine("help");

        output.Should().Contain("help").And.Contain("echo");
        output.Should().Contain("列出目前可用的指令");
        output.Should().Contain("輸出文字");
    }

    [Fact]
    public void Help_With_Command_Name_Should_Show_Usage()
    {
        var host = new ConsoleHost();
        host.Register(new DummyCommand());

        var output = host.ExecuteLine("help echo");

        output.Should().Contain("echo - 輸出文字");
        output.Should().Contain("用法：echo <text...>");
    }

    [Fact]
    public void Help_Unknown_Command_Should_Complain()
    {
        var host = new ConsoleHost();
        var output = host.ExecuteLine("help nope");

        output.Should().Contain("找不到指令：nope");
    }
}

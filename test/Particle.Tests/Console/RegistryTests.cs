using FluentAssertions;
using Console.Abstractions;
using Console.Commands;
using Console.Core;
using Xunit;
namespace Console.Tests;
public class RegistryIntegrationTests
{
    private sealed class OneCmd : IConsoleCommand
    {
        public string Name => "one";
        public string Description => "test one";
        public string Usage => "one";
        public string Execute(IConsoleHost host, string[] args) => "1";
    }

    [Fact]
    public void ExecuteLine_Should_Run_Command_Or_Show_Unknown()
    {
        var host = new ConsoleHost();
        host.Register(new OneCmd());

        host.ExecuteLine("one").Should().Be("1");
        host.ExecuteLine("nope").Should().Contain("未知指令");
    }
}

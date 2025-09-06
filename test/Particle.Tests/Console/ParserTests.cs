using FluentAssertions;
using Console.Core;
using Xunit;
namespace Console;
public class ParserTests
{
    [Theory]
    [InlineData("echo hello world", "echo", new[] { "hello", "world" })]
    [InlineData("echo \"hello world\"", "echo", new[] { "hello world" })]
    [InlineData("say \"hi\"", "say", new[] { "hi" })]
    public void Parse_Should_Split_Command_And_Args(string input, string expectedCmd, string[] expectedArgs)
    {
        var (cmd, args) = CommandParser.Parse(input);
        cmd.Should().Be(expectedCmd);
        args.Should().BeEquivalentTo(expectedArgs);
    }
}

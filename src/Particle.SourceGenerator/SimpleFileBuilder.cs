// Particle.SourceGenerator/SimpleFileBuilder.cs
#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Particle.SourceGenerator;

/// <summary>極簡檔案產生器（為了降低相依、集中 using/namespace/member 組裝）。</summary>
public sealed class SimpleFileBuilder
{
    private readonly string _fileName;
    private readonly string _ns;
    private readonly List<string> _usings = new();
    private readonly List<MemberDeclarationSyntax> _members = new();

    public SimpleFileBuilder(string fileName, string ns)
    {
        _fileName = fileName;
        _ns = ns;
    }

    public SimpleFileBuilder AddUsing(string ns)
    {
        _usings.Add(ns);
        return this;
    }

    public SimpleFileBuilder AddMember(MemberDeclarationSyntax member)
    {
        _members.Add(member);
        return this;
    }

    public (string FileName, SourceText SourceText) Render()
    {
        var usings = _usings
            .Distinct()
            .Select(u => UsingDirective(IdentifierName(u)))
            .ToArray();

        var @namespace = NamespaceDeclaration(IdentifierName(_ns))
            .WithMembers(List(_members));

        var unit = CompilationUnit()
            .WithUsings(List(usings))
            .WithMembers(SingletonList<MemberDeclarationSyntax>(@namespace))
            .NormalizeWhitespace();

        return (_fileName, unit.GetText(Encoding.UTF8));
    }
}

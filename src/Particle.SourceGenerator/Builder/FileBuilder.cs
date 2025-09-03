using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Microsoft.CodeAnalysis;

namespace Particle.SourceGenerator.Builder;

public sealed class FileBuilder
{
    public string FileName { get; }
    public string Namespace { get; }

    private readonly UsingManager _usingManager = new();
    private readonly List<MemberDeclarationSyntax> _members = new();

    public FileBuilder(string fileName, string @namespace)
    {
        FileName = fileName;
        Namespace = @namespace;
        _usingManager.SetCurrentNamespace(@namespace);
    }

    public FileBuilder AddUsing(params string[] namespaces)
    {
        _usingManager.Add(namespaces);
        return this;
    }

    public FileBuilder AddMember(MemberDeclarationSyntax member)
    {
        _members.Add(member);
        return this;
    }

    public CompilationUnitSyntax RenderCompilationUnit()
    {
        var cu = CompilationUnit().WithUsings(List(_usingManager.ToSyntax()));
        var nsDecl = NamespaceDeclaration(IdentifierName(Namespace))
            .WithMembers(List(_members));
        return cu.WithMembers(SingletonList<MemberDeclarationSyntax>(nsDecl))
                 .NormalizeWhitespace(eol: "\n", indentation: "    ");
    }

    public CodeRenderPlan Render()
    {
        var cu = RenderCompilationUnit();
        var text = cu.GetText(Encoding.UTF8);
        return new CodeRenderPlan(FileName, text);
    }
}

public sealed record CodeRenderPlan(string FileName, SourceText SourceText);

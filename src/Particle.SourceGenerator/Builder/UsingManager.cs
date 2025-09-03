using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Particle.SourceGenerator.Builder;

public sealed class UsingManager
{
    private readonly HashSet<string> _usings = new(StringComparer.Ordinal);
    private string? _currentNamespace;

    public UsingManager SetCurrentNamespace(string? ns)
    {
        _currentNamespace = string.IsNullOrWhiteSpace(ns) ? null : ns;
        return this;
    }

    public UsingManager Add(string ns)
    {
        if (string.IsNullOrWhiteSpace(ns)) return this;
        if (_currentNamespace != null && string.Equals(_currentNamespace, ns, StringComparison.Ordinal))
            return this;
        _usings.Add(ns);
        return this;
    }

    public UsingManager Add(params string[] namespaces)
    {
        foreach (var n in namespaces) Add(n);
        return this;
    }

    public IEnumerable<UsingDirectiveSyntax> ToSyntax() =>
        _usings
            .OrderBy(n => n.StartsWith("System", StringComparison.Ordinal) ? 0 : 1)
            .ThenBy(n => n, StringComparer.Ordinal)
            .Select(n => UsingDirective(ParseName(n)));
}

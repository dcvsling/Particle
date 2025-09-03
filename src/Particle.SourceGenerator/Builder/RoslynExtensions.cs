using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Particle.SourceGenerator.Builder;

public static class RoslynExtensions
{
    public static string GetFullNamespace(this INamespaceSymbol? ns)
        => ns?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
               .Replace("global::", string.Empty) ?? string.Empty;

    public static string GetDeclNamespace(this ISymbol symbol)
        => symbol.ContainingNamespace?.ToDisplayString() ?? string.Empty;

    public static string GetTypeKindKeyword(this INamedTypeSymbol t)
        => t.TypeKind == TypeKind.Struct ? "struct" : "class";

    public static bool IsAttributeName(this AttributeSyntax syntax, params ReadOnlySpan<string> names)
        => names.Contains(syntax.Name switch
        {
            IdentifierNameSyntax id => id.Identifier.ValueText,
            QualifiedNameSyntax q => q.Right.Identifier.ValueText,
            _ => string.Empty
        });
}

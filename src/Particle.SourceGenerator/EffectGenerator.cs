// Particle.SourceGenerator/EffectGenerator.cs
#nullable enable
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Particle.SourceGenerator;

[Generator(LanguageNames.CSharp)]
public sealed partial class EffectGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var methods = context.SyntaxProvider.CreateSyntaxProvider(
            static (node, _) => IsMethodWithAttributes(node),
            static (ctx, ct) => ToEffectCandidate(ctx, ct)
        );

        var combo = context.CompilationProvider.Combine(methods.Collect());
        context.RegisterSourceOutput(combo, static (spc, tuple) =>
        {
            var (comp, list) = tuple;
            Pipeline.Generate(spc, comp, list);
        });
    }
    public static bool IsMethodWithAttributes(SyntaxNode node)
        => node is MethodDeclarationSyntax m && m.AttributeLists.Count > 0;

    public static (IMethodSymbol sym, MethodDeclarationSyntax method) ToEffectCandidate(
        GeneratorSyntaxContext ctx, CancellationToken ct)
    {
        var method = (MethodDeclarationSyntax)ctx.Node;
        if (!HasEffectAttribute(method)) return default;

        if (ctx.SemanticModel.GetDeclaredSymbol(method, ct) is not IMethodSymbol sym) return default;
        if (!IsValidSignature(sym)) return default;

        return (sym, method);
    }

    public static bool HasEffectAttribute(MethodDeclarationSyntax method)
    {
        foreach (var attr in method.AttributeLists.SelectMany(a => a.Attributes))
        {
            var name = attr.Name switch
            {
                IdentifierNameSyntax id => id.Identifier.ValueText,
                QualifiedNameSyntax q => q.Right.Identifier.ValueText,
                _ => string.Empty
            };
            // 支援多種寫法
            if (name is "Effect" or "EffectAttribute" or "Partice.Effect" or "Partice.EffectAttribute")
                return true;
        }
        return false;
    }

    public static bool IsValidSignature(IMethodSymbol sym)
    {
        if (sym.Parameters.Length < 2) return false;            // ref T + 至少一標量
        if (!sym.Parameters[0].RefKind.HasFlag(RefKind.Ref)) return false; // 第一參數必須 ref
        return true;
    }
}

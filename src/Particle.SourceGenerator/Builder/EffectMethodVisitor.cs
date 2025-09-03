using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Particle.SourceGenerator.Builder;

/// <summary>
/// 訪問 [Effect] 方法中的運算特徵，供後續最佳化決策（例如：是否能轉換為 SIMD）。
/// </summary>
public sealed class EffectMethodVisitor : CSharpSyntaxVisitor
{
    public int AddCount { get; private set; }
    public int SubCount { get; private set; }
    public int MulCount { get; private set; }
    public int DivCount { get; private set; }
    public int MathCallCount { get; private set; }

    public override void VisitBinaryExpression(BinaryExpressionSyntax node)
    {
        switch (node.Kind())
        {
            case SyntaxKind.AddExpression: AddCount++; break;
            case SyntaxKind.SubtractExpression: SubCount++; break;
            case SyntaxKind.MultiplyExpression: MulCount++; break;
            case SyntaxKind.DivideExpression: DivCount++; break;
        }
        base.VisitBinaryExpression(node);
    }

    public override void VisitInvocationExpression(InvocationExpressionSyntax node)
    {
        // 簡單判斷：識別 Math.* 呼叫（更嚴謹可配合 SemanticModel）
        if (node.Expression is MemberAccessExpressionSyntax m
            && m.Expression is IdentifierNameSyntax id
            && id.Identifier.ValueText == "Math")
        {
            MathCallCount++;
        }
        base.VisitInvocationExpression(node);
    }
}

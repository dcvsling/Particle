using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Particle.SourceGenerator;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Particle.SourceGenerator;

internal static class VectorLowerer
{
    public static ExpressionSyntax Vec(IRExpr e) => e switch
    {
        IRConst c      => NewVec(LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(c.Value))),
        IRScalar s     => IdentifierName("v_" + s.Name),
        IRVarField f   => IdentifierName(f.Name + "v"),
        IRUnary u      => u.Func switch
        {
            SimdFunc.Abs  => CallVecStatic("Abs", Vec(u.Arg)),
            SimdFunc.Sqrt => CallVecStatic("SquareRoot", Vec(u.Arg)),
            _ => throw new System.NotSupportedException()
        },
        IRBinary b     => BinaryExpression(b.Op switch
        {
            SimdOp.Add => SyntaxKind.AddExpression,
            SimdOp.Sub => SyntaxKind.SubtractExpression,
            SimdOp.Mul => SyntaxKind.MultiplyExpression,
            SimdOp.Div => SyntaxKind.DivideExpression,
            _ => throw new System.NotSupportedException()
        }, Vec(b.Left), Vec(b.Right)),
        IRCall c2      => c2.Func switch
        {
            SimdFunc.Min => CallVecStatic("Min", Vec(c2.A), Vec(c2.B)),
            SimdFunc.Max => CallVecStatic("Max", Vec(c2.A), Vec(c2.B)),
            _ => throw new System.NotSupportedException()
        },
        _ => throw new System.NotSupportedException()
    };

    public static ExpressionSyntax Scalar(IRExpr e, string particleIdent, string indexIdent) => e switch
    {
        IRConst c    => LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(c.Value)),
        IRScalar s   => IdentifierName(s.Name),
        IRVarField f => ElementAccessExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, IdentifierName(particleIdent), IdentifierName(f.Name)))
                            .WithArgumentList(BracketedArgumentList(SingletonSeparatedList(Argument(IdentifierName(indexIdent))))),
        IRUnary u    => u.Func switch
        {
            SimdFunc.Abs  => InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, IdentifierName("MathF"), IdentifierName("Abs")))
                                .WithArgumentList(ArgumentList(SingletonSeparatedList(Argument(Scalar(u.Arg, particleIdent, indexIdent))))),
            SimdFunc.Sqrt => InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, IdentifierName("MathF"), IdentifierName("Sqrt")))
                                .WithArgumentList(ArgumentList(SingletonSeparatedList(Argument(Scalar(u.Arg, particleIdent, indexIdent))))),
            _ => throw new System.NotSupportedException()
        },
        IRBinary b   => BinaryExpression(b.Op switch
        {
            SimdOp.Add => SyntaxKind.AddExpression,
            SimdOp.Sub => SyntaxKind.SubtractExpression,
            SimdOp.Mul => SyntaxKind.MultiplyExpression,
            SimdOp.Div => SyntaxKind.DivideExpression,
            _ => throw new System.NotSupportedException()
        }, Scalar(b.Left, particleIdent, indexIdent), Scalar(b.Right, particleIdent, indexIdent)),
        IRCall c2    => c2.Func switch
        {
            SimdFunc.Min => InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, IdentifierName("MathF"), IdentifierName("Min")))
                                .WithArgumentList(ArgumentList(SeparatedList(new[] { Argument(Scalar(c2.A, particleIdent, indexIdent)), Argument(Scalar(c2.B, particleIdent, indexIdent)) }))),
            SimdFunc.Max => InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, IdentifierName("MathF"), IdentifierName("Max")))
                                .WithArgumentList(ArgumentList(SeparatedList(new[] { Argument(Scalar(c2.A, particleIdent, indexIdent)), Argument(Scalar(c2.B, particleIdent, indexIdent)) }))),
            _ => throw new System.NotSupportedException()
        },
        _ => throw new System.NotSupportedException()
    };

    private static ObjectCreationExpressionSyntax NewVec(ExpressionSyntax arg)
        => ObjectCreationExpression(GenericName(Identifier("Vector")).WithTypeArgumentList(TypeArgumentList(SingletonSeparatedList<TypeSyntax>(PredefinedType(Token(SyntaxKind.FloatKeyword))))))
           .WithArgumentList(ArgumentList(SingletonSeparatedList(Argument(arg))));

    private static InvocationExpressionSyntax CallVecStatic(string name, params ExpressionSyntax[] args)
        => InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, IdentifierName("Vector"), IdentifierName(name)))
           .WithArgumentList(ArgumentList(SeparatedList(args.Select(a => Argument(a)))));
}

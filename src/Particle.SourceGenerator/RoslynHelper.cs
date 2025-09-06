// Particle.SourceGenerator/RoslynHelper.cs
#nullable enable
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Particle.SourceGenerator;

// ------------------------ Small Helpers ------------------------

public static class RoslynHelper
{
    // 確保回傳的是 TypeSyntax（而不是 ExpressionSyntax）
    public static TypeSyntax GenericType(string name, TypeSyntax arg)
        => GenericName(Identifier(name))
        .WithTypeArgumentList(TypeArgumentList(SingletonSeparatedList(arg)));
    public static TypeSyntax FloatType => PredefinedType(Token(SyntaxKind.FloatKeyword));
    public static TypeSyntax IntType => PredefinedType(Token(SyntaxKind.IntKeyword));

    // 這個是給 ObjectCreationExpression 用的泛型型別建構器
    public static TypeSyntax VoidType => PredefinedType(Token(SyntaxKind.VoidKeyword));
    public static TypeSyntax DoubleType => PredefinedType(Token(SyntaxKind.DoubleKeyword));
    public static ArrayTypeSyntax FloatArrayType =>
        ArrayType(FloatType).WithRankSpecifiers(SingletonList(ArrayRankSpecifier(SingletonSeparatedList<ExpressionSyntax>(
            OmittedArraySizeExpression()))));
        public static ArrayTypeSyntax FloatArrayTypeRankOmitted =>
    ArrayType(FloatType).WithRankSpecifiers(
        SingletonList(ArrayRankSpecifier(
            SingletonSeparatedList<ExpressionSyntax>(OmittedArraySizeExpression()))));

    // new float[<size>]：建構子（或任何需要具體長度的地方）用
    public static ExpressionSyntax NewFloatArray(ExpressionSyntax size) =>
        ArrayCreationExpression(
            ArrayType(FloatType).WithRankSpecifiers(
                SingletonList(ArrayRankSpecifier(
                    SingletonSeparatedList(size)))));
    public static IdentifierNameSyntax Id(string name) => IdentifierName(name);
    public static TypeSyntax TypeId(string name) => ParseTypeName(name);
    public static ExpressionSyntax Lit(int v) => LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(v));

    public static ExpressionSyntax Add(ExpressionSyntax a, ExpressionSyntax b) => BinaryExpression(SyntaxKind.AddExpression, a, b);
    public static ExpressionSyntax Sub(ExpressionSyntax a, ExpressionSyntax b) => BinaryExpression(SyntaxKind.SubtractExpression, a, b);
    public static ExpressionSyntax Mul(ExpressionSyntax a, ExpressionSyntax b) => BinaryExpression(SyntaxKind.MultiplyExpression, a, b);
    public static ExpressionSyntax Lt(ExpressionSyntax a, ExpressionSyntax b) => BinaryExpression(SyntaxKind.LessThanExpression, a, b);
    public static ExpressionSyntax Le(ExpressionSyntax a, ExpressionSyntax b) => BinaryExpression(SyntaxKind.LessThanOrEqualExpression, a, b);

    public static ExpressionSyntax Member(ExpressionSyntax expr, string name)
        => MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, expr, IdentifierName(name));

    public static ExpressionSyntax Generic(string name, TypeSyntax arg)
        => GenericName(Identifier(name)).WithTypeArgumentList(TypeArgumentList(SingletonSeparatedList(arg)));

    public static ArgumentSyntax RefArg(ExpressionSyntax expr)
        => Argument(expr).WithRefKindKeyword(Token(SyntaxKind.RefKeyword));

    public static StatementSyntax AssignStmt(ExpressionSyntax left, ExpressionSyntax right)
        => ExpressionStatement(AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, left, right));

    public static ExpressionSyntax Element(ExpressionSyntax array, ExpressionSyntax index)
        => ElementAccessExpression(array).WithArgumentList(BracketedArgumentList(SingletonSeparatedList(Argument(index))));

    public static LocalDeclarationStatementSyntax IntLocal(string name, ExpressionSyntax init)
        => LocalDeclarationStatement(
            VariableDeclaration(IntType).WithVariables(SingletonSeparatedList(
                VariableDeclarator(Identifier(name)).WithInitializer(EqualsValueClause(init)))));
    
}

// Particle.SourceGenerator/ParticleSoAEmitter.cs
#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Particle.SourceGenerator;

// ------------------------ Emitters ------------------------

public static class ParticleSoAEmitter
{
    public static void EmitParticleSoA(SourceProductionContext spc, INamedTypeSymbol particle)
    {
        var ns       = particle.ContainingNamespace?.ToDisplayString() ?? "Global";
        var pName    = particle.Name;
        var simdName = pName + "_SIMD";

        // 僅 public float 欄位
        var floatFields = particle.GetMembers()
            .OfType<IFieldSymbol>()
            .Where(f => f.DeclaredAccessibility == Accessibility.Public &&
                        f.Type.SpecialType == SpecialType.System_Single)
            .Select(f => f.Name)
            .OrderBy(n => n, StringComparer.Ordinal)
            .ToArray();

        var fb = new SimpleFileBuilder(simdName + ".g.cs", ns)
            .AddUsing("System")
            .AddUsing("Particle.Abstractions");

        // 組合 struct 成員
        var members = new List<MemberDeclarationSyntax>
        {
            Fields.LengthBacking(),                // private int _length;
            Properties.LengthProperty(),           // public int Length => _length;
        };

        // public float[] X; ...（不產生屬性）
        members.AddRange(Fields.FloatArrays(floatFields));

        // ctor: <P>_SIMD(int capacity = 1) -> 建陣列 + capacity 最小值 1
        members.Add(Ctors.SimdCtor(simdName, floatFields));

        // EnsureCapacity：足夠 → return，否則倍增
        if (floatFields.Length > 0)
            members.Add(Methods.EnsureCapacity(floatFields[0], floatFields));

        // Add(ref P p)
        members.Add(Methods.AddMethod(pName, floatFields));

        // struct <P>_SIMD : ISIMDParticle<P>
        var decl = StructDeclaration(Identifier(simdName))
            .AddModifiers(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.PartialKeyword))
            .WithMembers(List(members))
            .WithBaseList(BaseList(
                SingletonSeparatedList<BaseTypeSyntax>(
                    SimpleBaseType(RoslynHelper.TypeId($"ISIMDParticle<{pName}>")))));

        fb.AddMember(decl);
        var plan = fb.Render();
        spc.AddSource(plan.FileName, plan.SourceText);
    }

    // ---- Building blocks (SoA) ----

    public static class Fields
    {
        public static MemberDeclarationSyntax LengthBacking()
            => FieldDeclaration(VariableDeclaration(RoslynHelper.IntType)
                    .WithVariables(SingletonSeparatedList(VariableDeclarator(Identifier("_length")))))
                .AddModifiers(Token(SyntaxKind.PrivateKeyword));

        public static IEnumerable<MemberDeclarationSyntax> FloatArrays(IEnumerable<string> names)
            => names.Select(n =>
                FieldDeclaration(
                    VariableDeclaration(RoslynHelper.FloatArrayTypeRankOmitted)
                        .WithVariables(SingletonSeparatedList(VariableDeclarator(Identifier(n)))))
                .AddModifiers(Token(SyntaxKind.PublicKeyword)));
    }

    public static class Properties
    {
        public static MemberDeclarationSyntax LengthProperty()
            => PropertyDeclaration(RoslynHelper.IntType, Identifier("Length"))
                .AddModifiers(Token(SyntaxKind.PublicKeyword))
                .WithExpressionBody(ArrowExpressionClause(IdentifierName("_length")))
                .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));
    }

    public static class Ctors
    {
        public static ConstructorDeclarationSyntax SimdCtor(string typeName, string[] floatFields)
        {
            var body = new List<StatementSyntax>
            {
                // if (capacity < 1) capacity = 1;
                IfStatement(
                    RoslynHelper.Lt(RoslynHelper.Id("capacity"), RoslynHelper.Lit(1)),
                    Block(RoslynHelper.AssignStmt(RoslynHelper.Id("capacity"), RoslynHelper.Lit(1))))
            };

            // X = new float[capacity]; ...
            foreach (var f in floatFields)
            {
                body.Add(RoslynHelper.AssignStmt(
                    RoslynHelper.Id(f),
                    RoslynHelper.NewFloatArray(RoslynHelper.Id("capacity"))));
            }

            return ConstructorDeclaration(Identifier(typeName))
                .AddModifiers(Token(SyntaxKind.PublicKeyword))
                .WithParameterList(ParameterList(SingletonSeparatedList(
                    Parameter(Identifier("capacity"))
                        .WithType(RoslynHelper.IntType)
                        .WithDefault(EqualsValueClause(RoslynHelper.Lit(1))))))
                .WithBody(Block(body));
        }
    }

    public static class Methods
    {
        public static MethodDeclarationSyntax EnsureCapacity(string firstArrayName, string[] allArrayNames)
        {
            // if (needed <= first.Length) return;
            var earlyReturn = IfStatement(
                RoslynHelper.Le(RoslynHelper.Id("needed"), RoslynHelper.Member(RoslynHelper.Id(firstArrayName), "Length")),
                Block(ReturnStatement()));

            // int newCap = Math.Max(needed, first.Length * 2);
            var newCap = LocalDeclarationStatement(
                VariableDeclaration(RoslynHelper.IntType)
                    .WithVariables(SingletonSeparatedList(
                        VariableDeclarator(Identifier("newCap"))
                            .WithInitializer(EqualsValueClause(
                                InvocationExpression(RoslynHelper.Member(RoslynHelper.Id("Math"), "Max"))
                                    .WithArgumentList(ArgumentList(SeparatedList(new[]
                                    {
                                        Argument(RoslynHelper.Id("needed")),
                                        Argument(RoslynHelper.Mul(
                                            RoslynHelper.Member(RoslynHelper.Id(firstArrayName), "Length"),
                                            RoslynHelper.Lit(2)))
                                    }))))))));

            // Array.Resize(ref X, newCap); ...
            var resizes = allArrayNames.Select(name =>
                ExpressionStatement(
                    InvocationExpression(RoslynHelper.Member(RoslynHelper.Id("Array"), "Resize"))
                        .WithArgumentList(ArgumentList(SeparatedList(new[]
                        {
                            RoslynHelper.RefArg(RoslynHelper.Id(name)),
                            Argument(RoslynHelper.Id("newCap"))
                        })))));
            StatementSyntax[] statements = [earlyReturn, newCap, ..resizes];
            return MethodDeclaration(RoslynHelper.VoidType, Identifier("EnsureCapacity"))
                .AddModifiers(Token(SyntaxKind.PrivateKeyword))
                .WithParameterList(ParameterList(SingletonSeparatedList(
                    Parameter(Identifier("needed")).WithType(RoslynHelper.IntType))))
                .WithBody(Block(statements));
        }

        public static MethodDeclarationSyntax AddMethod(string pName, string[] floatFields)
        {
            var body = new List<StatementSyntax>
            {
                // EnsureCapacity(_length + 1);
                ExpressionStatement(
                    InvocationExpression(RoslynHelper.Id("EnsureCapacity"))
                        .WithArgumentList(ArgumentList(SingletonSeparatedList(
                            Argument(RoslynHelper.Add(RoslynHelper.Id("_length"), RoslynHelper.Lit(1))))))),
                // int i = _length;
                LocalDeclarationStatement(
                    VariableDeclaration(RoslynHelper.IntType)
                        .WithVariables(SingletonSeparatedList(
                            VariableDeclarator(Identifier("i"))
                                .WithInitializer(EqualsValueClause(RoslynHelper.Id("_length"))))))
            };

            // X[i] = p.X; ...
            foreach (var f in floatFields)
            {
                body.Add(RoslynHelper.AssignStmt(
                    RoslynHelper.Element(RoslynHelper.Id(f), RoslynHelper.Id("i")),
                    RoslynHelper.Member(RoslynHelper.Id("p"), f)));
            }

            // _length++;
            body.Add(ExpressionStatement(PostfixUnaryExpression(
                SyntaxKind.PostIncrementExpression, RoslynHelper.Id("_length"))));

            return MethodDeclaration(RoslynHelper.VoidType, Identifier("Add"))
                .AddModifiers(Token(SyntaxKind.PublicKeyword))
                .WithParameterList(ParameterList(SingletonSeparatedList(
                    Parameter(Identifier("p"))
                        .WithType(RoslynHelper.TypeId(pName))
                        .WithModifiers(TokenList(Token(SyntaxKind.RefKeyword))))))
                .WithBody(Block(body));
        }
    }
}

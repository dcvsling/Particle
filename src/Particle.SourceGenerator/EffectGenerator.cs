using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Particle.SourceGenerator.Builder;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Particle.SourceGenerator;

[Generator(LanguageNames.CSharp)]
public sealed class EffectGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        
        var methods = context.SyntaxProvider.CreateSyntaxProvider(
            static (node, _) => node is MethodDeclarationSyntax m && m.AttributeLists.Count > 0,
            static (ctx, ct) =>
            {
                var method = (MethodDeclarationSyntax)ctx.Node;
                bool has = method.AttributeLists.SelectMany(a => a.Attributes).Any(a =>
                {
                    var name = a.Name switch
                    {
                        IdentifierNameSyntax id => id.Identifier.ValueText,
                        QualifiedNameSyntax q => q.Right.Identifier.ValueText,
                        _ => string.Empty
                    };
                    return name is "Effect" or "EffectAttribute" or "Partice.Effect" or "Partice.EffectAttribute";
                });
                if (!has) return default;
                if (ctx.SemanticModel.GetDeclaredSymbol(method, ct) is not IMethodSymbol sym) return default;
                if (sym.Parameters.Length < 2) return default; // ref T + 至少一標量
                if (!sym.Parameters[0].RefKind.HasFlag(RefKind.Ref)) return default;
                return (sym, method);
            }).Where(s => s != default)!;

        var combo = context.CompilationProvider.Combine(methods.Collect());
        context.RegisterSourceOutput(combo, (spc, tuple) =>
        {
            var (comp, list) = tuple;
            var emittedSoA = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);
            foreach (var (methodSym, methodDecl) in list.Distinct())
            {
                var particleType = (INamedTypeSymbol)methodSym.Parameters[0].Type;
                if (!emittedSoA.Contains(particleType)) { emittedSoA.Add(particleType); EmitParticleSoA(spc, particleType); }
                EmitEffectSimdUpdate(spc, comp, methodSym, methodDecl);
            }
        });
    }

    private static void EmitParticleSoA(SourceProductionContext spc, INamedTypeSymbol particle)
    {
        var ns = particle.ContainingNamespace?.ToDisplayString() ?? "Global";
        var pName = particle.Name;
        var simdName = pName + "_SIMD";

        var floatFields = particle.GetMembers().OfType<IFieldSymbol>()
            .Where(f => f.DeclaredAccessibility == Accessibility.Public && f.Type.SpecialType == SpecialType.System_Single)
            .Select(f => f.Name).OrderBy(n => n, StringComparer.Ordinal).ToArray();

        var fb = new FileBuilder(simdName + ".g.cs", ns)
            .AddUsing("System")
            .AddUsing("Particle.Abstractions");

        var members = new List<MemberDeclarationSyntax>();

        // int _length; public int Length => _length;
        members.Add(FieldDeclaration(VariableDeclaration(PredefinedType(Token(SyntaxKind.IntKeyword)))
            .WithVariables(SingletonSeparatedList(VariableDeclarator(Identifier("_length")))))
            .AddModifiers(Token(SyntaxKind.PrivateKeyword)));

        members.Add(PropertyDeclaration(PredefinedType(Token(SyntaxKind.IntKeyword)), Identifier("Length"))
            .AddModifiers(Token(SyntaxKind.PublicKeyword))
            .WithExpressionBody(ArrowExpressionClause(IdentifierName("_length")))
            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)));

        // public float[] X; ...（不產生屬性）
        foreach (var f in floatFields)
        {
            members.Add(FieldDeclaration(VariableDeclaration(
                    ArrayType(PredefinedType(Token(SyntaxKind.FloatKeyword)))
                        .WithRankSpecifiers(SingletonList(ArrayRankSpecifier(SingletonSeparatedList<ExpressionSyntax>(OmittedArraySizeExpression())))))
                .WithVariables(SingletonSeparatedList(VariableDeclarator(Identifier(f)))))
                .AddModifiers(Token(SyntaxKind.PublicKeyword)));
        }

        // ctor: <P>_SIMD(int capacity = 1)
        var ctorStmts = new List<StatementSyntax>
        {
            IfStatement(BinaryExpression(SyntaxKind.LessThanExpression, IdentifierName("capacity"),
                LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(1))),
                Block(ExpressionStatement(AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, IdentifierName("capacity"),
                    LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(1))))))
        };
        foreach (var f in floatFields)
        {
            ctorStmts.Add(ExpressionStatement(AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, IdentifierName(f),
                ArrayCreationExpression(ArrayType(PredefinedType(Token(SyntaxKind.FloatKeyword)))
                    .WithRankSpecifiers(SingletonList(ArrayRankSpecifier(SingletonSeparatedList<ExpressionSyntax>(IdentifierName("capacity")))))))));
        }
        var ctor = ConstructorDeclaration(Identifier(simdName))
            .AddModifiers(Token(SyntaxKind.PublicKeyword))
            .WithParameterList(ParameterList(SingletonSeparatedList(
                Parameter(Identifier("capacity")).WithType(PredefinedType(Token(SyntaxKind.IntKeyword)))
                    .WithDefault(EqualsValueClause(LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(1)))))))
            .WithBody(Block(ctorStmts));
        members.Add(ctor);

        // EnsureCapacity
        if (floatFields.Length > 0)
        {
            var first = floatFields[0];
            var ensureStmts = new List<StatementSyntax>
            {
                IfStatement(BinaryExpression(SyntaxKind.LessThanOrEqualExpression, IdentifierName("needed"),
                    MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, IdentifierName(first), IdentifierName("Length"))),
                    Block(SingletonList<StatementSyntax>(ReturnStatement()))),

                LocalDeclarationStatement(VariableDeclaration(PredefinedType(Token(SyntaxKind.IntKeyword))).WithVariables(
                    SingletonSeparatedList(VariableDeclarator(Identifier("newCap")).WithInitializer(EqualsValueClause(
                        InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, IdentifierName("Math"), IdentifierName("Max")))
                        .WithArgumentList(ArgumentList(SeparatedList(new[]{
                            Argument(IdentifierName("needed")),
                            Argument(BinaryExpression(SyntaxKind.MultiplyExpression,
                                MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, IdentifierName(first), IdentifierName("Length")),
                                LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(2))))
                        }))))))))
            };
            foreach (var f in floatFields)
            {
                ensureStmts.Add(ExpressionStatement(InvocationExpression(
                    MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, IdentifierName("Array"), IdentifierName("Resize")))
                    .WithArgumentList(ArgumentList(SeparatedList(new[]{
                        Argument(IdentifierName(f)).WithRefKindKeyword(Token(SyntaxKind.RefKeyword)),
                        Argument(IdentifierName("newCap"))
                    })))));
            }

            var ensure = MethodDeclaration(PredefinedType(Token(SyntaxKind.VoidKeyword)), Identifier("EnsureCapacity"))
                .AddModifiers(Token(SyntaxKind.PrivateKeyword))
                .WithParameterList(ParameterList(SingletonSeparatedList(
                    Parameter(Identifier("needed")).WithType(PredefinedType(Token(SyntaxKind.IntKeyword))))))
                .WithBody(Block(ensureStmts));
            members.Add(ensure);
        }

        // Add(ref P p)
        var addBody = new List<StatementSyntax>();
        addBody.Add(ExpressionStatement(InvocationExpression(IdentifierName("EnsureCapacity"))
            .WithArgumentList(ArgumentList(SingletonSeparatedList(
                Argument(BinaryExpression(SyntaxKind.AddExpression, IdentifierName("_length"),
                    LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(1)))))))));

        addBody.Add(LocalDeclarationStatement(VariableDeclaration(PredefinedType(Token(SyntaxKind.IntKeyword)))
            .WithVariables(SingletonSeparatedList(VariableDeclarator(Identifier("i"))
                .WithInitializer(EqualsValueClause(IdentifierName("_length")))))));

        foreach (var f in floatFields)
        {
            addBody.Add(ExpressionStatement(AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                ElementAccessExpression(IdentifierName(f)).WithArgumentList(BracketedArgumentList(SingletonSeparatedList(Argument(IdentifierName("i"))))),
                MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, IdentifierName("p"), IdentifierName(f)))));
        }
        addBody.Add(ExpressionStatement(PostfixUnaryExpression(SyntaxKind.PostIncrementExpression, IdentifierName("_length"))));

        var add = MethodDeclaration(PredefinedType(Token(SyntaxKind.VoidKeyword)), Identifier("Add"))
            .AddModifiers(Token(SyntaxKind.PublicKeyword))
            .WithParameterList(ParameterList(SingletonSeparatedList(
                Parameter(Identifier("p")).WithType(IdentifierName(pName)).WithModifiers(TokenList(Token(SyntaxKind.RefKeyword))))))
            .WithBody(Block(addBody));
        members.Add(add);

        // struct <P>_SIMD : ISIMDParticle<P>
        var decl = StructDeclaration(Identifier(simdName))
            .AddModifiers(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.PartialKeyword))
            .WithMembers(List(members))
            .WithBaseList(BaseList(SingletonSeparatedList<BaseTypeSyntax>(
                SimpleBaseType(ParseTypeName($"ISIMDParticle<{pName}>")))));

        var plan = fb.AddMember(decl).Render();
        spc.AddSource(plan.FileName, plan.SourceText);
    }

    private static void EmitEffectSimdUpdate(SourceProductionContext spc, Compilation compilation, IMethodSymbol method, MethodDeclarationSyntax methodDecl)
    {
        var ns = method.ContainingType.ContainingNamespace?.ToDisplayString() ?? "Global";
        var typeName = method.ContainingType.Name;
        var isStruct = method.ContainingType.TypeKind == TypeKind.Struct;
        var simdName = ((INamedTypeSymbol)method.Parameters[0].Type).Name + "_SIMD";

        var model = compilation.GetSemanticModel(methodDecl.SyntaxTree);
        if (AstToIrPlanner.TryPlan(method, methodDecl, model, out var pName) is not IRProgram ir) return;

        var fb = new FileBuilder(typeName + ".SIMD.Update.g.cs", ns)
            .AddUsing("System")
            .AddUsing("System.Numerics")
            .AddUsing("Particle.Abstractions");

        // 參數：ref <Particle>_SIMD particle + 其他原本參數
        var paramNodes = new List<ParameterSyntax>
        {
            Parameter(Identifier("particle")).WithType(IdentifierName(simdName)).WithModifiers(TokenList(Token(SyntaxKind.RefKeyword)))
        };
        for (int i = 1; i < method.Parameters.Length; i++)
        {
            var p = method.Parameters[i];
            var t = p.Type.SpecialType switch
            {
                SpecialType.System_Single => PredefinedType(Token(SyntaxKind.FloatKeyword)) as TypeSyntax,
                SpecialType.System_Double => PredefinedType(Token(SyntaxKind.DoubleKeyword)) as TypeSyntax,
                _ => PredefinedType(Token(SyntaxKind.FloatKeyword))
            };
            paramNodes.Add(Parameter(Identifier(p.Name)).WithType(t));
        }

        var stmts = new List<StatementSyntax>
        {
            LocalDeclarationStatement(VariableDeclaration(PredefinedType(Token(SyntaxKind.IntKeyword))).WithVariables(SingletonSeparatedList(VariableDeclarator(Identifier("length")).WithInitializer(EqualsValueClause(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, IdentifierName("particle"), IdentifierName("Length"))))))),
            LocalDeclarationStatement(VariableDeclaration(PredefinedType(Token(SyntaxKind.IntKeyword))).WithVariables(SingletonSeparatedList(VariableDeclarator(Identifier("width")).WithInitializer(EqualsValueClause(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, GenericName(Identifier("Vector")).WithTypeArgumentList(TypeArgumentList(SingletonSeparatedList<TypeSyntax>(PredefinedType(Token(SyntaxKind.FloatKeyword))))), IdentifierName("Count"))))))),
            LocalDeclarationStatement(VariableDeclaration(PredefinedType(Token(SyntaxKind.IntKeyword))).WithVariables(SingletonSeparatedList(VariableDeclarator(Identifier("i")).WithInitializer(EqualsValueClause(LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(0)))))))
        };

        // 廣播 scalars → v_<name>
        foreach (var s in ir.Scalars.OrderBy(s => s, StringComparer.Ordinal))
        {
            var paramSym = method.Parameters.FirstOrDefault(pp => pp.Name == s);
            ExpressionSyntax src = IdentifierName(s);
            if (paramSym is IParameterSymbol ps && ps.Type.SpecialType == SpecialType.System_Double)
                src = CastExpression(PredefinedType(Token(SyntaxKind.FloatKeyword)), src);
            var vec = ObjectCreationExpression(GenericName(Identifier("Vector")).WithTypeArgumentList(TypeArgumentList(SingletonSeparatedList<TypeSyntax>(PredefinedType(Token(SyntaxKind.FloatKeyword)))))).WithArgumentList(ArgumentList(SingletonSeparatedList(Argument(src))));
            stmts.Add(LocalDeclarationStatement(VariableDeclaration(IdentifierName("var")).WithVariables(SingletonSeparatedList(VariableDeclarator(Identifier("v_"+s)).WithInitializer(EqualsValueClause(vec))))));
        }

        // 向量 for：載入 UsedFields → 計算 → CopyTo
        var loopBody = new List<StatementSyntax>();
        foreach (var f in ir.UsedFields.OrderBy(s => s, StringComparer.Ordinal))
        {
            loopBody.Add(LocalDeclarationStatement(VariableDeclaration(IdentifierName("var")).WithVariables(SingletonSeparatedList(VariableDeclarator(Identifier(f+"v")).WithInitializer(EqualsValueClause(ObjectCreationExpression(GenericName(Identifier("Vector")).WithTypeArgumentList(TypeArgumentList(SingletonSeparatedList<TypeSyntax>(PredefinedType(Token(SyntaxKind.FloatKeyword)))))).WithArgumentList(ArgumentList(SeparatedList(new[]{ Argument(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, IdentifierName("particle"), IdentifierName(f))), Argument(IdentifierName("i")) })))))))));
        }
        foreach (var a in ir.Statements)
        {
            loopBody.Add(ExpressionStatement(AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, IdentifierName(a.Field+"v"), VectorLowerer.Vec(a.Expr))));
        }
        foreach (var f in ir.AssignedFields.OrderBy(s => s, StringComparer.Ordinal))
        {
            loopBody.Add(ExpressionStatement(InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, IdentifierName(f+"v"), IdentifierName("CopyTo"))).WithArgumentList(ArgumentList(SeparatedList(new[]{ Argument(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, IdentifierName("particle"), IdentifierName(f))), Argument(IdentifierName("i")) })))));
        }
        var forVec = ForStatement(Block(loopBody)).WithCondition(BinaryExpression(SyntaxKind.LessThanOrEqualExpression, IdentifierName("i"), BinaryExpression(SyntaxKind.SubtractExpression, IdentifierName("length"), IdentifierName("width")))).WithIncrementors(SingletonSeparatedList<ExpressionSyntax>(AssignmentExpression(SyntaxKind.AddAssignmentExpression, IdentifierName("i"), IdentifierName("width"))));
        stmts.Add(forVec);

        // 尾段：純量計算
        var tail = new List<StatementSyntax>();
        foreach (var a in ir.Statements)
        {
            var lhs = ElementAccessExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, IdentifierName("particle"), IdentifierName(a.Field))).WithArgumentList(BracketedArgumentList(SingletonSeparatedList(Argument(IdentifierName("i")))));
            tail.Add(ExpressionStatement(AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, lhs, VectorLowerer.Scalar(a.Expr, "particle", "i"))));
        }
        var forTail = ForStatement(Block(tail)).WithCondition(BinaryExpression(SyntaxKind.LessThanExpression, IdentifierName("i"), IdentifierName("length"))).WithIncrementors(SingletonSeparatedList<ExpressionSyntax>(PostfixUnaryExpression(SyntaxKind.PostIncrementExpression, IdentifierName("i"))));
        stmts.Add(forTail);

        var updateBody = Block(stmts);
        var updateMethod = MethodDeclaration(PredefinedType(Token(SyntaxKind.VoidKeyword)), Identifier("Update"))
            .AddModifiers(Token(SyntaxKind.PublicKeyword))
            .WithParameterList(ParameterList(SeparatedList(paramNodes)))
            .WithBody(updateBody);

        // 直接在 partial 上實作 IParticleEffect<simdName>
        var baseList = BaseList(SingletonSeparatedList<BaseTypeSyntax>(SimpleBaseType(ParseTypeName($"IParticleEffect<{simdName}>"))));
        var partial = TypeMethodBuilders.CreatePartialType(isStruct ? "struct" : "class", typeName, [updateMethod], baseList);

        var outPlan = fb.AddMember(partial).Render();
        spc.AddSource(outPlan.FileName, outPlan.SourceText);
    }
}
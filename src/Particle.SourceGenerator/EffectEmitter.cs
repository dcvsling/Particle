// Particle.SourceGenerator/EffectEmitter.cs
#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Particle.SourceGenerator;

public static class EffectEmitter
{
    public static void EmitEffectSimdUpdate(
        SourceProductionContext spc,
        Compilation compilation,
        IMethodSymbol method,
        MethodDeclarationSyntax methodDecl)
    {
        var ns        = method.ContainingType.ContainingNamespace?.ToDisplayString() ?? "Global";
        var typeName  = method.ContainingType.Name;
        var isStruct  = method.ContainingType.TypeKind == TypeKind.Struct;
        var simdName  = ((INamedTypeSymbol)method.Parameters[0].Type).Name + "_SIMD";

        // 這裡僅保留流程化管線；IR 規劃與降階仍交給你現有的元件
        var model = compilation.GetSemanticModel(methodDecl.SyntaxTree);

        if (AstToIrPlanner.TryPlan(method, methodDecl, model, out var pName) is not IRProgram ir)
            return;

        var fb = new SimpleFileBuilder($"{typeName}.SIMD.Update.g.cs", ns)
            .AddUsing("System")
            .AddUsing("System.Numerics")
            .AddUsing("Particle.Abstractions");

        // 1) 組參數
        var parameters = BuildSignature(method, simdName);

        // 2) 前置區塊（length/width/i）
        var stmts = Prologue();

        // 3) 廣播 scalar 到 Vector<float>
        stmts.AddRange(BroadcastScalars(ir, method));

        // 4) 向量主迴圈：Load → Compute → CopyBack
        stmts.Add(VectorLoop(ir));

        // 5) 尾段純量
        stmts.Add(ScalarTail(ir));

        var updateMethod = MethodDeclaration(RoslynHelper.VoidType, Identifier("Update"))
            .AddModifiers(Token(SyntaxKind.PublicKeyword))
            .WithParameterList(ParameterList(SeparatedList(parameters)))
            .WithBody(Block(stmts));

        // 在 partial 上實作 IParticleEffect<TSimd>
        var baseList = BaseList(SingletonSeparatedList<BaseTypeSyntax>(
            SimpleBaseType(RoslynHelper.TypeId($"IParticleEffect<{simdName}>"))));

        var partial = (method.ContainingType.TypeKind == TypeKind.Struct)
            ? TypeDeclaration("struct", typeName, new[] { updateMethod }, baseList)
            : TypeDeclaration("class",  typeName, new[] { updateMethod }, baseList);

        fb.AddMember(partial);
        var outPlan = fb.Render();
        spc.AddSource(outPlan.FileName, outPlan.SourceText);
    }

    public static TypeDeclarationSyntax TypeDeclaration(
        string kind, string name, IEnumerable<MemberDeclarationSyntax> members, BaseListSyntax baseList)
    {
        var decl = kind switch
        {
            "struct" => (TypeDeclarationSyntax)StructDeclaration(Identifier(name)),
            _        => ClassDeclaration(Identifier(name))
        };
        return decl
            .AddModifiers(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.PartialKeyword))
            .WithMembers(List(members))
            .WithBaseList(baseList);
    }

    public static IEnumerable<ParameterSyntax> BuildSignature(IMethodSymbol method, string simdName)
    {
        // 第 0 個參數改為 ref <Particle>_SIMD
        yield return Parameter(Identifier("particle"))
            .WithType(RoslynHelper.TypeId(simdName))
            .WithModifiers(TokenList(Token(SyntaxKind.RefKeyword)));

        for (int i = 1; i < method.Parameters.Length; i++)
        {
            var p = method.Parameters[i];
            var t = p.Type.SpecialType switch
            {
                SpecialType.System_Single => RoslynHelper.FloatType as TypeSyntax,
                SpecialType.System_Double => RoslynHelper.DoubleType as TypeSyntax,
                _ => RoslynHelper.FloatType
            };
            yield return Parameter(Identifier(p.Name)).WithType(t);
        }
    }

    public static List<StatementSyntax> Prologue() => new()
    {
        // int length = particle.Length;
        RoslynHelper.IntLocal("length", RoslynHelper.Member(RoslynHelper.Id("particle"), "Length")),
        // int width = Vector<float>.Count;
        RoslynHelper.IntLocal("width",  RoslynHelper.Member(RoslynHelper.Generic("Vector", RoslynHelper.FloatType), "Count")),
        // int i = 0;
        RoslynHelper.IntLocal("i", RoslynHelper.Lit(0))
    };

    public static IEnumerable<StatementSyntax> BroadcastScalars(IRProgram ir, IMethodSymbol method)
    {
        foreach (var s in ir.Scalars.OrderBy(s => s, StringComparer.Ordinal))
        {
            var paramSym = method.Parameters.FirstOrDefault(pp => pp.Name == s);
            ExpressionSyntax src = RoslynHelper.Id(s);
            if (paramSym is { Type.SpecialType: SpecialType.System_Double })
                src = CastExpression(RoslynHelper.FloatType, src);

            // var v_<s> = new Vector<float>(src);
            yield return LocalDeclarationStatement(
                VariableDeclaration(IdentifierName("var"))
                    .WithVariables(SingletonSeparatedList(
                        VariableDeclarator(Identifier("v_"+s))
                            .WithInitializer(EqualsValueClause(NewFloat(src))))));
        }
    }

    public static StatementSyntax VectorLoop(IRProgram ir)
    {
        var loopBody = new List<StatementSyntax>();

        // 載入用到的欄位：var Xv = new Vector<float>(particle.X, i);
        foreach (var f in ir.UsedFields.OrderBy(s => s, StringComparer.Ordinal))
        {
            loopBody.Add(LocalDeclarationStatement(
                VariableDeclaration(IdentifierName("var"))
                    .WithVariables(SingletonSeparatedList(
                        VariableDeclarator(Identifier(f+"v"))
                            .WithInitializer(EqualsValueClause(
                                LoadFromArray(RoslynHelper.Member(RoslynHelper.Id("particle"), f), RoslynHelper.Id("i"))))))));
        }

        // 計算：Xv = <lowered expr>;
        foreach (var a in ir.Statements)
            loopBody.Add(RoslynHelper.AssignStmt(RoslynHelper.Id(a.Field+"v"), VectorLowerer.Vec(a.Expr)));

        // 回寫：Xv.CopyTo(particle.X, i);
        foreach (var f in ir.AssignedFields.OrderBy(s => s, StringComparer.Ordinal))
        {
            loopBody.Add(
                ExpressionStatement(
                    InvocationExpression(
                        RoslynHelper.Member(RoslynHelper.Id(f+"v"), "CopyTo"))
                    .WithArgumentList(ArgumentList(SeparatedList(new[]
                    {
                        Argument(RoslynHelper.Member(RoslynHelper.Id("particle"), f)),
                        Argument(RoslynHelper.Id("i"))
                    })))));
        }

        return ForStatement(Block(loopBody))
            .WithCondition(RoslynHelper.Le(RoslynHelper.Id("i"),
                RoslynHelper.Sub(RoslynHelper.Id("length"), RoslynHelper.Id("width"))))
            .WithIncrementors(SingletonSeparatedList<ExpressionSyntax>(
                AssignmentExpression(SyntaxKind.AddAssignmentExpression, RoslynHelper.Id("i"), RoslynHelper.Id("width"))));
    }

    public static StatementSyntax ScalarTail(IRProgram ir)
    {
        var tail = new List<StatementSyntax>();

        foreach (var a in ir.Statements)
        {
            var lhs = RoslynHelper.Element(
                RoslynHelper.Member(RoslynHelper.Id("particle"), a.Field),
                RoslynHelper.Id("i"));

            tail.Add(RoslynHelper.AssignStmt(
                lhs, VectorLowerer.Scalar(a.Expr, "particle", "i")));
        }

        return ForStatement(Block(tail))
            .WithCondition(RoslynHelper.Lt(RoslynHelper.Id("i"), RoslynHelper.Id("length")))
            .WithIncrementors(SingletonSeparatedList<ExpressionSyntax>(
                PostfixUnaryExpression(SyntaxKind.PostIncrementExpression, RoslynHelper.Id("i"))));
    }
    public static ExpressionSyntax NewFloat(ExpressionSyntax scalar)
        => ObjectCreationExpression(RoslynHelper.GenericType("Vector", RoslynHelper.FloatType))
            .WithArgumentList(ArgumentList(SingletonSeparatedList(Argument(scalar))));

    public static ExpressionSyntax LoadFromArray(ExpressionSyntax array, ExpressionSyntax index)
        => ObjectCreationExpression(RoslynHelper.GenericType("Vector", RoslynHelper.FloatType))
            .WithArgumentList(ArgumentList(SeparatedList(new[]
            {
                Argument(array),
                Argument(index)
            })));
}
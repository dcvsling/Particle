using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Particle.SourceGenerator;

internal static class AstToIrPlanner
{
    public static IRProgram? TryPlan(IMethodSymbol method, MethodDeclarationSyntax methodDecl, SemanticModel model, out string particleParamName)
    {
        particleParamName = method.Parameters[0].Name;
        if (methodDecl.Body is null) return null;

        var prog = new IRProgram();

        foreach (var stmt in methodDecl.Body.Statements)
        {
            if (stmt is not ExpressionStatementSyntax es) return null;
            if (es.Expression is not AssignmentExpressionSyntax assign) return null;
            if (assign.Kind() != SyntaxKind.SimpleAssignmentExpression) return null;

            if (assign.Left is not MemberAccessExpressionSyntax l ||
                l.Expression is not IdentifierNameSyntax pid ||
                pid.Identifier.ValueText != particleParamName ||
                l.Name is not IdentifierNameSyntax lf)
                return null;

            var lhsField = lf.Identifier.ValueText;
            var rhs = LowerExpr(assign.Right, method, model, particleParamName, prog);
            if (rhs is null) return null;

            prog.UsedFields.Add(lhsField);
            prog.AssignedFields.Add(lhsField);
            prog.Statements.Add(new IRAssign(lhsField, rhs));
        }

        foreach (var f in prog.UsedFields)
        {
            var fld = method.Parameters[0].Type.GetMembers().OfType<IFieldSymbol>().FirstOrDefault(m => m.Name == f);
            if (fld is null || fld.Type.SpecialType != SpecialType.System_Single) return null;
        }
        foreach (var s in prog.Scalars)
        {
            var mField = method.ContainingType.GetMembers().OfType<IFieldSymbol>().FirstOrDefault(m => m.Name == s);
            var mParam = method.Parameters.FirstOrDefault(pp => pp.Name == s && pp.Name != method.Parameters[0].Name);
            bool ok = false;
            if (mField is not null)
                ok = mField.Type.SpecialType is SpecialType.System_Single or SpecialType.System_Double;
            if (!ok && mParam is not null)
                ok = mParam.Type.SpecialType is SpecialType.System_Single or SpecialType.System_Double;
            if (!ok) return null;
        }

        return prog.Statements.Count == 0 ? null : prog;
    }

    private static IRExpr? LowerExpr(ExpressionSyntax expr, IMethodSymbol method, SemanticModel model, string particleParam, IRProgram prog)
    {
        switch (expr)
        {
            case LiteralExpressionSyntax lit when lit.IsKind(SyntaxKind.NumericLiteralExpression):
                return new IRConst(Convert.ToSingle(lit.Token.Value));

            case IdentifierNameSyntax id:
                var sym = model.GetSymbolInfo(id).Symbol;
                if (sym is IFieldSymbol fs && SymbolEqualityComparer.Default.Equals(fs.ContainingType, method.ContainingType))
                { prog.Scalars.Add(id.Identifier.ValueText); return new IRScalar(id.Identifier.ValueText); }
                if (sym is IParameterSymbol ps && !string.Equals(ps.Name, particleParam, StringComparison.Ordinal))
                { prog.Scalars.Add(ps.Name); return new IRScalar(ps.Name); }
                return null;

            case MemberAccessExpressionSyntax ma:
                if (ma.Expression is IdentifierNameSyntax pid && pid.Identifier.ValueText == particleParam && ma.Name is IdentifierNameSyntax f)
                { prog.UsedFields.Add(f.Identifier.ValueText); return new IRVarField(f.Identifier.ValueText); }
                return null;

            case ParenthesizedExpressionSyntax pe:
                return LowerExpr(pe.Expression, method, model, particleParam, prog);

            case PrefixUnaryExpressionSyntax pre when pre.IsKind(SyntaxKind.UnaryMinusExpression):
                var arg = LowerExpr(pre.Operand, method, model, particleParam, prog);
                return arg is null ? null : new IRBinary(SimdOp.Sub, new IRConst(0f), arg);

            case BinaryExpressionSyntax be:
                var l = LowerExpr(be.Left, method, model, particleParam, prog);
                var r = LowerExpr(be.Right, method, model, particleParam, prog);
                if (l is null || r is null) return null;
                if (be.IsKind(SyntaxKind.AddExpression)) return new IRBinary(SimdOp.Add, l, r);
                if (be.IsKind(SyntaxKind.SubtractExpression)) return new IRBinary(SimdOp.Sub, l, r);
                if (be.IsKind(SyntaxKind.MultiplyExpression)) return new IRBinary(SimdOp.Mul, l, r);
                if (be.IsKind(SyntaxKind.DivideExpression)) return new IRBinary(SimdOp.Div, l, r);
                return null;

            case InvocationExpressionSyntax call:
                string? name = null;
                if (call.Expression is IdentifierNameSyntax idname) name = idname.Identifier.ValueText;
                if (call.Expression is MemberAccessExpressionSyntax man)
                {
                    name = man.Name.Identifier.ValueText;
                    var xe = man.Expression.ToString();
                    if (!(xe.EndsWith("Math", StringComparison.Ordinal) || xe.EndsWith("MathF", StringComparison.Ordinal) || xe.EndsWith("System.Math", StringComparison.Ordinal) || xe.EndsWith("System.MathF", StringComparison.Ordinal)))
                        name = null;
                }
                if (name is null) return null;
                var args = call.ArgumentList.Arguments.Select(a => LowerExpr(a.Expression, method, model, particleParam, prog)).ToArray();
                if (args.Any(a => a is null)) return null;
                switch (name)
                {
                    case "Abs": return new IRUnary(SimdFunc.Abs, args![0]!);
                    case "Sqrt": return new IRUnary(SimdFunc.Sqrt, args![0]!);
                    case "Min": return new IRCall(SimdFunc.Min, args![0]!, args![1]!);
                    case "Max": return new IRCall(SimdFunc.Max, args![0]!, args![1]!);
                    case "Clamp":
                        var a0 = args![0]!; var a1 = args![1]!; var a2 = args![2]!;
                        return new IRCall(SimdFunc.Min, new IRCall(SimdFunc.Max, a0, a1), a2);
                    default: return null;
                }
        }
        return null;
    }
}

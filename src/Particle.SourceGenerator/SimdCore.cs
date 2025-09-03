namespace Particle.SourceGenerator;

public enum SimdOp { Add, Sub, Mul, Div }
public enum SimdFunc { Abs, Min, Max, Sqrt }

public abstract record IRExpr;
public sealed record IRVarField(string Name) : IRExpr;               // v.<field>
public sealed record IRScalar(string Name) : IRExpr;                 // 參數或容器欄位
public sealed record IRConst(float Value) : IRExpr;                  // 常數
public sealed record IRUnary(SimdFunc Func, IRExpr Arg) : IRExpr;    // Abs/Sqrt
public sealed record IRBinary(SimdOp Op, IRExpr Left, IRExpr Right) : IRExpr; // + - * /
public sealed record IRCall(SimdFunc Func, IRExpr A, IRExpr B) : IRExpr;      // Min/Max

public sealed record IRAssign(string Field, IRExpr Expr);

public sealed class IRProgram
{
    public System.Collections.Generic.HashSet<string> UsedFields { get; } = new(System.StringComparer.Ordinal);
    public System.Collections.Generic.HashSet<string> AssignedFields { get; } = new(System.StringComparer.Ordinal);
    public System.Collections.Generic.HashSet<string> Scalars { get; } = new(System.StringComparer.Ordinal);
    public System.Collections.Generic.List<IRAssign> Statements { get; } = new();
}

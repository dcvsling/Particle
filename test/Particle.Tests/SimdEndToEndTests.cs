using System.Linq;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Particle.SourceGenerator;
using Xunit;

namespace Particle.SourceGenerator.Tests;

public class SimdEndToEndTests
{
    private static CSharpCompilation CreateInput(string src)
    {
        var tree = CSharpSyntaxTree.ParseText(src);
        var refs = new[]
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Runtime.CompilerServices.IsExternalInit).Assembly.Location),
        };
        return CSharpCompilation.Create("Demo", new[] { tree }, refs, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }

    [Fact]
    public void Should_Generate_SoA_And_Vector_Update_From_Generic_AST()
    {
        var input = @"
using Partice;

namespace Demo;

public partial struct P { public float X, Y, VX, VY, LIFE; }

public partial struct Eff
{
    public float G;

    [Effect]
    private void Step(ref P v, float DT)
    {
        v.VY   = v.VY - G * DT;
        v.Y    = v.Y  - v.VY * DT;
        v.LIFE = MathF.Clamp(v.LIFE - DT, 0f, 1f);
        v.VX   = MathF.Max(0f, v.VX - MathF.Abs(-DT) * MathF.Sqrt(1f));
        v.X    = v.X + v.VX * DT;
    }
}";
        var comp = CreateInput(input);
        GeneratorDriver driver = CSharpGeneratorDriver.Create(new EffectGenerator());
        driver = driver.RunGeneratorsAndUpdateCompilation(comp, out var outComp, out var diags, CancellationToken.None);

        diags.Should().OnlyContain(d => d.Severity != DiagnosticSeverity.Error);

        var run = driver.GetRunResult();
        var res = run.Results.Single();
        res.GeneratedSources.Should().NotBeEmpty();

        var src = string.Join("\n----\n", res.GeneratedSources.Select(s => s.SourceText.ToString()));

        // 基本格式：換行 using
        src.Should().Contain("using System;\nusing System.Numerics;");

        // 產生 SoA 型別
        src.Should().Contain("struct P_SIMD");

        // 產生向量化 Update 並含 Vector<float> 與 CopyTo
        src.Should().Contain("void Update(ref P_SIMD particle, float DT)");
        src.Should().Contain("Vector<float>");
        src.Should().Contain(".CopyTo(particle");

        // 支援 Abs/Min/Max/Sqrt（以 Vector.* 映射）與 Clamp 展開
        src.Should().Contain("Vector.Abs");
        src.Should().Contain("Vector.Max");
        src.Should().Contain("Vector.SquareRoot");

        // 介面適配器
        src.Should().Contain("struct Eff_SIMDAdapter");
        src.Should().Contain("IParticleEffect<P_SIMD>");
    }
}

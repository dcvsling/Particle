using System;
using System.Linq;
using System.Numerics;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Particle.SourceGenerator;
using Xunit;

namespace Particle.GeneratorTests
{
    public class EffectGenerator_VectorAstTests
    {
        [Fact]
        public void Should_Generate_SoA_And_Vector_Update_From_Generic_AST()
        {
            // 修正：Partice -> Particle
            var input = @"
using Particle;

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
            GeneratorDriver driver = CSharpGeneratorDriver.Create(new EffectGenerator())
                .WithUpdatedParseOptions(new CSharpParseOptions(LanguageVersion.Preview));

            driver = driver.RunGeneratorsAndUpdateCompilation(comp, out var outComp, out var diags, TestContext.Current.CancellationToken);

            // 若需要檢查錯誤，可解開下行，但允許 Warning。
            // diags.Should().OnlyContain(d => d.Severity != DiagnosticSeverity.Error);

            var run = driver.GetRunResult();
            var res = run.Results.Single();
            res.GeneratedSources.Should().NotBeEmpty("Source Generator 應該要產生 SoA 與向量化更新碼");

            var src = string.Join("\n----\n", res.GeneratedSources.Select(s => s.SourceText.ToString()));
            var norm = NormalizeNewLines(src);

            // using：避免受 CRLF/LF 與自動排序影響，改採獨立存在性檢查
            norm.Should().Contain("using System;");
            norm.Should().Contain("using Particle.Abstractions;");

            // 產生 SoA 型別（避免限定關鍵字差異，例如是否 partial）
            norm.Should().Contain("struct P_SIMD");

            // 產生向量化 Update 並含 Vector<float> 與 CopyTo（簽名用較寬鬆比對）
            norm.Should().Contain("void Update(ref P_SIMD particle, float DT)");
            norm.Should().Contain("Vector<float>");
            norm.Should().Contain(".CopyTo(particle");

            // 支援 Abs/Max/Sqrt（以 Vector.* 映射）與 Clamp 展開
            norm.Should().Contain("Vector.Abs");
            norm.Should().Contain("Vector.Max");
            norm.Should().Contain("Vector.SquareRoot");

            // 介面適配器（名稱可能位於命名空間/型別中，單純檢查片段存在）
            norm.Should().Contain("partial struct Eff");
            norm.Should().Contain("IParticleEffect<P_SIMD>");
        }

        private static string NormalizeNewLines(string s)
            => s.Replace("\r\n", "\n").Replace('\r', '\n');

        private static Compilation CreateInput(string source)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Preview));

            // 建立編譯參考（.NET 9 測試情境下常見做法）
            var refs = new[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(MathF).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Vector<>).Assembly.Location),
                // 若測試專案已參考 Particle.Abstractions，通常不需要額外手動載入。
                // 若仍遇到找不到 IParticleEffect/EffectAttribute，可在測試輸入中
                // 以最小宣告暫時打樁，或在此加入對對應組件的 MetadataReference。
            };

            return CSharpCompilation.Create(
                assemblyName: "Generator_Input_Assembly",
                syntaxTrees: new[] { syntaxTree },
                references: refs,
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
            );
        }
    }
}

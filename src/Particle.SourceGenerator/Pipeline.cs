// Particle.SourceGenerator/Pipeline.cs
#nullable enable
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Particle.SourceGenerator;

public sealed partial class EffectGenerator
{
    // ---------------- Pipeline Orchestration ----------------

    public static class Pipeline
    {
        public static void Generate(
            SourceProductionContext spc,
            Compilation comp,
            ImmutableArray<(IMethodSymbol sym, MethodDeclarationSyntax method)> methods)
        {
            var emittedSoA = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);

            foreach (var (methodSym, methodDecl) in methods.Distinct())
            {
                var particleType = (INamedTypeSymbol)methodSym.Parameters[0].Type;

                if (!emittedSoA.Contains(particleType))
                {
                    emittedSoA.Add(particleType);
                    ParticleSoAEmitter.EmitParticleSoA(spc, particleType);
                }

                EffectEmitter.EmitEffectSimdUpdate(spc, comp, methodSym, methodDecl);
            }
        }
    }
}

// Source Generator: reads user UpdateScalar(ref T v) and emits SoA layout + SIMD kernel + dispatcher
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Particle.SourceGenerator;

internal sealed record Candidate(INamedTypeSymbol EffectType,
                                 IMethodSymbol Method,
                                 INamedTypeSymbol ParticleType);
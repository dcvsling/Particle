using Microsoft.Extensions.DependencyInjection;
using Particle.Abstractions;

namespace Particle.Builder;

public class ParticleSystemBuilder<T> where T : struct
{
    internal ParticleSystemBuilder(ParticleSystemBuilder builder)
    {
        Builder = builder;
    }

    public ParticleSystemBuilder Builder { get; }

    public ParticleSystemBuilder<T> WithEmitter<TEmitter>() where TEmitter : class, IParticleEmitter<T>
    {
        Builder.Services.AddSingleton<IParticleEmitter<T>, TEmitter>();
        return this;
    }

    public ParticleSystemBuilder<T> WithEffect<TEffect>() where TEffect : class, IParticleEffect<T>
    {
        Builder.Services.AddSingleton<IParticleEffect<T>, TEffect>();
        return this;
    }
    public ParticleSystemBuilder<T> WithDrawer<TDrawer>() where TDrawer : class, IParticleDrawer<T>
    {
        Builder.Services.AddSingleton<IParticleDrawer<T>, TDrawer>();
        return this;
    }
}
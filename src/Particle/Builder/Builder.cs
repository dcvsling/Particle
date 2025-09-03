using Microsoft.Extensions.DependencyInjection;
using Particle.Abstractions;

namespace Particle.Builder;

public class ParticleSystemBuilder
{
    internal ParticleSystemBuilder(IServiceCollection services)
    {
        Services = services;
    }

    public IServiceCollection Services { get; }


    public ParticleSystemBuilder AddComponent(IParticleComponent component)
    {
        Services.AddSingleton(component);
        return this;
    }

    public ParticleSystemBuilder<T> AddParticle<T>() where T : struct
    {
        Services.AddSingleton<IParticleComponent, ParticleComponent<T>>();
        return new ParticleSystemBuilder<T>(this);
    }
}

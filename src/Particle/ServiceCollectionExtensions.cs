using Particle.Builder;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static ParticleSystemBuilder AddParticleSystem(this IServiceCollection services)
        => new ParticleSystemBuilder(services);
}   
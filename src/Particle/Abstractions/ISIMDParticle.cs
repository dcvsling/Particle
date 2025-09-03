namespace Particle.Abstractions;


public interface ISIMDParticle<T> where T : struct
{
    int Length { get; }
    void Add(ref T p);
}

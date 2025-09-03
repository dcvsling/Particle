namespace Particle.Abstractions;

public interface IParticleEmitter<T> where T : struct
{
    void Emit(long deltaTime, Buffer<T> buffer);
}

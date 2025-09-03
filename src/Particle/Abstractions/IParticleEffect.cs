namespace Particle.Abstractions;


public interface IParticleEffect<T> where T : struct
{
    void Update(ref T particle, float delta);
}

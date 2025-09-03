using Particle.Abstractions;

namespace Particle.Internal;

internal class ParticleEmitTrigger
{
    void Subscribe(Action trigger)
    {
        
    }
}




/// <summary>
/// Generates particles at a specified rate using a factory function.
/// </summary>
internal class Emitter<T> : IParticleEmitter<T> where T : struct
{
    private readonly RefAction<T> _particleFactory;
    private float _accumulated;

    /// <summary>
    /// Emission rate in particles per second.
    /// </summary>
    public long Rate { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Emitter"/> class.
    /// </summary>
    /// <param name="particleFactory">Factory function to create new particles.</param>
    /// <param name="rate">Emission rate in particles per second.</param>
    public Emitter(RefAction<T> particleFactory, long rate)
    {
        _particleFactory = particleFactory ?? throw new ArgumentNullException(nameof(particleFactory));
        Rate = rate;
        _accumulated = 0f;
    }

    /// <summary>
    /// Emits new particles according to the emission rate and elapsed time.
    /// </summary>
    /// <param name="deltaTime">Elapsed time in seconds.</param>
    /// <returns>Sequence of newly generated particles.</returns>
    public void Emit(long deltaTime, Buffer<T> buffer)
    {
        _accumulated += deltaTime;
        using var iterator = buffer.Tail;
        while (_accumulated > Rate)
        {
            _accumulated -= Rate;
            if (!iterator.MoveNext())
                break;
            _particleFactory(ref iterator.Current);
        }
    }
}

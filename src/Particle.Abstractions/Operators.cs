using System.Numerics;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework.Graphics;

namespace Particle.Abstractions;

public delegate void RefAction<T>(ref T value) where T : struct;
public delegate void Lerp<T>(ref T value, float delta) where T : struct;
public static class EfffectSpanExtensions
{
    extension<T>(IParticleEffectSpan<T> @this) where T : struct
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IParticleEffectSpan<T> Append(IParticleEffectSpan<T> span)
            => new ConcatParticleEffectSpan<T>(@this, span);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IParticleEffectSpan<T> Span(long timeSpan, Action<IParticleEffectBuilder<T>> configure)
        {
            var builder = new SpanParticleEffectBuilder<T>();
            configure(builder);
            return @this.Append(builder.Build(timeSpan));
        }
    }
    internal sealed class SpanParticleEffectBuilder<T>() : IParticleEffectBuilder<T> where T : struct
    {
        private readonly List<IParticleEffect<T>> _effects = new();
        public IParticleEffectBuilder<T> Add(IParticleEffect<T> effect)
        {
            _effects.Add(effect);
            return this;
        }

        public IParticleEffectSpan<T> Build(long span = 1000)
            => new ParticleEffectSpan<T>(span, _effects);
    }
    internal sealed class ParticleEffectSpan<T>(long span, params IEnumerable<IParticleEffect<T>> effects) : IParticleEffectSpan<T> where T : struct
    {
        public long Span => span;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IParticleEffectSpan<T> Append(IParticleEffectSpan<T> span)
            => new ConcatParticleEffectSpan<T>(this, span);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Update(ref T particle, long deltaTime)
        {
            var delta = Math.Min(deltaTime, span) / span;

            var enumerator = effects.GetEnumerator();
            while (enumerator.MoveNext())
                enumerator.Current.Update(ref particle, delta);
        }
    }
    internal class ConcatParticleEffectSpan<T>(IParticleEffectSpan<T> last, IParticleEffectSpan<T> next) : IParticleEffectSpan<T> where T : struct
    {
        public long Span => last.Span + next.Span;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IParticleEffectSpan<T> Append(IParticleEffectSpan<T> span)
            => new ConcatParticleEffectSpan<T>(this, span);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Update(ref T particle, long deltaTime)
        {
            if (deltaTime <= 0) return;
            last.Update(ref particle, deltaTime);
            deltaTime -= last.Span;
            if (deltaTime > 0)
                next.Update(ref particle, deltaTime);
        }
    }

    
}

public static class EffectExtensions
{
    internal static Lerp<Particle> DefaultBehavoior => (ref Particle particle, float delta) =>
    {
        particle.Ratation += particle.RotateSpeed * delta;

        particle.Velocity += particle.Acceleration * delta;
        particle.Position += particle.Velocity * delta;
    };
    public static IParticleEffect<Particle> Create(Lerp<Particle> lerp)
        => new ParticleEffect(lerp);

    extension(IParticleEffect<Particle> @this)
    {

    }
    
}

public static class Rotate
{

}

public static class Scale
{

}

public static class Count
{
    
}

public static class Each
{
    
}
using System.Buffers;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Particle.Abstractions;

public interface IParticleEffect<T> where T : struct
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void Update(ref T particle, float delta);
}

internal class ParticleEffect(Lerp<Particle> lerp) : IParticleEffect<Particle>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Update(ref Particle particle, float delta)
        => lerp(ref particle, delta);
}
public interface IParticleEffectSpan<T> where T : struct
{
    long Span { get; }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void Update(ref T particle, long deltaTime);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    IParticleEffectSpan<T> Append(IParticleEffectSpan<T> span);
}



internal class ParticleComponent<T> : DrawableGameComponent where T : struct
{
    public ParticleComponent(Game game) : base(game)
    {
    }
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
    }
    public override void Draw(GameTime gameTime)
    {
        base.Draw(gameTime);
    }
}

public interface IParticleEffectBuilder<T>
    where T : struct
{
    IParticleEffectBuilder<T> Add(IParticleEffect<T> effect);
    IParticleEffectSpan<T> Build(long span);
}
public struct Particle
{
    public Texture2D Texture;
    public Vector2 Position;
    public Vector2 Velocity;
    public Vector2 Acceleration;
    public bool IsActivated;
    public Vector2 Size;
    public Vector2 Scale;
    public float Ratation;
    public float RotateSpeed;
    public Color BackgroundColor;
    public float Opacity;
}
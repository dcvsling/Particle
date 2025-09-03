using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Particle.Abstractions;

namespace Particle;

internal class ParticleComponent<T>(
    IParticleEmitter<T> emitter,
    IParticleEffect<T> effect,
    IParticleDrawer<T> drawer) : IParticleComponent where T : struct
{
    private readonly Buffer<T> buffer = new Buffer<T>(1024);
    private long lastUpdateTime = 0;

    public void Draw(SpriteBatch spriteBatch)
    {
        var iterator = buffer.Head; 
        while(iterator.MoveNext())
            drawer.Draw(in iterator.Current, spriteBatch);
    }

    public void Update(GameTime gameTime)
    {
        lastUpdateTime = lastUpdateTime == 0 ? gameTime.TotalGameTime.Ticks : lastUpdateTime;
        var deltaTime = (gameTime.TotalGameTime.Ticks - lastUpdateTime) / TimeSpan.TicksPerMillisecond;
        emitter.Emit(deltaTime, buffer);
        var iterator = buffer.Head;
        while(iterator.MoveNext())
            effect.Update(ref iterator.Current, deltaTime);
    }
}


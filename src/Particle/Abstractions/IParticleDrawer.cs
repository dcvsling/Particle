using Microsoft.Xna.Framework.Graphics;

namespace Particle.Abstractions;

public interface IParticleDrawer<T> where T : struct
{
    void Draw(in T particle, SpriteBatch spriteBatch);
}

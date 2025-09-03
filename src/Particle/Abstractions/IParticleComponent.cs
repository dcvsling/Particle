using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Particle.Abstractions;

public interface IParticleComponent
{
    void Update(GameTime gameTime);
    void Draw(SpriteBatch spriteBatch);
}

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Particle.Abstractions;

namespace Particle
{
    /// <summary>
    /// Manages multiple emitters and performs updating and rendering of particles.
    /// </summary>
    public class ParticleSystem
    {
        private readonly List<IParticleComponent> _components = new();
        /// <summary>
        /// Adds an emitter to the system.
        /// </summary>
        public void AddCompnent(IParticleComponent component)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));
            _components.Add(component);
        }

        /// <summary>
        /// Adds a custom particle effect pipeline to the system.
        /// </summary>
        /// <summary>
        /// Updates the particle system state.
        /// </summary>
        /// <param name="deltaTime">Elapsed time in seconds since last update.</param>
        public void Update(GameTime gameTime)
        {
            foreach (var processor in _components)
            {
                processor.Update(gameTime);
            }
        }

        /// <summary>
        /// Draws all active particles using the provided SpriteBatch and texture.
        /// </summary>
        /// <param name="spriteBatch">SpriteBatch instance for rendering.</param>
        /// <param name="texture">Texture for drawing particles.</param>
        public void Draw(SpriteBatch spriteBatch, Texture2D texture)
        {
            if (spriteBatch == null) throw new ArgumentNullException(nameof(spriteBatch));
            if (texture == null) throw new ArgumentNullException(nameof(texture));

            foreach (var c in _components)
            {
                c.Draw(spriteBatch);
            }
        }
    }
}
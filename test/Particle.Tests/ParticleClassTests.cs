using Xunit;
using Microsoft.Xna.Framework;
using Particle;

namespace Particle.Tests
{
    public class ParticleClassTests
    {
        [Fact]
        public void Constructor_InitializesProperties()
        {
            var position = new Vector2(1f, 2f);
            var velocity = new Vector2(3f, 4f);
            var acceleration = new Vector2(5f, 6f);
            const float lifetime = 10f;
            var color = Color.Red;

            var particle = new Particle(position, velocity, acceleration, lifetime, color);

            Assert.Equal(position, particle.Position);
            Assert.Equal(velocity, particle.Velocity);
            Assert.Equal(acceleration, particle.Acceleration);
            Assert.Equal(lifetime, particle.Lifetime);
            Assert.Equal(0f, particle.Age);
            Assert.Equal(color, particle.Color);
        }

        [Fact]
        public void IsAliveAndLifeRemaining_Behavior()
        {
            const float lifetime = 5f;
            var particle = new Particle(Vector2.Zero, Vector2.Zero, Vector2.Zero, lifetime, Color.White);

            particle.Age = 2f;
            Assert.True(particle.IsAlive);
            Assert.Equal(3f, particle.LifeRemaining);

            particle.Age = lifetime;
            Assert.True(particle.IsAlive);
            Assert.Equal(0f, particle.LifeRemaining);
        }
    }
}
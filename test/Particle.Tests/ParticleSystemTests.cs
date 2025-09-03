// using System.Linq;
// using Xunit;
// using Microsoft.Xna.Framework;
// using Particle;
// using Microsoft.Xna.Framework.Graphics;

// namespace Particle.Tests
// {
//     public class ParticleSystemTests
//     {
//         [Fact]
//         public void Update_AddsAndUpdatesParticles()
//         {
//             var emitter = new Emitter<Particle>(
//                 (ref Particle p) => p = new Particle(Vector2.Zero, Vector2.Zero, Vector2.Zero, 5f, Color.White),
//                 2f);
//             var system = new ParticleSystem();
//             system.AddEmitter(emitter);

//             system.Update(1f);
//             Assert.Equal(2, system.Particles.Length);

//             var p = system.Particles[0];
//             Assert.Equal(1f, p.Age);
//             Assert.Equal(Vector2.Zero, p.Position);
//         }

//         [Fact]
//         public void Update_RemovesExpiredParticles()
//         {
//             var emitter = new Emitter<Particle>(
//                 (ref Particle p) => p = new Particle(Vector2.Zero, Vector2.Zero, Vector2.Zero, 1f, Color.White),
//                 2f);
//             var system = new ParticleSystem();
//             system.AddEmitter(emitter);

//             system.Update(1f);
//             Assert.Equal(2, system.Particles.Length);

//             system.Update(1f);
//             Assert.Equal(0, system.Particles.Length);
//         }

//         [Fact]
//         public void Draw_NullSpriteBatch_ThrowsArgumentNullException()
//         {
//             var system = new ParticleSystem();
//             Assert.Throws<ArgumentNullException>(() => system.Draw(default!, default!));
//         }
//     }
// }
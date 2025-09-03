// using System;
// using System.Linq;
// using Xunit;
// using Microsoft.Xna.Framework;
// using Particle;

// namespace Particle.Tests
// {
//     public class EmitterTests
//     {
//         [Fact]
//         public void Constructor_NullFactory_Throws()
//             => Assert.Throws<ArgumentNullException>(() => new Emitter<Particle>(default!, 1f));

//         [Fact]
//         public void Emit_GeneratesCorrectCountBasedOnRate()
//         {
//             int count = 0;
//             RefAction<Particle> factory = (ref Particle p) => {  count++; p = new Particle(Vector2.Zero, Vector2.Zero, Vector2.Zero, 1f, Color.White); };
//             var emitter = new Emitter<Particle>(factory, 10f);

//             var particles = emitter.Emit(0.5f);
//             Assert.Equal(5, particles.Length);
//             Assert.Equal(5, count);
//         }

//         [Fact]
//         public void Emit_AccumulatesFractionalEmissions()
//         {
//             int count = 0;
//             RefAction<Particle> factory = (ref Particle p) => { count++; p = new Particle(Vector2.Zero, Vector2.Zero, Vector2.Zero, 1f, Color.White); };
//             var emitter = new Emitter<Particle>(factory, 1f);

//             // first 0.6 seconds => 0 particles, leftover 0.6
//             var first = emitter.Emit(0.6f);
//             Assert.Equal(0, first.Length);

//             // next 0.5 seconds => (0.6+0.5)*1 = 1.1 => 1 particle
//             var second = emitter.Emit(0.5f);
//             Assert.Equal(1, second.Length);
//         }
//     }
// }
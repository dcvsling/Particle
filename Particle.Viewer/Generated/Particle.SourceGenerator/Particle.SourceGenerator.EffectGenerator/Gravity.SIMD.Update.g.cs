using System;
using System.Numerics;
using Particle.Abstractions;

namespace Particle.Viewer
{
    partial struct Gravity : IParticleEffect<Particle2D_SIMD>
    {
        public void Update(ref Particle2D_SIMD particle, float dt)
        {
            int length = particle.Length;
            int width = Vector<float>.Count;
            int i = 0;
            var v_G = new Vector<float>(G);
            var v_dt = new Vector<float>(dt);
            for (; i <= length - width; i += width)
            {
                var VYv = new Vector<float>(particle.VY, i);
                var Yv = new Vector<float>(particle.Y, i);
                VYv = VYv - v_G * v_dt;
                Yv = Yv - VYv * v_dt;
                VYv.CopyTo(particle.VY, i);
                Yv.CopyTo(particle.Y, i);
            }

            for (; i < length; i++)
            {
                particle.VY[i] = particle.VY[i] - G * dt;
                particle.Y[i] = particle.Y[i] - particle.VY[i] * dt;
            }
        }
    }
}
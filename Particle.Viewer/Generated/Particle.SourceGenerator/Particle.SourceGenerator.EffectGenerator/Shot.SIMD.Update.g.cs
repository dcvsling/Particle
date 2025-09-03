using System;
using System.Numerics;
using Particle.Abstractions;

namespace Particle.Viewer
{
    partial struct Shot : IParticleEffect<Particle2D_SIMD>
    {
        public void Update(ref Particle2D_SIMD particle, float dt)
        {
            int length = particle.Length;
            int width = Vector<float>.Count;
            int i = 0;
            var v_SPEED = new Vector<float>(SPEED);
            var v_dt = new Vector<float>(dt);
            for (; i <= length - width; i += width)
            {
                var Xv = new Vector<float>(particle.X, i);
                Xv = Xv + v_SPEED * v_dt;
                Xv.CopyTo(particle.X, i);
            }

            for (; i < length; i++)
            {
                particle.X[i] = particle.X[i] + SPEED * dt;
            }
        }
    }
}
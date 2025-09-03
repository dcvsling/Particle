using System;
using Particle.Abstractions;

namespace Particle.Viewer
{
    public partial struct Particle2D_SIMD : ISIMDParticle<Particle2D>
    {
        private int _length;
        public int Length => _length;

        public float[] LIFE;
        public float[] VX;
        public float[] VY;
        public float[] X;
        public float[] Y;
        public Particle2D_SIMD(int capacity = 1)
        {
            if (capacity < 1)
            {
                capacity = 1;
            }

            LIFE = new float[capacity];
            VX = new float[capacity];
            VY = new float[capacity];
            X = new float[capacity];
            Y = new float[capacity];
        }

        private void EnsureCapacity(int needed)
        {
            if (needed <= LIFE.Length)
            {
                return;
            }

            int newCap = Math.Max(needed, LIFE.Length * 2);
            Array.Resize(ref LIFE, newCap);
            Array.Resize(ref VX, newCap);
            Array.Resize(ref VY, newCap);
            Array.Resize(ref X, newCap);
            Array.Resize(ref Y, newCap);
        }

        public void Add(ref Particle2D p)
        {
            EnsureCapacity(_length + 1);
            int i = _length;
            LIFE[i] = p.LIFE;
            VX[i] = p.VX;
            VY[i] = p.VY;
            X[i] = p.X;
            Y[i] = p.Y;
            _length++;
        }
    }
}
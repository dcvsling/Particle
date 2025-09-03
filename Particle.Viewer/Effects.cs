using Particle.Abstractions;

namespace Particle.Viewer;

public partial struct Particle2D { public float X,Y,VX,VY,LIFE;  }

public partial struct Gravity(float g) : IParticleEffect<Particle2D>
{
    public float G = g;
    [Effect]
    public void Update(ref Particle2D v, float dt)
    {
        v.VY = v.VY - G * dt;
        v.Y  = v.Y  - v.VY * dt;
    }
}

public partial struct Shot(float speed) : IParticleEffect<Particle2D>
{
    public float SPEED = speed;
    [Effect]
    public void Update(ref Particle2D v, float dt)
    {
        v.X = v.X + SPEED * dt;
    }
}
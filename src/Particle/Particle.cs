using System;
using Microsoft.Xna.Framework;

namespace Particle;

// public struct Particle
// {
//     public Texture2D Texture;
//     public Vector2 Position;
//     public Vector2 Velocity;
//     public Vector2 Acceleration;
//     public bool IsActivated;
//     public Vector2 Size;
//     public Vector2 Scale;
//     public float Ratation;
//     public float RotateSpeed;
//     public Color BackgroundColor;
//     public float Opacity;
// }


/// <summary>
/// Represents a particle with physical properties and lifecycle.
/// </summary>
public struct Particle(Vector2 position, Vector2 velocity, Vector2 acceleration, float lifetime, Color color)
{
    /// <summary>
    /// Current position of the particle.
    /// </summary>
    public Vector2 Position = position;

    /// <summary>
    /// Current velocity of the particle.
    /// </summary>
    public Vector2 Velocity = velocity;

    /// <summary>
    /// Acceleration applied to the particle each update.
    /// </summary>
    public Vector2 Acceleration = acceleration;

    /// <summary>
    /// Total lifespan of the particle in seconds.
    /// </summary>
    public float Lifetime = lifetime;

    /// <summary>
    /// Time elapsed since the particle was created.
    /// </summary>
    public float Age = 0f;

    /// <summary>
    /// Current display color of the particle.
    /// </summary>
    public Color Color = color;

    /// <summary>
/// Gets a value indicating whether the particle is still alive (Age less than or equal to Lifetime).
/// </summary>
public bool IsAlive => Age <= Lifetime;

    /// <summary>
    /// Gets the remaining lifetime of the particle (clamped at zero).
    /// </summary>
    public float LifeRemaining => MathF.Max(Lifetime - Age, 0f);
}

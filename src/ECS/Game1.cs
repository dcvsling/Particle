using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ECS;

internal class World : DrawableGameComponent
{
    public World(Game game) : base(game)
    {
    }

}



public interface IEntity : IEquatable<IEntity>
{
    object Id { get; }
}

public struct Entity<T>(T id) : IEntity
{
    public object Id { get; } = id;

    public bool Equals(IEntity other)
        => other.Id == Id;
}


public interface IComponent<T> where T : struct
{
    void Add(T component);

    void Update(float deltaTime);

    Span<T>.Enumerator Get(IEntity entity);

    Span<T>.Enumerator GetEnumerator();
}
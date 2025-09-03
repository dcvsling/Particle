namespace GUI;

/// <summary>
/// Interface for integrating GUI controls with an ECS world.
/// </summary>
public interface IEcsGuiLoader
{
    /// <summary>
    /// Load or update GUI controls into the given ECS world/context.
    /// </summary>
    /// <param name="world">ECS world or context instance.</param>
    void Load(object world);
}
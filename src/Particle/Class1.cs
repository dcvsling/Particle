using System.Buffers;
using System.Runtime.CompilerServices;
using Particle.Abstractions;

namespace Particle;


internal ref struct Particles<T>(int capacity) : IDisposable where T : struct 
{
    public ref T this[int index] => ref _owner.Memory.Span[index];
    public int Capacity => capacity;
    private IMemoryOwner<T> _owner = MemoryPool<T>.Shared.Rent(capacity);
    public Span<T>.Enumerator GetEnumerator() => _owner.Memory.Span.GetEnumerator();

    public void Dispose()
        => _owner.Dispose();
}


using System.Buffers;

namespace Particle;


public class Buffer<T>(int capacity) : IDisposable where T : struct
{
    private int _head = 0;
    private int _tail = 0;
    private int _capacity = capacity;
    private int _actived => (_head - _tail + _capacity) % _capacity;
    private readonly IMemoryOwner<T> _owners = MemoryPool<T>.Shared.Rent(capacity);
    public Iterator Head => new Iterator(
        _owners.Memory.Span, _head, _actived, _capacity, () => _head = (_head + 1) % _capacity);
    public Iterator Tail => new Iterator(
        _owners.Memory.Span, _tail, _capacity - _actived, _capacity, () => _tail = (_tail + 1) % _capacity);            
    public Iterator GetEnumerator() => new Iterator(
        _owners.Memory.Span, 0, _capacity, _capacity, () => {});
   public void Dispose()
    {
        _owners.Dispose();
    }

    public ref struct Iterator(
        Span<T> particles, int from, int length, int capacity,
        Action nextCallBack) : IDisposable
    {
        private Span<T> particles = particles;
        private int _cursor = -1;

        public bool MoveNext()
        {
            nextCallBack();
            return ++_cursor < length;
        } 

        public void Reset() 
            => _cursor = -1;

        public void Dispose()
            => particles = default;

        public int Length => length;
        public ref T Current => ref particles[(_cursor + from) % capacity];

    }

}

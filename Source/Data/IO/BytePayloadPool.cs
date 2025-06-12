///////////////////////////////////////////////////////
/// Filename: BytePayloadPool.cs
/// Date: June 8, 2025
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using System.Buffers;
using System.Collections.Concurrent;
using EppNet.Node;

public class BytePayloadPool : INodeDescendant
{
    public NetworkNode Node { get; }

    public ArrayPool<byte> ArrayPool { get; }

    public int DefaultCapacity { get; }

    private readonly ConcurrentBag<BytePayload> _pool;

    public IResizeStrategy ResizeStrategy { get; }

    internal BytePayloadPool(NetworkNode node, ArrayPool<byte> pool = null, int defaultCapacity = 1024)
    {
        this.Node = node;
        this.ArrayPool = (pool is null) ? ArrayPool<byte>.Shared : pool;
        this.DefaultCapacity = defaultCapacity;
        this.ResizeStrategy = new ExponentialResizeStrategy();
        this._pool = new();
    }

    public BytePayload Rent(int capacity)
    {
        if (!_pool.TryTake(out BytePayload payload) || payload.IsDestroyed)
            payload = new BytePayload(this, capacity);

        return payload;
    }

    internal bool Return(BytePayload payload)
    {
        if (payload.IsDestroyed)
            return false;

        _pool.Add(payload);
        return true;
    }

}
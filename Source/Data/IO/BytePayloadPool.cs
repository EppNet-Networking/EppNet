///////////////////////////////////////////////////////
/// Filename: BytePayloadPool.cs
/// Date: June 8, 2025
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Node;

using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

[DebuggerDisplay("Rents = {Rents}, Returns = {Returns}, CurrentlyRented = {CurrentlyRented}, PoolSize = {PoolSize}, Prunes = {Prunes}, DefaultCapacity = {DefaultCapacity}")]
public class BytePayloadPool : INodeDescendant
{
    public NetworkNode Node { get; }

    public ArrayPool<byte> ArrayPool { get; }

    public int DefaultCapacity { get; }

    public int Rents => _rents;
    public int Returns => _returns;
    public int Prunes => _prunes;

    public int PoolSize { get => _pool.Count; }
    public int CurrentlyRented { get => _rented.Count; }

    public IResizeStrategy ResizeStrategy { get; }

    protected int _rents, _returns, _prunes;

    private readonly ConcurrentBag<BytePayload> _pool;

    private readonly ConcurrentDictionary<int, WeakReference<BytePayload>> _rented;

    /// <summary>
    /// This is the next available ID for tracking <see cref="BytePayload"/>s available for rent.<br/>
    /// Payloads are assigned a unique ID at instantiation time.<br/>
    /// The default capacity is the default capacity in bytes and is the exact number of bytes for each new payload if a capacity isn't specified
    /// </summary>
    internal int _nextId;

    internal BytePayloadPool(NetworkNode node, ArrayPool<byte> pool = null, int defaultCapacity = 1024)
    {
        this.Node = node ?? throw new ArgumentNullException(nameof(node));
        this.ArrayPool = (pool is null) ? ArrayPool<byte>.Shared : pool;
        this.DefaultCapacity = defaultCapacity;
        this.ResizeStrategy = new ExponentialResizeStrategy();
        this._pool = new();
        this._rented = new();
        this._nextId = -1;
    }

    /// <summary>
    /// Rents a <see cref="BytePayload"/> from the pool.<br/>
    /// If no payloads are available, this will create a new one and allocate the specified amount of memory (in bytes).<br/>
    /// Passing -1 as the capacity implies a capacity of <see cref="DefaultCapacity"/>
    /// </summary>
    /// <param name="capacity"></param>
    /// <returns></returns>

    public BytePayload Rent(int capacity = -1)
    {
        capacity = capacity < 1 ? DefaultCapacity : capacity;

        if (!_pool.TryTake(out BytePayload payload) || payload.IsDestroyed)
        {
            int id = Interlocked.Increment(ref _nextId);
            payload = new BytePayload(this, id, capacity);
        }
        else
            payload.EnsureCapacity(capacity, true);

        _rented.TryAdd(payload._poolId, new WeakReference<BytePayload>(payload));
        Interlocked.Increment(ref _rents);
        return payload;
    }

    /// <summary>
    /// Prunes garbage collected payloads from the internal rented tracking dictionary
    /// </summary>
    /// <returns></returns>

    public int Prune()
    {
        int pruned = 0;

        foreach (KeyValuePair<int, WeakReference<BytePayload>> kvp in _rented)
        {
            if (!kvp.Value.TryGetTarget(out _) && _rented.TryRemove(kvp.Key, out _))
            {
                Interlocked.Increment(ref _prunes);
                pruned++;
            }
        }

        return pruned;
    }

    /// <summary>
    /// Returns a <see cref="BytePayload"/> to the pool.<br/>
    /// If the payload was destroyed, this ensures it isn't added back to the pool.
    /// </summary>
    /// <param name="payload"></param>
    /// <returns></returns>

    public bool Return(BytePayload payload)
    {
        if (payload.IsDestroyed)
        {
            // Let's ensure the payload isn't in the rented dictionary
            _rented.TryRemove(payload._poolId, out _);
            return false;
        }

        _pool.Add(payload);
        _rented.TryRemove(payload._poolId, out _);
        Interlocked.Increment(ref _returns);
        return true;
    }

    /// <summary>
    /// Clears out the pool and destroys all payloads this instance is responsible for. That includes:<br/>
    /// - Rented payloads<br/>
    /// - Pooled payloads
    /// </summary>
    /// <returns>The number of <see cref="BytePayload"/>s destroyed</returns>

    public int Clear()
    {
        int destroyed = 0;

        while (_pool.TryTake(out BytePayload payload))
        {
            payload.Destroy();
            destroyed++;
        }

        foreach (KeyValuePair<int, WeakReference<BytePayload>> kvp in _rented)
        {
            if (kvp.Value.TryGetTarget(out BytePayload payload))
            {
                // TODO: BytePayloadPool: Log that we cleared a payload that might've been in use
                payload.Destroy();
                destroyed++;
            }
        }

        _rented.Clear();
        _nextId = -1;
        return destroyed;
    }

}

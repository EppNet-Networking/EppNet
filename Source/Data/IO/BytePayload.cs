///////////////////////////////////////////////////////
/// Filename: BytePayload.cs
/// Date: June 8, 2025
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using System;

public sealed class BytePayload : IDisposable
{

    public int Capacity { internal set; get; }

    public int Length { internal set; get; }

    public DateTime LastUse { internal set; get; }

    public bool IsDestroyed { internal set; get; }

    public byte[] Buffer { internal set; get; }

    public bool IsPooled { internal set; get; }

    public IResizeStrategy ResizeStrategy;
    private readonly BytePayloadPool _pool;

    internal BytePayload(BytePayloadPool pool, int capacity)
    {
        this._pool = pool ?? throw new ArgumentNullException(nameof(pool));

        this.IsDestroyed = IsPooled = false;
        this.Capacity = capacity;
        this.Length = 0;
        this.LastUse = DateTime.Now;
        this.Buffer = new byte[capacity];
        this.ResizeStrategy = pool.ResizeStrategy;
    }

    internal BytePayload(IResizeStrategy resizeStrategy, int capacity)
    {
        this._pool = null;

        this.IsDestroyed = IsPooled = false;
        this.Capacity = capacity;
        this.Length = 0;
        this.LastUse = DateTime.Now;
        this.Buffer = new byte[capacity];
        this.ResizeStrategy = resizeStrategy ?? throw new ArgumentNullException(nameof(resizeStrategy));
    }

    public Span<byte> AsSpan() =>
        new(Buffer, 0, Length);

    public ReadOnlySpan<byte> AsReadOnlySpan() =>
        new(Buffer, 0, Length);

    public void EnsureCapacity(int neededBytes, bool exact = false)
    {
        LastUse = DateTime.Now;

        if (neededBytes < 0)
            throw new ArgumentOutOfRangeException(nameof(neededBytes));

        if (Length + neededBytes <= Capacity)
            return;

        int additional = neededBytes - (Capacity - Length);
        int newSize = exact ? Capacity + additional : ResizeStrategy.Resize(Capacity + additional);
        Resize(newSize);
    }

    public void Resize(int newSize)
    {
        if (IsDestroyed || newSize <= Capacity)
            return;

        byte[] newBuffer = new byte[newSize];
        System.Buffer.BlockCopy(Buffer, 0, newBuffer, 0, Length);

        this.Buffer = newBuffer;
        this.Capacity = newSize;
    }

    public void Reset()
    {
        if (Buffer is not null)
            Array.Clear(Buffer, 0, Length);

        Length = 0;
        LastUse = DateTime.Now;
    }

    /// <summary>
    /// Resets this payload and returns it to the pool (if-applicable)
    /// </summary>

    public void Dispose()
    {
        if (IsDestroyed || Buffer is null)
            return;

        Reset();

        if (_pool is not null)
        {
            _pool.Return(this);
            this.IsPooled = true;
        }
    }
    
    /// <summary>
    /// Destroys this payload and frees it memory. It is no longer capable of being pooled.
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>

    public void Destroy()
    {
        if (!IsDestroyed && Buffer is not null)
        {
            this.Buffer = Array.Empty<byte>();
            this.Length = this.Capacity = 0;
            this.IsDestroyed = true;
            this.IsPooled = false;
        }
    }

}

public ref struct BytePayloadWriter
{

    public int Position
    {
        internal set;
        get;
    }

    private readonly BytePayload _owner;

    public BytePayloadWriter(BytePayload owner)
    {
        if (owner.IsDestroyed)
            throw new ObjectDisposedException("BytePayload has been disposed!");

        this._owner = owner;
        this.Position = 0;
    }

    public BytePayloadWriter(BytePayload owner, int offset)
    {
        if (owner.IsDestroyed)
            throw new ObjectDisposedException("BytePayload has been disposed!");

        if (offset < 0 || offset >= owner.Length)
            throw new ArgumentOutOfRangeException("Offset is out of bounds!");

        this._owner = owner;
        this.Position = offset;
    }

    public void WriteByte(byte value)
    {
        #if DEBUG
        if (_owner.IsDestroyed)
            throw new ObjectDisposedException("BytePayload has been disposed!");
        #endif
        
        _owner.EnsureCapacity(1);
        _owner.Buffer[Position++] = value;
        _owner.Length++;
    }

    public void WriteBytes(ReadOnlySpan<byte> data)
    {
        #if DEBUG
        if (_owner.IsDestroyed)
            throw new ObjectDisposedException("BytePayload has been disposed!");
        #endif

        _owner.EnsureCapacity(data.Length);
        data.CopyTo(new Span<byte>(_owner.Buffer, Position, data.Length));
        Position += data.Length;

        // Update buffer length and last use
        _owner.Length = Math.Max(Position, _owner.Length);
        _owner.LastUse = DateTime.Now;
    }

    public Span<byte> Reserve(int count)
    {
        #if DEBUG
        if (_owner.IsDestroyed)
            throw new ObjectDisposedException("BytePayload has been disposed!");
        #endif
        
        _owner.EnsureCapacity(count);
        Span<byte> slice = new(_owner.Buffer, Position, count);
        Position += count;

        // Update buffer length and last use
        _owner.Length = Math.Max(Position, _owner.Length);
        _owner.LastUse = DateTime.Now;
        return slice;
    }

}

public ref struct BytePayloadReader
{

    public int Position { internal set; get; }

    private readonly BytePayload _owner;

    public BytePayloadReader(BytePayload owner)
    {
        if (owner.IsDestroyed)
            throw new ObjectDisposedException(nameof(owner));

        this._owner = owner;
        this.Position = 0;
    }

    public BytePayloadReader(BytePayload owner, int offset)
    {
        if (owner.IsDestroyed)
            throw new ObjectDisposedException(nameof(owner));

        if (offset < 0 || offset >= owner.Length)
            throw new ArgumentOutOfRangeException("Offset is out of bounds!");

        this._owner = owner;
        this.Position = offset;
    }

    public byte ReadByte()
    {
        #if DEBUG
        if (_owner.IsDestroyed)
            throw new ObjectDisposedException("BytePayload has been disposed!");

        if (Position >= _owner.Length)
            throw new IndexOutOfRangeException("Attempted to read beyond buffer length!");
        #endif
        return _owner.Buffer[Position++];
    }

    public ReadOnlySpan<byte> ReadBytes(int count)
    {
        #if DEBUG
        if (_owner.IsDestroyed)
            throw new ObjectDisposedException("BytePayload has been disposed!");

        if (Position + count > _owner.Length)
            throw new IndexOutOfRangeException("Attempted to read beyond buffer length!");
        #endif
        ReadOnlySpan<byte> span = new(_owner.Buffer, Position, count);
        Position += count;
        return span;
    }

}

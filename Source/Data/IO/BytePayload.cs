///////////////////////////////////////////////////////
/// Filename: BytePayload.cs
/// Date: June 8, 2025
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using System;
using System.Runtime.CompilerServices;

[DebuggerDisplay("Capacity = {Capacity}, Length = {Length}, IsPooled = {IsPooled}")]
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

    public Span<byte> AsSpan()
    {
        Touch();
        return new(Buffer, 0, Length);
    }

    public ReadOnlySpan<byte> AsReadOnlySpan()
    {
        Touch();
        return new(Buffer, 0, Length);
    }

    public Memory<byte> AsMemory()
    {
        Touch();
        return new(Buffer, 0, Length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

    /// <summary>
    /// Updates tracking
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void Touch()
    {
        this.LastUse = DateTime.Now;
    }

}

[DebuggerDisplay("Position = {Position}, BytesWritten = {BytesWritten}")]
public ref struct BytePayloadWriter
{

    public int Position { internal set; get; }

    public int BytesWritten { internal set; get; }

    private readonly BytePayload _owner;

    public BytePayloadWriter(BytePayload owner)
    {
        if (owner.IsDestroyed)
            throw new ObjectDisposedException("BytePayload has been disposed!");

        this._owner = owner;
        this.Position = 0;
        this.BytesWritten = 0;
    }

    public BytePayloadWriter(BytePayload owner, int offset)
    {
        if (owner.IsDestroyed)
            throw new ObjectDisposedException("BytePayload has been disposed!");

        if (offset < 0 || offset >= owner.Length)
            throw new ArgumentOutOfRangeException("Offset is out of bounds!");

        this._owner = owner;
        this.Position = offset;
        this.BytesWritten = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteByte(byte value)
    {
#if DEBUG
        if (_owner.IsDestroyed)
            throw new ObjectDisposedException("BytePayload has been disposed!");
#endif

        _owner.EnsureCapacity(1);
        _owner.Buffer[Position++] = value;
        _owner.Touch();

        _owner.Length++;

        // Update bytes written
        BytesWritten++;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        _owner.Length = Position;
        _owner.Touch();

        BytesWritten += data.Length;
    }

    /// <summary>
    /// Reserves the specified number of bytes for writing<br/>
    /// This advances the writer and updates the number of bytes written immediately
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<byte> Reserve(int bytes, bool clear = true)
    {
#if DEBUG
        if (_owner.IsDestroyed)
            throw new ObjectDisposedException("BytePayload has been disposed!");
#endif

        _owner.EnsureCapacity(bytes);
        Span<byte> slice = new(_owner.Buffer, Position, bytes);

        if (clear)
            slice.Clear();

        Position += bytes;
        BytesWritten += bytes;

        // Update buffer length and last use
        _owner.Length = Position;
        _owner.Touch();
        return slice;
    }

    /// <summary>
    /// Advances the current position of the buffer by the specified number of bytes
    /// </summary>
    /// <param name="bytes"></param>

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Advance(int bytes) =>
        Position = Math.Min(Position + bytes, _owner.Length);


    /// <summary>
    /// Resets the position of the writer to the beginning of the buffer<br/>
    /// Resets <see cref="BytesWritten"/> to 0<br/><br/>
    /// <strong>NOTE!</strong> Does not undo bytes written to the buffer!
    /// </summary>

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Reset() =>
        this.Position = BytesWritten = 0;

}

[DebuggerDisplay("Position = {Position}, Remaining = {Remaining}, BytesRead = {BytesRead}")]
public ref struct BytePayloadReader
{

    public int Position { internal set; get; }

    public readonly int Remaining => _owner.Length - Position;
    public readonly bool HasRemaining => Remaining > 0;

    public int BytesRead { internal set; get; }

    private readonly BytePayload _owner;

    public BytePayloadReader(BytePayload owner)
    {
        if (owner.IsDestroyed)
            throw new ObjectDisposedException(nameof(owner));

        this._owner = owner;
        this.Position = 0;
        this.BytesRead = 0;
    }

    public BytePayloadReader(BytePayload owner, int offset)
    {
        if (owner.IsDestroyed)
            throw new ObjectDisposedException(nameof(owner));

        if (offset < 0 || offset >= owner.Length)
            throw new ArgumentOutOfRangeException("Offset is out of bounds!");

        this._owner = owner;
        this.Position = offset;
        this.BytesRead = 0;
    }

    /// <summary>
    /// Tries to peek the byte at the current position<br/>
    /// If successful, outputs the byte
    /// </summary>
    /// <param name="value"></param>
    /// <returns>Whether or not a byte is available to peek</returns>

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryPeekByte(out byte value)
    {
        if (_owner.IsDestroyed || !HasRemaining)
        {
            value = default;
            return false;
        }

        value = _owner.Buffer[Position];
        _owner.Touch();
        return true;
    }

    /// <summary>
    /// Tries to peek the byte at the current position<br/>
    /// If successful, outputs the byte
    /// </summary>
    /// <param name="value"></param>
    /// <returns>Whether or not a byte is available to peek</returns>

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryPeek(out byte value) =>
        TryPeekByte(out value);

    /// <summary>
    /// Tries to peek the specified amount of bytes<br/>
    /// If successful, outputs a ReadOnlySpan<byte> containing the bytes
    /// </summary>
    /// <param name="count"></param>
    /// <param name="data"></param>
    /// <returns>Whether or not the specified number of bytes could be peeked</returns>

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryPeekBytes(int count, out ReadOnlySpan<byte> data)
    {
        if (_owner.IsDestroyed || Position + count > _owner.Length)
        {
            data = default;
            return false;
        }

        _owner.Touch();
        data = new ReadOnlySpan<byte>(_owner.Buffer, Position, count);
        return true;
    }

    /// <summary>
    /// Tries to peek the specified amount of bytes<br/>
    /// If successful, outputs a ReadOnlySpan<byte> containing the bytes
    /// </summary>
    /// <param name="count"></param>
    /// <param name="data"></param>
    /// <returns>Whether or not the specified number of bytes could be peeked</returns>

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryPeek(int count, out ReadOnlySpan<byte> data) =>
        TryPeekByte(count, out data);

    /// <summary>
    /// Peeks the byte at the current position
    /// </summary>
    /// <returns>The byte peeked</returns>

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte PeekByte()
    {
#if DEBUG
        if (_owner.IsDestroyed)
            throw new ObjectDisposedException("BytePayload has been disposed!");
        if (!HasRemaining)
            throw new IndexOutOfRangeException("Attempted to peek beyond buffer length!");
#endif

        _owner.Touch();
        return _owner.Buffer[Position];
    }

    /// <summary>
    /// Peeks the byte at the current position
    /// </summary>
    /// <returns>The byte peeked</returns>

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte Peek() =>
        PeekByte();

    /// <summary>
    /// Peeks the specified number of bytes at the current position
    /// </summary>
    /// <returns>The bytes peeked</returns>

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<byte> PeekBytes(int count)
    {
#if DEBUG
        if (_owner.IsDestroyed)
            throw new ObjectDisposedException("BytePayload has been disposed!");
        if (Position + count > _owner.Length)
            throw new IndexOutOfRangeException("Attempted to peek beyond buffer length!");
#endif
        _owner.Touch();
        return new(_owner.Buffer, Position, count);
    }

    /// <summary>
    /// Peeks the specified number of bytes at the current position
    /// </summary>
    /// <returns>The bytes peeked</returns>

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<byte> Peek(int count) =>
        PeekBytes(count);

    /// <summary>
    /// Tries to read the byte at the current position<br/>
    /// If successful, outputs the byte
    /// </summary>
    /// <param name="value"></param>
    /// <returns>Whether or not a byte is available to read</returns>

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryReadByte(out byte value)
    {
        if (_owner.IsDestroyed || !HasRemaining)
        {
            value = default;
            return false;
        }

        value = _owner.Buffer[Position++];
        _owner.Touch();
        BytesRead++;
        return true;
    }

    /// <summary>
    /// Tries to read the byte at the current position<br/>
    /// If successful, outputs the byte
    /// </summary>
    /// <param name="value"></param>
    /// <returns>Whether or not a byte is available to read</returns>

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryRead(out byte value) =>
        TryReadByte(out value);

    /// <summary>
    /// Reads the byte at the current position<br/>
    /// </summary>
    /// <returns>The byte read</returns>

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte ReadByte()
    {
#if DEBUG
        if (_owner.IsDestroyed)
            throw new ObjectDisposedException("BytePayload has been disposed!");
        if (Position >= _owner.Length)
            throw new IndexOutOfRangeException("Attempted to read beyond buffer length!");
#endif

        byte value = _owner.Buffer[Position++];
        _owner.Touch();

        BytesRead++;

        return value;
    }

    /// <summary>
    /// Reads the byte at the current position<br/>
    /// </summary>
    /// <returns>The byte read</returns>

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte Read() =>
        ReadBytes();

    /// <summary>
    /// Reads the specified number of bytes at the current position<br/>
    /// </summary>
    /// <returns>The bytes read</returns>

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        BytesRead += count;

        _owner.Touch();
        return span;
    }

    /// <summary>
    /// Reads the specified number of bytes at the current position<br/>
    /// </summary>
    /// <returns>The bytes read</returns>

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<byte> Read(int count) =>
        ReadBytes(count);

    /// <summary>
    /// Advances the current position of the buffer by the specified number of bytes
    /// </summary>
    /// <param name="bytes"></param>

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Advance(int bytes) =>
        Position = Math.Min(Position + bytes, _owner.Length);

    /// <summary>
    /// Resets the position of the reader to the beginning of the buffer<br/>
    /// Sets <see cref="BytesRead"/> to 0
    /// </summary>

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Reset() =>
        this.Position = BytesRead = 0;

}

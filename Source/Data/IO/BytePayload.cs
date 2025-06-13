///////////////////////////////////////////////////////
/// Filename: BytePayload.cs
/// Date: June 8, 2025
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Utilities;

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

[DebuggerDisplay("Capacity = {Capacity}, Length = {Length}, IsPooled = {IsPooled}")]
public sealed class BytePayload : IDisposable
{

    public const int HeaderOffset = 1;

    public int Capacity { internal set; get; }

    public int Length { internal set; get; }

    public DateTime LastUse { internal set; get; }

    public bool IsDestroyed { internal set; get; }

    public byte[] Buffer { internal set; get; }

    public bool IsPooled { internal set; get; }

    public IResizeStrategy ResizeStrategy;

    /// <summary>
    /// The decimal precision of floating point numbers.
    /// </summary>
    public static int FloatPrecision = 4;

    public static double PrecisionDecimalPlaces
    {
        get
        {
            if (_precisionDecimalPlaces == 0)
                _precisionDecimalPlaces = FastMath.GetTenPow(FloatPrecision);

            return _precisionDecimalPlaces;
        }
    }

    private static double _precisionDecimalPlaces = 0d;

    public static double PrecisionReturnDecimalPlaces
    {
        get
        {
            if (_precisionReturnDecimalPlaces == 0)
                _precisionReturnDecimalPlaces = 1 / PrecisionDecimalPlaces;

            return _precisionReturnDecimalPlaces;
        }
    }

    private static double _precisionReturnDecimalPlaces = 0d;
    public byte Header
    {
        set
        {
            if (Buffer.Length > 0)
                Buffer[0] = value;
        }
        get => Buffer.Length > 0 ? Buffer[0] : (byte) 0;
    }

    private readonly BytePayloadPool _pool;
    internal readonly int _poolId;

    /// <summary>
    /// Creates a payload that makes use of the specified <see cref="BytePayloadPool"/>.<br/>
    /// The capacity parameter ensures that this buffer has the exact number of bytes available.
    /// </summary>
    /// <param name="pool"></param>
    /// <param name="capacity"></param>
    /// <exception cref="ArgumentNullException"></exception>
    internal BytePayload(BytePayloadPool pool, int poolId, int capacity)
    {
        this._pool = pool ?? throw new ArgumentNullException(nameof(pool));
        this._poolId = poolId;

        this.IsDestroyed = IsPooled = false;
        this.Capacity = capacity;
        this.Length = 0;
        this.LastUse = DateTime.Now;
        this.Buffer = pool.ArrayPool.Rent(capacity);
        this.ResizeStrategy = pool.ResizeStrategy;
    }

    /// <summary>
    /// Creates an independent payload that does not use any kind of pooling.
    /// </summary>
    /// <param name="resizeStrategy"></param>
    /// <param name="capacity"></param>
    /// <exception cref="ArgumentNullException"></exception>

    internal BytePayload(IResizeStrategy resizeStrategy, int capacity)
    {
        this._pool = null;
        this._poolId = -1;

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

        byte[] newBuffer = _pool?.ArrayPool.Rent(newSize) ?? new byte[newSize];
        System.Buffer.BlockCopy(Buffer, 0, newBuffer, 0, Length);
        _pool?.ArrayPool.Return(Buffer, true);

        this.Buffer = newBuffer;
        this.Capacity = newSize;
    }

    /// <summary>
    /// Resets this payload's length
    /// </summary>
    /// <param name="clear"></param>

    public void Reset(bool clear = true)
    {
        if (Buffer is not null && clear)
        {
            Header = 0;
            Array.Clear(Buffer, 0, Length);
        }

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

        bool willPool = _pool is not null && !IsPooled;
        Reset(clear: !willPool);

        if (willPool)
        {
            _pool.ArrayPool.Return(Buffer, true);
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
            _pool?.ArrayPool.Return(Buffer, true);
            this.Buffer = Array.Empty<byte>();
            this.Length = this.Capacity = 0;
            this.IsDestroyed = true;
            this.IsPooled = false;
            this.LastUse = DateTime.MinValue;
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

    /// <summary>
    /// Creates a new writer for the specified <see cref="BytePayload"/>.
    /// </summary>
    /// <param name="owner">The payload to write to</param>
    /// <param name="reserveHeader">Whether or not to start after the <see cref="BytePayload.HeaderOffset"/></param>
    /// <exception cref="ArgumentNullException">Tried to create a writer for a null payload</exception>
    /// <exception cref="ObjectDisposedException">Tried to create a writer for a destroyed payload</exception>

    public BytePayloadWriter(BytePayload owner, bool reserveHeader = true)
    {
        if (owner is null)
            throw new ArgumentNullException(nameof(owner));

        if (owner.IsDestroyed)
            throw new ObjectDisposedException("BytePayload has been disposed!");

        this._owner = owner;
        this.Position = reserveHeader ? BytePayload.HeaderOffset : 0;
        this.BytesWritten = 0;
    }

    /// <summary>
    /// Creates a new writer for the specified <see cref="BytePayload"/>.
    /// </summary>
    /// <param name="owner">The payload to write to</param>
    /// <param name="offset">The offset in bytes to begin writing from</param>
    /// <param name="reserveHeader">If enabled, offsets the beginning by the <see cref="BytePayload.HeaderOffset"/> in bytes</param>
    /// <exception cref="ArgumentNullException">Tried to create a writer for a null payload</exception>
    /// <exception cref="ObjectDisposedException">Tried to create a writer for a destroyed payload</exception>
    /// <exception cref="ArgumentOutOfRangeException">Tried to set the offset to a number out of range of the buffer</exception>

    public BytePayloadWriter(BytePayload owner, int offset, bool reserveHeader = true)
    {
        if (owner is null)
            throw new ArgumentNullException(nameof(owner));

        if (owner.IsDestroyed)
            throw new ObjectDisposedException("BytePayload has been disposed!");

        if (offset < 0 || offset >= owner.Length)
            throw new ArgumentOutOfRangeException("Offset is out of bounds!");

        this._owner = owner;
        this.Position = reserveHeader ? BytePayload.HeaderOffset + offset : offset;
        this.BytesWritten = 0;
    }

    /// <summary>
    /// Tries to write the specified byte to the buffer
    /// </summary>
    /// <param name="value"></param>

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryWriteByte(byte value)
    {
        if (_owner.IsDestroyed)
            return false;

        _owner.EnsureCapacity(1);
        _owner.Buffer[Position++] = value;
        _owner.Touch();

        _owner.Length = Math.Max(_owner.Length, Position);

        // Update bytes written
        BytesWritten++;
        return true;
    }

    /// <summary>
    /// Tries to write the specified byte to the buffer
    /// </summary>
    /// <param name="value"></param>

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryWrite(byte value) =>
        TryWriteByte(value);

    /// <summary>
    /// Tries to write the specified span to the buffer
    /// </summary>
    /// <param name="value"></param>

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryWriteBytes(Span<byte> data)
    {
        if (_owner.IsDestroyed)
            return false;

        _owner.EnsureCapacity(data.Length);
        data.CopyTo(new Span<byte>(_owner.Buffer, Position, data.Length));
        Position += data.Length;

        // Update buffer length and last use
        _owner.Length = Math.Max(_owner.Length, Position);
        _owner.Touch();

        BytesWritten += data.Length;
        return true;
    }

    /// <summary>
    /// Tries to write the specified span to the buffer
    /// </summary>
    /// <param name="value"></param>

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryWrite(Span<byte> data) =>
        TryWriteBytes(data);

    /// <summary>
    /// Writes the specified byte to the buffer
    /// </summary>
    /// <param name="value"></param>

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

        _owner.Length = Math.Max(_owner.Length, Position);

        // Update bytes written
        BytesWritten++;
    }

    /// <summary>
    /// Writes the specified byte to the buffer
    /// </summary>
    /// <param name="value"></param>

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(byte value) =>
        WriteByte(value);

    /// <summary>
    /// Writes the specified span to the buffer
    /// </summary>
    /// <param name="value"></param>

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteBytes(Span<byte> data)
    {
#if DEBUG
        if (_owner.IsDestroyed)
            throw new ObjectDisposedException("BytePayload has been disposed!");
#endif

        _owner.EnsureCapacity(data.Length);
        data.CopyTo(new Span<byte>(_owner.Buffer, Position, data.Length));
        Position += data.Length;

        // Update buffer length and last use
        _owner.Length = Math.Max(_owner.Length, Position);
        _owner.Touch();

        BytesWritten += data.Length;
    }

    /// <summary>
    /// Writes the specified span to the buffer
    /// </summary>
    /// <param name="value"></param>

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(Span<byte> data) =>
        WriteBytes(data);

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
        _owner.Length = Math.Max(_owner.Length, Position);
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

    /// <summary>
    /// Creates a new reader for the specified <see cref="BytePayload"/>.
    /// </summary>
    /// <param name="owner">The payload to read from</param>
    /// <param name="reserveHeader">Whether or not to start after the <see cref="BytePayload.HeaderOffset"/></param>
    /// <exception cref="ArgumentNullException">Tried to create a reader for a null payload</exception>
    /// <exception cref="ObjectDisposedException">Tried to create a reader for a destroyed payload</exception>

    public BytePayloadReader(BytePayload owner, bool skipHeader = true)
    {
        if (owner is null)
            throw new ArgumentNullException(nameof(owner));

        if (owner.IsDestroyed)
            throw new ObjectDisposedException(nameof(owner));

        this._owner = owner;
        this.Position = skipHeader ? BytePayload.HeaderOffset : 0;
        this.BytesRead = 0;
    }

    /// <summary>
    /// Creates a new reader for the specified <see cref="BytePayload"/>.
    /// </summary>
    /// <param name="owner">The payload to read from</param>
    /// <param name="offset">The offset in bytes to begin reading from</param>
    /// <param name="reserveHeader">If enabled, offsets the beginning by the <see cref="BytePayload.HeaderOffset"/> in bytes</param>
    /// <exception cref="ArgumentNullException">Tried to create a reader for a null payload</exception>
    /// <exception cref="ObjectDisposedException">Tried to create a reader for a destroyed payload</exception>
    /// <exception cref="ArgumentOutOfRangeException">Tried to set the offset to a number out of range of the buffer</exception>

    public BytePayloadReader(BytePayload owner, int offset, bool skipHeader = true)
    {
        if (owner is null)
            throw new ArgumentNullException(nameof(owner));

        if (owner.IsDestroyed)
            throw new ObjectDisposedException(nameof(owner));

        if (offset < 0 || offset >= owner.Length)
            throw new ArgumentOutOfRangeException("Offset is out of bounds!");

        this._owner = owner;
        this.Position = skipHeader ? BytePayload.HeaderOffset + offset : offset;
        this.BytesRead = 0;
    }

    /// <summary>
    /// Tries to peek the byte at the current position<br/>
    /// If successful, outputs the byte
    /// </summary>
    /// <param name="value"></param>
    /// <returns>Whether or not a byte is available to peek</returns>

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool TryPeekByte(out byte value)
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
    public readonly bool TryPeek(out byte value) =>
        TryPeekByte(out value);

    /// <summary>
    /// Tries to peek the specified amount of bytes<br/>
    /// If successful, outputs a ReadOnlySpan<byte> containing the bytes
    /// </summary>
    /// <param name="count"></param>
    /// <param name="data"></param>
    /// <returns>Whether or not the specified number of bytes could be peeked</returns>

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool TryPeekBytes(int count, out ReadOnlySpan<byte> data)
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
    public readonly bool TryPeek(int count, out ReadOnlySpan<byte> data) =>
        TryPeekBytes(count, out data);

    /// <summary>
    /// Peeks the byte at the current position
    /// </summary>
    /// <returns>The byte peeked</returns>

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly byte PeekByte()
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
    public readonly byte Peek() =>
        PeekByte();

    /// <summary>
    /// Peeks the specified number of bytes at the current position
    /// </summary>
    /// <returns>The bytes peeked</returns>

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly ReadOnlySpan<byte> PeekBytes(int count)
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
    public readonly ReadOnlySpan<byte> Peek(int count) =>
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
    /// Tries to read the specified number of bytes at the current position<br/>
    /// </summary>
    /// <returns>The bytes read</returns>

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryReadBytes(int count, out ReadOnlySpan<byte> data)
    {
        if (_owner.IsDestroyed || Position + count > _owner.Length)
        {
            data = default;
            return false;
        }

        data = new(_owner.Buffer, Position, count);
        Position += count;
        BytesRead += count;

        _owner.Touch();
        return true;
    }

    /// <summary>
    /// Tries to read the specified number of bytes at the current position<br/>
    /// </summary>
    /// <returns>The bytes read</returns>

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryRead(int count, out ReadOnlySpan<byte> data) =>
        TryReadBytes(count, out data);

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
        ReadByte();

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

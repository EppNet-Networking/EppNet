///////////////////////////////////////////////////////
/// Filename: BytePayloadReader.cs
/// Date: June 8, 2025
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Data;
using Microsoft.CodeAnalysis;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace EppNet.IO
{
    [DebuggerDisplay("Position = {Position}, Remaining = {Remaining}, BytesRead = {BytesRead}")]
    public ref struct BytePayloadReader
    {

        public int Position { internal set; get; }

        public readonly int Remaining => _owner.Length - Position;
        public readonly bool HasRemaining => Remaining > 0;

        public int BytesRead { internal set; get; }

        public Encoding Encoder { set; get; }

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
            this.Encoder = Encoding.UTF8;
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
            this.Encoder = Encoding.UTF8;
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
        /// Tries to read the specified dictionary from the buffer<br/>
        /// Ensure you have resolvers for both types!
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dict"></param>
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryReadDictionary<TKey, TValue>(out Dictionary<TKey, TValue> output)
        {
            byte header = ReadByte();
            output = null;

            if (header == IResolver.EmptyArrayHeader)
            {
                output = new();
                return true;
            }
            else if (header == IResolver.NullArrayHeader)
            {
                output = null;
                return true;
            }

            if (!_owner.Node.DataStorage.TryGetResolver<TKey>(out IResolver keyResolver) ||
                !_owner.Node.DataStorage.TryGetResolver<TValue>(out IResolver valueResolver))
                return false;

            IResolver._Internal_ReadHeaderAndGetLength(ref this, header, out int count);

            output = new();
            Resolver<TKey> kResolver = keyResolver as Resolver<TKey>;
            Resolver<TValue> vResolver = valueResolver as Resolver<TValue>;

            for (int i = 0; i < count; i++)
            {
                if (kResolver.Read(ref this, out TKey key).IsSuccess() &&
                    vResolver.Read(ref this, out TValue value).IsSuccess())
                    output.Add(key, value);
                else
                    return false;
            }

            return true;
        }

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
        /// Reads the specified dictionary from the buffer<br/>
        /// Ensure you have resolvers for both types!
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dict"></param>

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Dictionary<TKey, TValue> ReadDictionary<TKey, TValue>()
        {
            byte header = ReadByte();

            if (header == IResolver.EmptyArrayHeader)
                return new();
            else if (header == IResolver.NullArrayHeader)
                return null;

            if (!_owner.Node.DataStorage.TryGetResolver<TKey>(out IResolver keyResolver) ||
                !_owner.Node.DataStorage.TryGetResolver<TValue>(out IResolver valueResolver))
            {
                Debug.WriteLine($"ReadDictionary: Ensure resolvers exist for {typeof(TKey).FullName} and {typeof(TValue).FullName}!");
                return null;
            }

            IResolver._Internal_ReadHeaderAndGetLength(ref this, header, out int count);

            Dictionary<TKey, TValue> output = new();
            Resolver<TKey> kResolver = keyResolver as Resolver<TKey>;
            Resolver<TValue> vResolver = valueResolver as Resolver<TValue>;

            for (int i = 0; i < count; i++)
            {
                if (kResolver.Read(ref this, out TKey key).IsSuccess() &&
                    vResolver.Read(ref this, out TValue value).IsSuccess())
                    output.Add(key, value);
                else
                {
                    Debug.WriteLine($"ReadDictionary: Failed to read key or value for Dictionary<{typeof(TKey).FullName}, {typeof(TValue).FullName}>!");
                    break;
                }
            }

            return output;
        }

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
}

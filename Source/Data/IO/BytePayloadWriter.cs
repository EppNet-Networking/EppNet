///////////////////////////////////////////////////////
/// Filename: BytePayloadWriter.cs
/// Date: June 8, 2025
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Data;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace EppNet.IO
{
    [DebuggerDisplay("Position = {Position}, BytesWritten = {BytesWritten}")]
    public ref struct BytePayloadWriter
    {

        public int Position { internal set; get; }

        public int BytesWritten { internal set; get; }

        public Encoding Encoder { set; get; }

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
            this.Encoder = Encoding.UTF8;
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
            this.Encoder = Encoding.UTF8;
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
        /// Tries to write the specified dictionary to the buffer<br/>
        /// Ensure you have resolvers for both types!
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dict"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryWriteDictionary<TKey, TValue>(in Dictionary<TKey, TValue> dict)
        {
            if (dict is null || dict.Count == 0)
            {
                byte header = dict is null
                    ? IResolver.NullArrayHeader
                    : IResolver.EmptyArrayHeader;

                return TryWriteByte(header);
            }

            if (!_owner.Node.DataStorage.TryGetResolver<TKey>(out IResolver keyResolver) ||
                !_owner.Node.DataStorage.TryGetResolver<TValue>(out IResolver valueResolver))
                return false;

            IResolver._Internal_WriteHeaderAndLength(ref this, dict.Count);

            foreach (KeyValuePair<TKey, TValue> kvp in dict)
            {
                // Write key value pairs next to each other
                if (!keyResolver.Write(ref this, kvp.Key) ||
                    !valueResolver.Write(ref this, kvp.Value))
                    return false;
            }

            return true;
        }

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
        /// Writes the specified dictionary to the buffer<br/>
        /// Ensure you have resolvers for both types!
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dict"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteDictionary<TKey, TValue>(Dictionary<TKey, TValue> dict)
        {
            if (dict is null || dict.Count == 0)
            {
                byte header = dict is null
                    ? IResolver.NullArrayHeader
                    : IResolver.EmptyArrayHeader;

                WriteByte(header);
                return;
            }

            if (!_owner.Node.DataStorage.TryGetResolver<TKey>(out IResolver keyResolver) ||
                !_owner.Node.DataStorage.TryGetResolver<TValue>(out IResolver valueResolver))
            {
                Debug.WriteLine($"WriteDictionary: Ensure resolvers exist for {typeof(TKey).FullName} and {typeof(TValue).FullName}!");
                return;
            }

            IResolver._Internal_WriteHeaderAndLength(ref this, dict.Count);

            foreach (KeyValuePair<TKey, TValue> kvp in dict)
            {
                // Write key value pairs next to each other
                if (!keyResolver.Write(ref this, kvp.Key) ||
                    !valueResolver.Write(ref this, kvp.Value))
                {
                    Debug.WriteLine($"WriteDictionary: Failed to write key or value for Dictionary<{typeof(TKey).FullName}, {typeof(TValue).FullName}>!");
                    return;
                }
            }

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

}

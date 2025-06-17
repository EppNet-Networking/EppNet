///////////////////////////////////////////////////////
/// Filename: BytePayload.cs
/// Date: June 8, 2025
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Node;
using EppNet.Utilities;

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace EppNet.IO
{
    [DebuggerDisplay("Capacity = {Capacity}, Length = {Length}, IsPooled = {IsPooled}")]
    public sealed class BytePayload : INodeDescendant, IDisposable
    {

        public const int HeaderOffset = 1;

        public NetworkNode Node { get; }

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
            get => Buffer.Length > 0 ? Buffer[0] : (byte)0;
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

            this.Node = pool.Node;
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

        internal BytePayload(NetworkNode node, IResizeStrategy resizeStrategy, int capacity)
        {
            this._pool = null;
            this._poolId = -1;

            this.Node = node ?? throw new ArgumentNullException(nameof(node));
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
    
}

///////////////////////////////////////////////////////
/// Filename: VectorResolverBase.cs
/// Date: August 30, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using System;
using System.Runtime.CompilerServices;

namespace EppNet.Data
{

    public abstract class VectorResolverBase<TAdapter, TNative> : Resolver<TNative>
        where TAdapter : struct, IAdapter<TAdapter, float, TNative>
        where TNative : struct, IEquatable<TNative>
    {

        /// <summary>
        /// Signifies that this is a UnitX transmission.
        /// </summary>
        public const byte UnitXHeader = 64 + 0;

        /// <summary>
        /// Signifies that this is a UnitY transmission.
        /// </summary>
        public const byte UnitYHeader = 64 + 1;

        /// <summary>
        /// Signifies that this is a UnitZ transmission.
        /// </summary>
        public const byte UnitZHeader = 64 + 2;

        /// <summary>
        /// Signifies that this is a UnitW transmission.
        /// </summary>
        public const byte UnitWHeader = 64 + 3;

        /// <summary>
        /// Signifies that this is a uniform 1 transmission.
        /// </summary>
        public const byte OneHeader = 64 + 4;

        /// <summary>
        /// Signifies that every component is equal
        /// </summary>
        public const byte UniformHeader = 64 + 5;

        /// <summary>
        /// The default Vector type output (i.e. zero for each component)
        /// </summary>
        public TNative Default { protected set; get; }

        public static TAdapter DefaultAdapter => new();

        public TNative UnitX { protected set; get; }
        public TNative UnitY { protected set; get; }
        public TNative UnitZ { protected set; get; }
        public TNative UnitW { protected set; get; }
        public TNative One { protected set; get; }

        protected VectorResolverBase(bool autoAdvance = true) : base(autoAdvance) { }

        protected VectorResolverBase(int size, bool autoAdvance = true) : base(size, autoAdvance) { }

        protected VectorResolverBase(int size) : base(size) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected HeaderData _Internal_CreateHeaderWithType(TAdapter input, bool signed = false, bool absolute = true)
        {
            int largestTypeIndex = 0;

            // Type indices
            // 0 -> byte or sbyte
            // 1 -> ushort or short
            // 2 -> uint or int
            // 3 -> float

            for (int i = 0; i < input.NumComponents; i++)
            {
                float value = input[i];
                int typeIndex;

                // Floats are the largest type to represent.
                if (value % 1 != 0)
                {
                    // We must use floats for all.
                    largestTypeIndex = 3;
                    break;
                }

                if (signed)
                {
                    if (sbyte.MinValue <= value && value <= sbyte.MaxValue)
                        typeIndex = 0;

                    else if (ushort.MinValue <= value && value <= ushort.MaxValue)
                        typeIndex = 1;

                    else if (uint.MinValue <= value && value <= uint.MaxValue)
                        typeIndex = 2;

                    else
                        typeIndex = 3;
                }
                else
                {
                    if (byte.MinValue <= value && value <= byte.MaxValue)
                        typeIndex = 0;

                    else if (short.MinValue <= value && value <= short.MaxValue)
                        typeIndex = 1;

                    else if (int.MinValue <= value && value <= int.MaxValue)
                        typeIndex = 2;

                    else
                        typeIndex = 3;
                }

                if (typeIndex > largestTypeIndex)
                    largestTypeIndex = typeIndex;
            }

            return new((byte)((absolute ? 128 : 0) | (byte)largestTypeIndex),
                largestTypeIndex, signed, absolute, 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual bool WriteArray(ref BytePayloadWriter writer, TNative[] input, bool absolute = true)
        {
            if (input == null)
            {
                ByteResolver.Instance.Write(ref writer, IResolver.NullArrayHeader);
                return true;
            }
            else if (input.Length == 0)
            {
                ByteResolver.Instance.Write(ref writer, IResolver.EmptyArrayHeader);
                return true;
            }

            IResolver._Internal_WriteHeaderAndLength(ref writer, input.Length);
            bool written = true;

            for (int i = 0; i < input.Length; i++)
            {
                if (!written)
                    break;

                written = Write(ref writer, input[i], absolute);

                if (written && AutoAdvance)
                    writer.Advance(Size);
            }

            return written;
        }

        public bool Write(ref BytePayloadWriter writer, TNative input, bool absolute = true)
        {
            byte header = 0;
            TAdapter adapter = DefaultAdapter.FromNative(input);
            bool isUniform = adapter.AllComponentsEqual();

            if (input.Equals(Default) ||
                input.Equals(UnitX) ||
                input.Equals(UnitY) ||
                input.Equals(UnitZ) ||
                input.Equals(UnitW) ||
                input.Equals(One) ||
                isUniform)
            {
                if (input.Equals(Default))
                    header = 0;

                // This is up here because every single vector has this.
                // Less branch jumping
                else if (input.Equals(One))
                    header = OneHeader;

                else if (input.Equals(UnitX))
                    header = UnitXHeader;

                else if (input.Equals(UnitY))
                    header = UnitYHeader;

                else if (input.Equals(UnitZ))
                    header = UnitZHeader;

                else if (input.Equals(UnitW))
                    header = UnitWHeader;

                else if (isUniform)
                    header = UniformHeader;

                header |= (byte)(absolute ? 128 : 0);
                writer.WriteByte(header);
                return true;
            }

            HeaderData data = _Internal_CreateHeaderWithType(adapter, true, absolute);
            bool written = true;

            header = data.Header;

            // Let's finalize the header
            if (!absolute)
            {
                byte components = 0;

                for (int i = 0; i < adapter.NumComponents; i++)
                {
                    // State which components are being sent
                    if (adapter[i] != 0)
                        components |= (byte)(1 << i);
                }

                byte shifted = (byte)((components & 0b111111) << 2);
                header |= shifted;
            }

            ByteResolver.Instance.Write(ref writer, header);

            for (int i = 0; i < adapter.NumComponents; i++)
            {
                float value = adapter[i];

                if (!absolute && value == 0)
                    continue;

                written = data.TypeIndex switch
                {
                    0 => SByteResolver.Instance.Write(ref writer, (sbyte)value),
                    1 => ShortResolver.Instance.Write(ref writer, (short)value),
                    2 => Int32Resolver.Instance.Write(ref writer, (int)value),
                    _ => FloatResolver.Instance.Write(ref writer, value)
                };

                if (!written)
                    break;
            }

            return written;
        }

        protected override ReadResult _Internal_Read(ref BytePayloadReader reader, out TNative output)
        {
            bool read = reader.TryReadByte(out byte header);
            output = Default;

            if (!read)
                return ReadResult.Failed;

            bool absolute = (header & 0b10000000) != 0;
            int typeIndex = header & 0b11;

            int components = (header >> 2) & 0b1111;

            // Check if we received a special value. Negate the first bit
            TNative? fetched = (header & 0b01111111) switch
            {
                0 => Default,
                UnitXHeader => UnitX,
                UnitYHeader => UnitY,
                UnitZHeader => UnitZ,
                UnitWHeader => UnitW,
                _ => null
            };

            if (fetched.HasValue)
                return absolute ? ReadResult.Success : ReadResult.SuccessDelta;

            ReadResult readResult = ReadResult.Success;

            output = new();
            TAdapter adapter = new();

            for (int i = 0; i < adapter.NumComponents; i++)
            {

                // If this isn't an absolute update, we were only sent
                // components with a bit enabled.
                if (!absolute && ((byte)components & (1 << i)) == 0)
                    continue;

                float value;
                readResult = typeIndex switch
                {
                    0 => SByteResolver.Instance.ReadAs(ref reader, out value),
                    1 => ShortResolver.Instance.ReadAs(ref reader, out value),
                    2 => Int32Resolver.Instance.ReadAs(ref reader, out value),
                    _ => FloatResolver.Instance.ReadAs(ref reader, out value)
                };

                if (!readResult.IsSuccess())
                    return readResult;

                adapter[i] = value;
                AdapterUtils.PutComponent<TAdapter, float, TNative>(adapter, i, value);
            }

            if (readResult.IsSuccess())
                readResult = absolute ? ReadResult.Success : ReadResult.SuccessDelta;

            return readResult;
        }

        protected override bool _Internal_Write(ref BytePayloadWriter writer, TNative input) =>
            Write(ref writer, input, absolute: true);

    }

}
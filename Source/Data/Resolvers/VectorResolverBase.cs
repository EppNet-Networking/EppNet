///////////////////////////////////////////////////////
/// Filename: VectorResolverBase.cs
/// Date: August 30, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.IO;
using EppNet.Utilities;

using System;
using System.Runtime.CompilerServices;

namespace EppNet.Data
{

    public abstract class VectorResolverBase<TNative> : AdaptiveResolverBase<TNative, float>
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

        public TNative UnitX { protected set; get; }
        public TNative UnitY { protected set; get; }
        public TNative UnitZ { protected set; get; }
        public TNative UnitW { protected set; get; }
        public TNative One { protected set; get; }

        protected VectorResolverBase(int numComponents, bool autoAdvance = true) : base(numComponents, autoAdvance) { }

        protected VectorResolverBase(int numComponents, int size, bool autoAdvance = true) : base(numComponents, size, autoAdvance) { }

        protected VectorResolverBase(int numComponents, int size) : base(numComponents, size) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected HeaderData _Internal_CreateHeaderWithType(ref Span<float> components, bool signed = false, bool absolute = true)
        {
            int largestTypeIndex = 0;

            // Type indices
            // 0 -> byte or sbyte
            // 1 -> ushort or short
            // 2 -> uint or int
            // 3 -> float

            for (int i = 0; i < NumComponents; i++)
            {
                float value = components[i];
                int typeIndex = 0;

                int quantized = FastMath.QuantizeToInt(value, 4);

                if (sbyte.MinValue <= quantized && quantized <= sbyte.MaxValue)
                    typeIndex = 0;

                else if (short.MinValue <= quantized && quantized <= short.MaxValue)
                    typeIndex = 1;

                else if (int.MinValue <= quantized && quantized <= int.MaxValue)
                    typeIndex = 2;

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

            Span<float> values = stackalloc float[NumComponents];
            CopyTo(in input, values);

            bool isUniform = FastMath.AllComponentsEqual(values);

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

            HeaderData data = _Internal_CreateHeaderWithType(ref values, true, absolute);
            bool written = true;

            header = data.Header;

            // Let's finalize the header
            if (!absolute)
            {
                byte components = 0;

                for (int i = 0; i < NumComponents; i++)
                {
                    // State which components are being sent
                    if (values[i] != 0)
                        components |= (byte)(1 << i);
                }

                byte shifted = (byte)((components & 0b111111) << 2);
                header |= shifted;
            }

            ByteResolver.Instance.Write(ref writer, header);

            for (int i = 0; i < NumComponents; i++)
            {
                int value = FastMath.QuantizeToInt(values[i], 4);

                if (!absolute && value == 0)
                    continue;

                written = data.TypeIndex switch
                {
                    0 => SByteResolver.Instance.Write(ref writer, (sbyte)value),
                    1 => ShortResolver.Instance.Write(ref writer, (short)value),
                    2 => Int32Resolver.Instance.Write(ref writer, value),
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
            int specialValue = header & 0b01111111;

            TNative fetched = default;
            bool located = false;

            switch (specialValue)
            {

                case 0:
                    fetched = Default;
                    located = true;
                    break;

                case UnitXHeader:
                    fetched = UnitX;
                    located = true;
                    break;

                case UnitYHeader:
                    fetched = UnitY;
                    located = true;
                    break;

                case UnitZHeader:
                    fetched = UnitZ;
                    located = true;
                    break;

                case UnitWHeader:
                    fetched = UnitW;
                    located = true;
                    break;

                case OneHeader:
                    fetched = One;
                    located = true;
                    break;

                default:
                    break;

            }

            if (located)
            {
                output = fetched;
                return absolute ?
                    ReadResult.Success :
                    ReadResult.SuccessDelta;
            }

            ReadResult readResult = ReadResult.Success;
            Span<float> values = stackalloc float[NumComponents];

            if (specialValue == UniformHeader)
            {
                int value;
                readResult = typeIndex switch
                {
                    0 => SByteResolver.Instance.ReadAs(ref reader, out value),
                    1 => ShortResolver.Instance.ReadAs(ref reader, out value),
                    2 => Int32Resolver.Instance.ReadAs(ref reader, out value),
                    _ => FloatResolver.Instance.ReadAs(ref reader, out value)
                };

                if (!readResult.IsSuccess())
                    return readResult;

                float result = FastMath.DequantizeInt(value, 4);

                for (int i = 0; i < NumComponents; i++)
                    values[i] = result;
            }
            else
            {
                for (int i = 0; i < NumComponents; i++)
                {

                    // If this isn't an absolute update, we were only sent
                    // components with a bit enabled.
                    if (!absolute && ((byte)components & (1 << i)) == 0)
                        continue;

                    int value;
                    readResult = typeIndex switch
                    {
                        0 => SByteResolver.Instance.ReadAs(ref reader, out value),
                        1 => ShortResolver.Instance.ReadAs(ref reader, out value),
                        2 => Int32Resolver.Instance.ReadAs(ref reader, out value),
                        _ => FloatResolver.Instance.ReadAs(ref reader, out value)
                    };

                    if (!readResult.IsSuccess())
                        return readResult;

                    values[i] = FastMath.DequantizeInt(value, 4);
                }
            }

            if (readResult.IsSuccess())
                readResult = absolute ? ReadResult.Success : ReadResult.SuccessDelta;

            output = ToNative(values);
            return readResult;
        }

        protected override bool _Internal_Write(ref BytePayloadWriter writer, TNative input) =>
            Write(ref writer, input, absolute: true);

    }

}

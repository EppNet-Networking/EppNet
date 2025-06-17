///////////////////////////////////////////////////////
/// Filename: QuaternionResolverBase.cs
/// Date: August 29, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using EppNet.Utilities;

namespace EppNet.Data
{

    public abstract class QuaternionResolverBase<TNative> : AdaptiveResolverBase<TNative, float>
        where TNative : struct, IEquatable<TNative>
    {
        /// <summary>
        /// Whether or not to use byte quantization for packing Quaternions<br/>
        /// This defaults to true as the cost per quaternion with quantization is 4 bytes total
        /// </summary>
        public static bool ByteQuantization { set; get; } = true;

        /// <summary>
        /// Identity quaternions send this value to save bandwidth and computation time
        /// </summary>
        public const byte IdentityHeader = 32;

        /// <summary>
        /// Zero quaternions send this value to send bandwidth and computation time
        /// </summary>
        public const byte ZeroHeader = 16;

        public TNative Zero { protected set; get; }
        public TNative Identity { protected set; get; }

        public new int NumComponents => 4;

        protected QuaternionResolverBase() : base(numComponents: 4, autoAdvance: false) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]

        protected override ReadResult _Internal_Read(ref BytePayloadReader reader, out TNative output)
        {
            // Let's consider the header
            byte header = reader.ReadByte();

            if (header == IdentityHeader || header == ZeroHeader)
            {
                // Yay! Just match the quaternion and return!
                output = header == IdentityHeader ? Identity : Zero;
                return ReadResult.Success;
            }

            // We didn't have the easy way out. Consider the most significant bit.
            bool negative = (header & (1 << 7)) != 0;
            int largestIndex = BitOperations.TrailingZeroCount(negative ? (byte)(header & ~(1 << 7)) : header);
            Span<float> components = stackalloc float[4];

            for (int i = 0; i < components.Length; i++)
            {
                if (i == largestIndex)
                {
                    components[i] = 0f;
                    continue;
                }

                if (!ByteQuantization)
                {
                    // Read the float like any other (this auto advances)
                    FloatResolver.Instance.Read(ref reader, out components[i]);
                    continue;
                }

                // Byte dequantization
                components[i] = FastMath.DequantizeByte(reader.ReadByte());
            }

            float sumOfSquares = 1.0f - (
                (components[0] * components[0]) +
                (components[1] * components[1]) +
                (components[2] * components[2]) +
                (components[3] * components[3])
            );

            // Generate the largest value
            components[largestIndex] = MathF.Sqrt(MathF.Max(0f, sumOfSquares)) * (negative ? -1 : 1);

            output = ToNative(components);
            return ReadResult.Success;
        }

        /// <summary>
        /// This is an implementation of the Smallest Three algorithm<br/>
        /// A byte header is sent indicating the index of largest component, and
        /// if the sign is 
        /// If <see cref="ByteQuantization"/>:<br/>
        /// - 4 bytes are sent (1 byte header indicating index of largest 
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="input"></param>
        /// <returns></returns>

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override bool _Internal_Write(ref BytePayloadWriter writer, TNative input)
        {

            Span<float> data = stackalloc float[NumComponents];
            CopyTo(in input, data);

            // Identity and zero quats use a single byte

            if (FastMath.Quaternion_ApproximatelyEquals(
                0, 0, 0, 1,
                data[0], data[1], data[2], data[3]))
            {
                writer.WriteByte(IdentityHeader);
                return true;
            }

            if (FastMath.Quaternion_ApproximatelyEquals(
                0, 0, 0, 0,
                data[0], data[1], data[2], data[3]))
            {
                writer.WriteByte(ZeroHeader);
                return true;
            }

            data = FastMath.Quaternion_Normalize(ref data);

            int largestIndex = 0;
            float largest = MathF.Abs(data[0]);
            bool negative = data[0] < 0;

            for (int i = 1; i < NumComponents; i++)
            {
                float f = data[i];
                float value = MathF.Abs(f);

                if (value > largest)
                {
                    negative = f < 0;
                    largest = value;
                    largestIndex = i;
                }
            }

            // Header includes the following:
            // a bit turned on indicating the largest index
            // and the leading bit on if the largest value is negative
            byte header = (byte)(1 << (byte)largestIndex);

            if (negative)
                header |= 1 << 7;

            writer.WriteByte(header);

            if (ByteQuantization)
            {
                Span<byte> bytes = writer.Reserve(3, clear: false);
                int byteIndex = 0;
                int quatIndex = 0;

                while (byteIndex < bytes.Length)
                {
                    if (quatIndex == largestIndex)
                    {
                        quatIndex++;
                        continue;
                    }

                    // Quantize the float into a byte
                    bytes[byteIndex++] = FastMath.QuantizeToByte(data[quatIndex++]);
                }
            }
            else
            {
                // Byte quantization is off. Let's send the floats
                for (int i = 0; i < NumComponents; i++)
                {
                    if (i == largestIndex)
                        continue;

                    FloatResolver.Instance.Write(ref writer, data[i]);
                }
            }

            return true;
        }

    }

}

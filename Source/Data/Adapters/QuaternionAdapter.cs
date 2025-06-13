///////////////////////////////////////////////////////
/// Filename: QuaternionAdapter.cs
/// Date: September 2, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Utilities;

using System;
using System.Numerics;
using System.Runtime.CompilerServices;


namespace EppNet.Data
{

    public struct QuaternionAdapter : IAdapter<QuaternionAdapter, float, Quaternion>
    {

        public static implicit operator Quaternion(QuaternionAdapter a) =>
            new(a.X, a.Y, a.Z, a.W);

        public static explicit operator QuaternionAdapter(Quaternion q) =>
            new(q.X, q.Y, q.Z, q.W);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static QuaternionAdapter Normalize(QuaternionAdapter quat)
        {
            float len = quat.Length();
            return (len < FastMath.QuaternionEpsilon) ? new(0, 0, 0, 1) : new(
                quat.X / len,
                quat.Y / len,
                quat.Z / len,
                quat.W / len);
        }

        /// <summary>
        /// Maps a floating point number from [-1, 1] to [0, 255]<br/>
        /// <b>NOTE:</b> Input float must be normalized!
        /// </summary>
        /// <param name="input"></param>
        /// <returns>Byte in domain [0, 255]</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte Quantize(float input)
            => (byte)MathF.Round((input + 1.0f) * FastMath.ByteQuantizer);

        /// <summary>
        /// Maps a byte value from [0, 255] back to [-1, 1]
        /// </summary>
        /// <param name="input"></param>
        /// <returns>Float in domain [-1, 1]</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Dequantize(byte input) =>
            FastMath.Dequantize(input);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static QuaternionAdapter Quantized(QuaternionAdapter input)
        {
            QuaternionAdapter result = Normalize(input);

            return new QuaternionAdapter(
                Quantize(result.X),
                Quantize(result.Y),
                Quantize(result.Z),
                Quantize(result.W));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static QuaternionAdapter Dequantized(QuaternionAdapter input)
        {
            Span<float> result = stackalloc float[4];
            result[0] = input.X;
            result[1] = input.Y;
            result[2] = input.Z;
            result[3] = input.W;

            for (int i = 0; i < 4; i++)
                result[i] = Dequantize((byte)result[i]);

            int largestIndex = 0;
            float largest = MathF.Abs(result[0]);
            bool negative = result[0] < 0;

            for (int i = 1; i < 4; i++)
            {
                if (MathF.Abs(result[i]) > largest)
                {
                    largestIndex = i;
                    largest = MathF.Abs(result[i]);
                    negative = result[i] < 0;
                }
            }

            result[largestIndex] = 0;

            largest = MathF.Sqrt(1.0f - (MathF.Pow(result[0], 2f) + MathF.Pow(result[1], 2f)
                + MathF.Pow(result[2], 2f) + MathF.Pow(result[3], 2f)));

            result[largestIndex] = largest * (negative ? -1 : 1);
            return new QuaternionAdapter(result[0], result[1],
                result[2], result[3]);
        }

        public readonly int NumComponents => 4;

        public float X;
        public float Y;
        public float Z;
        public float W;

        public QuaternionAdapter(float x, float y, float z, float w)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
            this.W = w;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly float Length()
        {
            float dotProd = (X * X) + (Y * Y) + (Z * Z) + (W * W);
            return MathF.Sqrt(dotProd);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool ApproximatelyEquals(QuaternionAdapter other, float epsilon = FastMath.QuaternionEpsilon)
        {
            float dot = (X * other.X) + (Y * other.Y) + (Z * other.Z) + (W * other.W);
            return MathF.Abs(dot) > 1f - epsilon;
        }

        public float this[int index]
        {

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get
            {
                return index switch
                {
                    0 => X,
                    1 => Y,
                    2 => Z,
                    3 => W,
                    _ => throw new ArgumentOutOfRangeException(nameof(index)),
                };
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                this = index switch
                {
                    0 => new(value, Y, Z, W),
                    1 => new(X, value, Z, W),
                    2 => new(X, Y, value, W),
                    3 => new(X, Y, Z, value),
                    _ => throw new ArgumentOutOfRangeException(nameof(index))
                };
            }

        }

        public readonly QuaternionAdapter FromNative(in Quaternion quaternion) =>
            new(quaternion.X, quaternion.Y, quaternion.Z, quaternion.W);

        public readonly Quaternion ToNative() =>
            new(X, Y, Z, W);

        public override readonly string ToString() =>
            $"({X}, {Y}, {Z}, {W})";
    }

}

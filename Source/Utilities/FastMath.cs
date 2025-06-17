///////////////////////////////////////////////////////
/// Filename: FastMath.cs
/// Date: September 10, 2023
/// Author: Maverick Liberty, Will Calderwood
/// Link: Modified adaptation of 
/// https://stackoverflow.com/a/48448292
///////////////////////////////////////////////////////
using System;
using System.Runtime.CompilerServices;

namespace EppNet.Utilities
{
    public static class FastMath
    {
        /// <summary>
        /// How many decimal places to cache
        /// </summary>
        public const int CachedDecimals = 15;

        public const int CachedDequantizedFloats = 256;

        /// <summary>
        /// Magic number for quaternion quantization
        /// </summary>
        public const float ByteQuantizer = 127.5f;

        public const float Epsilon = 0.001f;

        /// <summary>
        /// Epsilon for quaternion comparisons
        /// </summary>
        public const float QuaternionEpsilon = 1e-6f;

        private static readonly double[] _roundLookup = CreateRoundLookup();
        private static readonly float[] _dequantizeTable = CreateDequantizeLookup();

        /// <summary>
        /// Essentially a wrapper around <see cref="Math.Pow(double, double)"/> with 10^index<br/>
        /// Indices from 0 to <see cref="CachedDecimals"/> - 1 are cached.
        /// </summary>
        /// <param name="index"></param>
        /// <returns>Math.Pow(10, index)</returns>
        public static double GetTenPow(int index)
        {
            if (0 <= index && index < CachedDecimals)
                return _roundLookup[index];

            return Math.Pow(10, index);
        }

        private static float[] CreateDequantizeLookup()
        {
            float[] result = new float[CachedDequantizedFloats];

            for (int i = 0; i < CachedDequantizedFloats; i++)
                result[i] = (i / ByteQuantizer) - 1.0f;

            return result;
        }

        private static double[] CreateRoundLookup()
        {
            double[] result = new double[CachedDecimals];

            for (int i = 0; i < result.Length; i++)
                result[i] = Math.Pow(10, i);

            return result;
        }

        /// <summary>
        /// Maps a floating point number from [-1, 1] to [0, 255]<br/>
        /// <b>NOTE:</b> Input float must be normalized!
        /// </summary>
        /// <param name="input"></param>
        /// <returns>Byte in domain [0, 255]</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte QuantizeToByte(float input)
        {
            float clamped = Math.Clamp((input + 1f) * ByteQuantizer, 0f, 255f);
            return (byte) Math.Round(clamped);
        }

        /// <summary>
        /// Maps a byte value from [0, 255] back to [-1, 1]
        /// </summary>
        /// <param name="input"></param>
        /// <returns>Float in domain [-1, 1]</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float DequantizeByte(this byte b) =>
            _dequantizeTable[b];

        public static int QuantizeToInt(float value, int decimals)
        {
            double scale = GetTenPow(decimals);
            return (int)Math.Round(value * scale);
        }

        public static float DequantizeInt(int quantized, int decimals)
        {
            double scale = GetTenPow(decimals);
            return (float)(quantized / scale);
        }

        /// <summary>
        /// Rounds the specified number to the specified decimal places<br/>
        /// - Rounding to 0 decimal places returns 1
        /// - Rounding to <0 decimal places returns the provided number
        /// </summary>
        /// <returns></returns>
        public static double Round(this float f, int decimals) =>
            ((double)f).Round(decimals);

        /// <summary>
        /// Rounds the specified number to the specified decimal places<br/>
        /// - Rounding to 0 decimal places returns 1
        /// - Rounding to <0 decimal places returns the provided number
        /// </summary>
        /// <returns></returns>

        public static double Round(this double d, int decimals)
        {
            double result;

            if (decimals == 0)
            {
                // Just return 1 as 10^0 is 1.
                result = 1d;
            }
            else if (decimals < 0)
            {
                // We don't support rounding to negative decimal places.
                result = d;
            }
            else
            {
                double adjustment = GetTenPow(decimals);
                result = Math.Floor((d * adjustment) + 0.5d) / adjustment;
            }

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Quaternion_ApproximatelyEquals(
            float aX, float aY, float aZ, float aW,
            float bX, float bY, float bZ, float bW,
            float epsilon = QuaternionEpsilon)
        {
            float dot = (aX * bX) + (aY * bY) + (aZ * bZ) + (aW * bW);
            return MathF.Abs(dot) > 1f - epsilon;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Quaternion_Length(float x, float y, float z, float w)
        {
            float dotProd = (x * x) + (y * y) + (z * z) + (w * w);
            return MathF.Sqrt(dotProd);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<float> Quaternion_Normalize(ref Span<float> data)
        {
            if (data.Length != 4)
            {
                data.Clear();
                return data;
            }

            float length = Quaternion_Length(data[0], data[1], data[2], data[3]);

            if (length < QuaternionEpsilon)
            {
                data.Clear();
                return data;
            }

            for (int i = 0; i < data.Length; i++)
                    data[i] /= length;

            return data;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool AllComponentsEqual(Span<float> values)
        {
            float first = values[0];
            for (int i = 1; i < values.Length; i++)
            {
                if (values[i] != first)
                    return false;
            }
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (float, float, float, float) Quaternion_Normalize(float x, float y, float z, float w)
        {
            float length = Quaternion_Length(x, y, z, w);

            if (length < QuaternionEpsilon)
                return (0, 0, 0, 1);

            return
            (
                x / length,
                y / length,
                z / length,
                w / length
            );
        }

        public static bool TryCastToFloat<T>(T value, out float result)
        {
            result = 0f;

            switch (value)
            {
                case float f:
                    result = f;
                    return true;

                case double d:
                    result = (float)d;
                    return true;

                case int i:
                    result = i;
                    return true;

                case uint ui:
                    result = ui;
                    return true;

                case long l:
                    result = l;
                    return true;

                case ulong ul:
                    result = ul;
                    return true;

                case short s:
                    result = s;
                    return true;

                case ushort us:
                    result = us;
                    return true;

                case byte b:
                    result = b;
                    return true;

                case sbyte sb:
                    result = sb;
                    return true;

                case decimal dec:
                    result = (float)dec;
                    return true;

                default:
                    return false;
            }
        }


    }
}

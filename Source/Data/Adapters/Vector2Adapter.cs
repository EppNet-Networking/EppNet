///////////////////////////////////////////////////////
/// Filename: Vector2Adapter.cs
/// Date: June 13, 2025
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using EppNet.Utilities;

namespace EppNet.Data
{

    public struct Vector2Adapter : IVectorAdapter<Vector2Adapter, float, Vector2>
    {

        public static Vector2Adapter FromNativeVector(in Vector2 vector) =>
            new(vector.X, vector.Y);

        public float X { set; get; }
        public float Y { set; get; }

        public readonly int NumComponents => 2;

        public float this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get => index switch
            {
                0 => X,
                1 => Y,
                _ => throw new ArgumentOutOfRangeException(nameof(index))
            };

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                switch (index)
                {
                    case 0:
                        X = value;
                        break;

                    case 1:
                        Y = value;
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(index));
                }
            }

        }

        public Vector2Adapter(float x, float y)
        {
            this.X = x;
            this.Y = y;
        }

        public readonly Vector2 ToNativeVector() => new(X, Y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void CopyTo(Span<float> data)
        {
            if (data.Length < NumComponents)
                throw new ArgumentException(nameof(data));

            data[0] = X;
            data[1] = Y;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool ApproximatelyEquals(Vector2Adapter other, float epsilon = FastMath.Epsilon) =>
            MathF.Abs(X - other.X) <= epsilon &&
            MathF.Abs(Y - other.Y) <= epsilon;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool AllComponentsEqual(float epsilon = FastMath.Epsilon) =>
            X.ApproximatelyEquals(Y, epsilon);

        public readonly override string ToString() =>
             $"{X}, {Y}";

    }

}

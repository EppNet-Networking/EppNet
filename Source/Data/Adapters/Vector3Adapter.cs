///////////////////////////////////////////////////////
/// Filename: Vector3Adapter.cs
/// Date: June 13, 2025
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using EppNet.Utilities;

namespace EppNet.Data
{

    public struct Vector3Adapter : IVectorAdapter<Vector3Adapter, float, Vector3>
    {

        public static Vector3Adapter FromNativeVector(in Vector3 other) =>
            new(other.X, other.Y, other.Z);

        public float X { set; get; }
        public float Y { set; get; }
        public float Z { set; get; }

        public readonly int NumComponents => 3;

        public float this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get => index switch
            {
                0 => X,
                1 => Y,
                2 => Z,
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

                    case 2:
                        Z = value;
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(index));
                }
            }
        }

        public Vector3Adapter(float x, float y, float z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        public readonly Vector3 ToNativeVector() =>
            new(X, Y, Z);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void CopyTo(Span<float> data)
        {
            if (data.Length < NumComponents)
                throw new ArgumentException(nameof(data));

            data[0] = X;
            data[1] = Y;
            data[2] = Z;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool ApproximatelyEquals(Vector3Adapter other, float epsilon = FastMath.Epsilon) =>
            MathF.Abs(X - other.X) <= epsilon &&
            MathF.Abs(Y - other.Y) <= epsilon &&
            MathF.Abs(Z - other.Z) <= epsilon;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool AllComponentsEqual(float epsilon) =>
            ApproximatelyEquatableExtensions.AreApproximatelyEqual(X, Y, Z, epsilon);

        public override readonly string ToString() =>
            $"({X}, {Y}, {Z})";

    }

}

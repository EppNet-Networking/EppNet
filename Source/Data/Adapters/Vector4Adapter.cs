///////////////////////////////////////////////////////
/// Filename: Vector4Adapter.cs
/// Date: June 13, 2025
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace EppNet.Data
{

    public struct Vector4Adapter : IAdapter<Vector4Adapter, float, Vector4>
    {

        public float X { set; get; }
        public float Y { set; get; }
        public float Z { set; get; }
        public float W { set; get; }

        public readonly int NumComponents => 4;

        public float this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get => index switch
            {
                0 => X,
                1 => Y,
                2 => Z,
                3 => W,
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

                    case 3:
                        W = value;
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(index));
                }
            }
        }

        public Vector4Adapter(float x, float y, float z, float w)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
            this.W = w;
        }

        public readonly Vector4Adapter FromNative(in Vector4 other) =>
            new(other.X, other.Y, other.Z, other.W);

        public readonly Vector4 ToNative() =>
            new(X, Y, Z, W);

        public override readonly string ToString() =>
            $"({X}, {Y}, {Z}, {W})";

    }

}

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

    public struct Vector3Adapter : IAdapter<Vector3Adapter, float, Vector3>
    {

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

        public readonly Vector3Adapter FromNative(in Vector3 vector) =>
            new(vector.X, vector.Y, vector.Z);

        public readonly Vector3 ToNative() =>
            new(X, Y, Z);

        public override readonly string ToString() =>
            $"({X}, {Y}, {Z})";

    }

}

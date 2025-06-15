///////////////////////////////////////////////////////
/// Filename: VectorAdapters.cs
/// Date: June 14, 2025
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace EppNet.Data
{

    public static class VectorAdaptersExtensions
    {

        public static Vector2Adapter FromNative(in Vector2 vector) =>
            new(vector.X, vector.Y);

    }

    public struct Vector2Adapter : IAdapter<Vector2Adapter, float, Vector2>
    {

        public float X { set; get; }
        public float Y { set; get; }

        public readonly int NumComponents => 2;

        public Vector2Adapter(in Vector2 native) 
            : this(native.X, native.Y)
        { }

        public Vector2Adapter(float x, float y)
        {
            this.X = x;
            this.Y = y;
        }

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

        public Vector2Adapter FromNative(in Vector2 vector)
        {
            this.X = vector.X;
            this.Y = vector.Y;
            return this;
        }

        public readonly Vector2 ToNative()
            => new(X, Y);

        public readonly override string ToString() =>
             $"{X}, {Y}";

    }

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

        public Vector3Adapter(in Vector3 native)
            : this(native.X, native.Y, native.Z)
        { }

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

        public Vector4Adapter(in Vector4 native) 
            : this(native.X, native.Y, native.Z, native.W)
        {}

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
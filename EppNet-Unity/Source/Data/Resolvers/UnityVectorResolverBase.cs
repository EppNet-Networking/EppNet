///////////////////////////////////////////////////////
/// Filename: UnityVectorResolverBase.cs
/// Date: March 29, 2025
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Data;

using System;
using System.Runtime.CompilerServices;

namespace EppNet.Unity.Data
{

    public abstract class UnityVectorResolverBase<T> : VectorResolverBase<T> where T : struct, IEquatable<T>
    {
        protected UnityVectorResolverBase(bool autoAdvance = true) : base(autoAdvance) { }

        protected UnityVectorResolverBase(int size, bool autoAdvance = true) : base(size, autoAdvance) { }

        protected UnityVectorResolverBase(int size) : base(size) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void ExtractFloatComponents(T input, Span<float> values)
        {
            switch (input)
            {

                case UnityEngine.Vector4 uv4:
                    values[0] = uv4.x;
                    values[1] = uv4.y;
                    values[2] = uv4.z;
                    values[3] = uv4.w;
                    break;

                case UnityEngine.Vector3 uv3:
                    values[0] = uv3.x;
                    values[1] = uv3.y;
                    values[2] = uv3.z;
                    break;

                case UnityEngine.Vector3Int uv3i:
                    values[0] = uv3i.x;
                    values[1] = uv3i.y;
                    values[2] = uv3i.z;
                    break;

                case UnityEngine.Vector2 uv2:
                    values[0] = uv2.x;
                    values[1] = uv2.y;
                    break;

                case UnityEngine.Vector2Int uv2i:
                    values[0] = uv2i.x;
                    values[1] = uv2i.y;
                    break;

                default:
                    base.ExtractFloatComponents(input, values);
                    break;

            }
        }
    }

}

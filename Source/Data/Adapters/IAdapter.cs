///////////////////////////////////////////////////////
/// Filename: IAdapter.cs
/// Date: June 13, 2025
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using System;
using System.Runtime.CompilerServices;
using EppNet.Utilities;

namespace EppNet.Data
{

    public interface IAdapter
    {
        public int NumComponents { get; }
    }

    public interface IAdapter<TAdapter, TComponent, TNative> : IAdapter, IEquatable<TAdapter>, IApproximatelyEquatable<TAdapter>
        where TAdapter : struct, IAdapter<TAdapter, TComponent, TNative>
        where TComponent : struct, IComparable<TComponent>
        where TNative : struct
    {

        TComponent this[int index] { set; get; }

        TAdapter FromNative(in TNative native);
        TNative ToNative();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void CopyTo(Span<TComponent> data)
        {
            if (data.Length < NumComponents)
                throw new ArgumentException(nameof(data));

            for (int i = 0; i < NumComponents; i++)
                data[i] = this[i];
        }

        TComponent[] AsArray()
        {
            TComponent[] data = new TComponent[NumComponents];
            for (int i = 0; i < NumComponents; i++)
                data[i] = this[i];

            return data;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        bool IApproximatelyEquatable<TAdapter>.ApproximatelyEquals(TAdapter other, float epsilon)
        {
            for (int i = 0; i < NumComponents; i++)
            {
                if (!ApproximatelyEquatableExtensions.ApproximatelyEquals(this[i], other[i], epsilon))
                    return false;
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        bool AllComponentsEqual(float epsilon = FastMath.Epsilon)
        {
            if (NumComponents == 1)
                return true;

            TComponent first = this[0];

            for (int i = 1; i < NumComponents; i++)
            {
                if (!ApproximatelyEquatableExtensions.ApproximatelyEquals(first, this[i], epsilon))
                    return false;
            }

            return true;
        }

        bool IEquatable<TAdapter>.Equals(TAdapter other)
        {
            for (int i = 0; i < NumComponents; i++)
            {
                if (!this[i].Equals(other[i]))
                    return false;
            }

            return true;
        }

    }

}
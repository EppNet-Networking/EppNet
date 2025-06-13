///////////////////////////////////////////////////////
/// Filename: IVectorAdapter.cs
/// Date: June 13, 2025
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using System;
using EppNet.Utilities;

namespace EppNet.Data
{

    public interface IVectorAdapter<TAdapter, TComponent, TVector> : IApproximatelyEquatable<TAdapter>
        where TAdapter : struct
        where TComponent : struct, IEquatable<TComponent>
        where TVector : struct, IEquatable<TVector>
    {
        TComponent X { set; get; }

        /// <summary>
        /// How many components are involved in this vector
        /// </summary>
        int NumComponents { get; }

        TComponent this[int index] { get; }

        TVector ToNativeVector();

        void CopyTo(Span<TComponent> data);

        bool AllComponentsEqual(float epsilon = FastMath.Epsilon);

    }

}
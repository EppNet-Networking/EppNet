///////////////////////////////////////////////////////
/// Filename: TransformResolverBase.cs
/// Date: June 13, 2025
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using System;

namespace EppNet.Data
{

    public abstract class TransformResolverBase<TVector, TQuaternion, TVectorResolver, TQuaternionResolver, TTransform>
        where TVectorResolver : VectorResolverBase<TVector>
        where TQuaternionResolver : QuaternionResolverBase<TQuaternion>
        where TVector : struct, IEquatable<TVector>
        where TQuaternion : struct, IEquatable<TQuaternion>
        where TTransform : struct, IEquatable<TTransform>
    {
        /// <summary>
        /// The identity transform header<br/>
        /// (0, 0, 0) Position<br/>
        /// (0, 0, 0, 0) Rotation <br/>
        /// (1, 1, 1) Scale
        /// </summary>
        public const byte IdentityHeader = 32;

        public abstract TransformAdapter<TVector, TQuaternion> ToAdapter(in TTransform transform);

    }

}

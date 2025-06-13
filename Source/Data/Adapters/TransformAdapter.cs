///////////////////////////////////////////////////////
/// Filename: TransformAdapter.cs
/// Date: June 13, 2025
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using System;

namespace EppNet.Data
{

    public struct TransformAdapter<TVector, TQuaternion>
        where TVector : struct, IEquatable<TVector>
        where TQuaternion : struct, IEquatable<TQuaternion>
    {

        public TVector Position;
        public TQuaternion Rotation;
        public TVector Scale;

    }

}
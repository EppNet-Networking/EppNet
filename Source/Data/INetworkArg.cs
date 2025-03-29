///////////////////////////////////////////////////////
/// Filename: INetworkArg.cs
/// Date: March 28, 2025
/// Author: Maverick Liberty
///////////////////////////////////////////////////////
using System;

namespace EppNet.Data
{

    public interface INetworkArg : ICloneable<INetworkArg>, IEquatable<INetworkArg>, ISignatureEquatable<INetworkArg>
    {
        public Type Type { get; }

        public object BoxedValue { get; }

        public object Get() =>
            BoxedValue;
    }

    public interface INetworkArg<TArg> : INetworkArg
    {
        public TArg Value { get; }

        public new TArg Get() =>
            Value;
    }

}
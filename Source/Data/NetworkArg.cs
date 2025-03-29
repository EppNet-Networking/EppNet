///////////////////////////////////////////////////////
/// Filename: NetworkArg.cs
/// Date: March 28, 2025
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

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

    public sealed class NetworkArg<TArg> : INetworkArg<TArg>
    {

        public TArg Value { set; get; }

        public object BoxedValue
        {
            set
            {
                if (value is TArg arg)
                    Value = arg;
                else
                    throw new ArgumentException($"Value must be of type {typeof(TArg)}");
            }

            get => Value;
        }

        public Type Type { get => typeof(TArg); }

        public NetworkArg() { }

        public NetworkArg(TArg value)
        {
            this.Value = value;
        }

        public INetworkArg Clone() =>
            new NetworkArg<TArg>(Value);

        public bool Equals(INetworkArg other) =>
            other is NetworkArg<TArg> oNetArg &&
            EqualityComparer<TArg>.Default.Equals(Value, oNetArg.Value);

        public bool SignatureEquals(INetworkArg other) =>
            other is NetworkArg<TArg>;
    }

}

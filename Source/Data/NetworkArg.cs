///////////////////////////////////////////////////////
/// Filename: NetworkArg.cs
/// Date: March 28, 2025
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using System;

namespace EppNet.Data
{

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
                    throw new ArgumentException("arg");
            }

            get => Value;
        }

        public IResolver<TArg> Resolver { private set; get; }

        public Type Type { get => typeof(TArg); }

        public NetworkArg() { }

        public NetworkArg(TArg value)
        {
            this.Value = value;
        }

        public INetworkArg Clone() =>
            new NetworkArg<TArg>(Value);

        public bool Equals(INetworkArg other) =>
            other is not null &&
            other is NetworkArg<TArg> oNetArg &&
            Value.Equals(oNetArg.Value);

        public bool SignatureEquals(INetworkArg other) =>
            other is not null &&
            other is NetworkArg<TArg>;
    }

}

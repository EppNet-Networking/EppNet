///////////////////////////////////////////////////////
/// Filename: NetworkArg.cs
/// Date: March 28, 2025
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

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

        /// <summary>
        /// The object wrapped by this class.
        /// </summary>

        public TArg Value { set; get; }

        /// <summary>
        /// <strong>NOTE:</strong> Uses boxing to set the internal value!<br/>
        /// Prefer typesafe assignment with <see cref="Type"/>
        /// </summary>

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

        /// <summary>
        /// The <see cref="System.Type"/> of the value wrapped by this class.
        /// </summary>

        public Type Type { get => typeof(TArg); }

        public NetworkArg() { }

        public NetworkArg(TArg value)
        {
            this.Value = value;
        }

        /// <summary>
        /// Creates a "shallow" copy; meaning, if <see cref="Value"/><br/>
        /// is a reference type, the copy will refer to the existing object.
        /// </summary>
        /// <returns></returns>

        public INetworkArg Clone() =>
            new NetworkArg<TArg>(Value);

        /// <summary>
        /// Checks if the provided <see cref="INetworkArg"/> is of the
        /// same type AND<br/> the value is equivalent according to the
        /// default <see cref="EqualityComparer"/> for <typeparamref name="TArg"/>.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(INetworkArg other) =>
            other is NetworkArg<TArg> oNetArg &&
            EqualityComparer<TArg>.Default.Equals(Value, oNetArg.Value);

        /// <summary>
        /// Checks if the provided <see cref="INetworkArg"/> wraps an equivalent
        /// <typeparamref name="TArg"/>.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool SignatureEquals(INetworkArg other) =>
            other is NetworkArg<TArg>;

        /// <summary>
        /// Checks if the provided <see cref="INetworkArg"/> wraps a compatible
        /// <typeparamref name="TArg"/>.<br/>
        /// Compatibility is:<br/>
        /// - Type argument equivalence; OR<br/>
        /// - Direct or indirect type derivation
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>

        public bool IsCompatibleWith(INetworkArg other) =>
            SignatureEquals(other) ||
            other is not null &&
            (other.Type.IsAssignableFrom(Type) ||
            Type.IsAssignableFrom(other.Type));

        public override int GetHashCode() =>
            Value?.GetHashCode() ?? 0;
    }

}

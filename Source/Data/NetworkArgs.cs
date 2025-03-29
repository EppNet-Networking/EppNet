///////////////////////////////////////////////////////
/// Filename: NetworkArgs.cs
/// Date: March 28, 2025
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace EppNet.Data
{

    public interface INetworkArgs
    {
        public int Count { get; }

        public bool Set(int index, INetworkArg arg);

        public INetworkArg Get(int index);

        public object GetBoxedValue(int index);
    }

    public abstract class NetworkArgs : INetworkArgs, IEquatable<NetworkArgs>, 
        ISignatureEquatable<NetworkArgs>, ICloneable<NetworkArgs>, IEnumerable<INetworkArg>
    {

        #region Factory methods

        public static NetworkArgs<TArg1> From<TArg1>(TArg1 arg1) => 
            new (arg1);

        public static NetworkArgs<TArg1, TArg2> From<TArg1, TArg2>(TArg1 arg1, TArg2 arg2) =>
            new(arg1, arg2);

        public static NetworkArgs<TArg1, TArg2, TArg3> From<TArg1, TArg2, TArg3>(TArg1 arg1, TArg2 arg2, TArg3 arg3) =>
            new(arg1, arg2, arg3);

        public static NetworkArgs<TArg1, TArg2, TArg3, TArg4> From<TArg1, TArg2, TArg3, TArg4>(TArg1 arg1, TArg2 arg2, TArg3 arg3, 
            TArg4 arg4) =>
            new(arg1, arg2, arg3, arg4);

        public static NetworkArgs<TArg1, TArg2, TArg3, TArg4, TArg5> From<TArg1, TArg2, TArg3, TArg4, TArg5>(TArg1 arg1, TArg2 arg2, 
            TArg3 arg3, TArg4 arg4, TArg5 arg5) =>
            new(arg1, arg2, arg3, arg4, arg5);

        public static NetworkArgs<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6> From<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6>(TArg1 arg1, TArg2 arg2,
            TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6) =>
            new(arg1, arg2, arg3, arg4, arg5, arg6);

        public static NetworkArgs<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7> From<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7>(TArg1 arg1, TArg2 arg2,
            TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7) =>
            new(arg1, arg2, arg3, arg4, arg5, arg6, arg7);

        #endregion

        public int Count { get; }

        public INetworkArg this[int index]
        {
            set => Set(index, value);
            get => Get(index);
        }

        private readonly INetworkArg[] _args;

        protected NetworkArgs(int count)
        {
            this.Count = count;
            this._args = new INetworkArg[count];
        }

        public bool Set(int index, INetworkArg arg)
        {
            if (index < 0 || index >= Count)
                throw new IndexOutOfRangeException($"Index must be at least 0 and less than {Count}. Got: {index}.");

            _args[index] = arg;
            return true;
        }

        public INetworkArg Get(int index)
        {
            if (index < 0 || index >= Count)
                throw new IndexOutOfRangeException($"Index must be at least 0 and less than {Count}. Got: {index}.");

            return _args[index];
        }

        /// <summary>
        /// Tries to set the value held by the argument at the specified index
        /// </summary>
        /// <typeparam name="TArg"></typeparam>
        /// <param name="index"></param>
        /// <param name="value"></param>
        /// <returns>Whether or not the argument was updated</returns>

        public bool TrySetArg<TArg>(int index, TArg value)
        {
            if (index < 0 || index >= Count)
                return false;

            if (_args[index] is NetworkArg<TArg> typedArg)
            {
                typedArg.Value = value;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Tries to get the value held by the argument at the specified index
        /// </summary>
        /// <typeparam name="TArg"></typeparam>
        /// <param name="index"></param>
        /// <param name="arg"></param>
        /// <returns>Whether or not the argument was retrieved</returns>

        public bool TryGetArg<TArg>(int index, out NetworkArg<TArg> arg)
        {
            INetworkArg result = Get(index);
            arg = null;

            if (result is NetworkArg<TArg> typedResult)
            {
                arg = typedResult;
                return true;
            }

            return false;
        }

        INetworkArg INetworkArgs.Get(int index) =>
            Get(index);

        public object GetBoxedValue(int index) =>
            Get(index)?.BoxedValue ?? default;

        public bool Equals(NetworkArgs other)
        {
            if (other is null)
                return false;

            if (other.Count != Count)
                return false;

            for (int i = 0; i < Count; i++)
                if (!_args[i].Equals(other._args[i]))
                    return false;

            return true;
        }

        public bool SignatureEquals(NetworkArgs other)
        {
            if (other is null)
                return false;

            if (other.Count != Count)
                return false;

            for (int i = 0; i < Count; i++)
                if (!_args[i].SignatureEquals(other._args[i]))
                    return false;

            return true;
        }

        public abstract NetworkArgs Clone();

        public override string ToString() =>
            $"NetworkArgs[{Count}]: " + string.Join(", ", 
                this.Select(arg => arg.BoxedValue?.ToString() ?? "null"));

        public IEnumerator<INetworkArg> GetEnumerator()
            => ((IEnumerable<INetworkArg>)_args).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }

    public class NetworkArgs<TArg1> : NetworkArgs
    {

        public NetworkArgs() : this(true) { }

        private NetworkArgs(bool initialize) : base(1)
        {
            if (!initialize) return;
            this[0] = new NetworkArg<TArg1>();
        }

        public NetworkArgs(TArg1 arg) : base(1)
        {
            this[0] = new NetworkArg<TArg1>(arg);
        }

        public override NetworkArgs Clone()
        {
            NetworkArgs<TArg1> copy = new(initialize: false);

            for (int i = 0; i < Count; i++)
                copy[i] = this[i].Clone();

            return copy;
        }

    }

    public class NetworkArgs<TArg1, TArg2> : NetworkArgs
    {

        public NetworkArgs() : this(true) { }

        private NetworkArgs(bool initialize) : base(2)
        {
            if (!initialize) return;
            this[0] = new NetworkArg<TArg1>();
            this[1] = new NetworkArg<TArg2>();
        }

        public NetworkArgs(TArg1 value1, TArg2 value2) : base(2)
        {
            this[0] = new NetworkArg<TArg1>(value1);
            this[1] = new NetworkArg<TArg2>(value2);
        }

        public override NetworkArgs Clone()
        {
            NetworkArgs<TArg1, TArg2> copy = new(initialize: false);

            for (int i = 0; i < Count; i++)
                copy[i] = this[i].Clone();

            return copy;
        }

    }

    public class NetworkArgs<TArg1, TArg2, TArg3> : NetworkArgs
    {

        public NetworkArgs() : this(true) { }

        private NetworkArgs(bool initialize) : base(3)
        {
            if (!initialize) return;
            this[0] = new NetworkArg<TArg1>();
            this[1] = new NetworkArg<TArg2>();
            this[2] = new NetworkArg<TArg3>();
        }

        public NetworkArgs(TArg1 value1, TArg2 value2, TArg3 value3) : base(3)
        {
            this[0] = new NetworkArg<TArg1>(value1);
            this[1] = new NetworkArg<TArg2>(value2);
            this[2] = new NetworkArg<TArg3>(value3);
        }

        public override NetworkArgs Clone()
        {
            NetworkArgs<TArg1, TArg2, TArg3> copy = new(initialize: false);

            for (int i = 0; i < Count; i++)
                copy[i] = this[i].Clone();

            return copy;
        }
    }

    public class NetworkArgs<TArg1, TArg2, TArg3, TArg4> : NetworkArgs
    {

        public NetworkArgs() : this(true) { }

        private NetworkArgs(bool initialize) : base(4)
        {
            if (!initialize) return;
            this[0] = new NetworkArg<TArg1>();
            this[1] = new NetworkArg<TArg2>();
            this[2] = new NetworkArg<TArg3>();
            this[3] = new NetworkArg<TArg4>();
        }

        public NetworkArgs(TArg1 value1, TArg2 value2, TArg3 value3, TArg4 value4) : base(4)
        {
            this[0] = new NetworkArg<TArg1>(value1);
            this[1] = new NetworkArg<TArg2>(value2);
            this[2] = new NetworkArg<TArg3>(value3);
            this[3] = new NetworkArg<TArg4>(value4);
        }

        public override NetworkArgs Clone()
        {
            NetworkArgs<TArg1, TArg2, TArg3, TArg4> copy = new(initialize: false);

            for (int i = 0; i < Count; i++)
                copy[i] = this[i].Clone();

            return copy;
        }
    }

    public class NetworkArgs<TArg1, TArg2, TArg3, TArg4, TArg5> : NetworkArgs
    {

        public NetworkArgs() : this(true) { }

        private NetworkArgs(bool initialize) : base(5)
        {
            if (!initialize) return;
            this[0] = new NetworkArg<TArg1>();
            this[1] = new NetworkArg<TArg2>();
            this[2] = new NetworkArg<TArg3>();
            this[3] = new NetworkArg<TArg4>();
            this[4] = new NetworkArg<TArg5>();
        }

        public NetworkArgs(TArg1 value1, TArg2 value2, TArg3 value3, TArg4 value4,
            TArg5 value5) : base(5)
        {
            this[0] = new NetworkArg<TArg1>(value1);
            this[1] = new NetworkArg<TArg2>(value2);
            this[2] = new NetworkArg<TArg3>(value3);
            this[3] = new NetworkArg<TArg4>(value4);
            this[4] = new NetworkArg<TArg5>(value5);
        }

        public override NetworkArgs Clone()
        {
            NetworkArgs<TArg1, TArg2, TArg3, TArg4, TArg5> copy = new(initialize: false);

            for (int i = 0; i < Count; i++)
                copy[i] = this[i].Clone();

            return copy;
        }
    }

    public class NetworkArgs<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6> : NetworkArgs
    {
        public NetworkArgs() : this(true) { }

        private NetworkArgs(bool initialize) : base(6)
        {
            if (!initialize) return;
            this[0] = new NetworkArg<TArg1>();
            this[1] = new NetworkArg<TArg2>();
            this[2] = new NetworkArg<TArg3>();
            this[3] = new NetworkArg<TArg4>();
            this[4] = new NetworkArg<TArg5>();
            this[5] = new NetworkArg<TArg6>();
        }

        public NetworkArgs(TArg1 value1, TArg2 value2, TArg3 value3, TArg4 value4, 
            TArg5 value5, TArg6 value6) : base(6)
        {
            this[0] = new NetworkArg<TArg1>(value1);
            this[1] = new NetworkArg<TArg2>(value2);
            this[2] = new NetworkArg<TArg3>(value3);
            this[3] = new NetworkArg<TArg4>(value4);
            this[4] = new NetworkArg<TArg5>(value5);
            this[5] = new NetworkArg<TArg6>(value6);
        }

        public override NetworkArgs Clone()
        {
            NetworkArgs<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6> copy = new(initialize: false);

            for (int i = 0; i < Count; i++)
                copy[i] = this[i].Clone();

            return copy;
        }
    }

    public class NetworkArgs<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7> : NetworkArgs
    {

        public NetworkArgs() : this(true) { }

        private NetworkArgs(bool initialize) : base(7)
        {
            if (!initialize) return;
            this[0] = new NetworkArg<TArg1>();
            this[1] = new NetworkArg<TArg2>();
            this[2] = new NetworkArg<TArg3>();
            this[3] = new NetworkArg<TArg4>();
            this[4] = new NetworkArg<TArg5>();
            this[5] = new NetworkArg<TArg6>();
            this[6] = new NetworkArg<TArg7>();
        }

        public NetworkArgs(TArg1 value1, TArg2 value2, TArg3 value3, TArg4 value4,
            TArg5 value5, TArg6 value6, TArg7 value7) : base(7)
        {
            this[0] = new NetworkArg<TArg1>(value1);
            this[1] = new NetworkArg<TArg2>(value2);
            this[2] = new NetworkArg<TArg3>(value3);
            this[3] = new NetworkArg<TArg4>(value4);
            this[4] = new NetworkArg<TArg5>(value5);
            this[5] = new NetworkArg<TArg6>(value6);
            this[6] = new NetworkArg<TArg7>(value7);
        }

        public override NetworkArgs Clone()
        {
            NetworkArgs<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7> copy = new(initialize: false);

            for (int i = 0; i < Count; i++)
                copy[i] = this[i].Clone();

            return copy;
        }
    }

}

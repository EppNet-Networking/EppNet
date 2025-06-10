///////////////////////////////////////////////////////
/// Filename: Timestamp.cs
/// Date: February 16, 2025
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Node;
using EppNet.Utilities;

using System;
using System.Diagnostics.CodeAnalysis;

namespace EppNet.Data
{

    /// <summary>
    /// Essentially a tuple grouping the network time shown and the local steady time<br/>
    /// Comparisons are done using the local time to keep timestamps unaffected by clock syncs
    /// </summary>
    public readonly struct Timestamp : IComparable, IComparable<Timestamp>, IEquatable<Timestamp>
    {

        public static readonly Timestamp Zero = new(TimeSpan.Zero, TimeSpan.Zero);

        /// <summary>
        /// The time displayed on the network clock
        /// </summary>
        public TimeSpan NetworkTime { get; }

        public TimeSpan LocalTime { get; }

        public Timestamp(TimeSpan netTime)
        {
            this.NetworkTime = netTime;
            this.LocalTime = TimeSpan.FromMilliseconds(ENet.Library.Time);
        }

        public Timestamp([NotNull] NetworkNode node)
        {
            Guard.AgainstNull(node);
            this.NetworkTime = node.Time;
            this.LocalTime = TimeSpan.FromMilliseconds(ENet.Library.Time);
        }

        public Timestamp([NotNull] INodeDescendant nodeDescendant)
        {
            Guard.AgainstNull(nodeDescendant);
            Guard.AgainstNull(nodeDescendant.Node);
            this.NetworkTime = nodeDescendant.Node.Time;
            this.LocalTime = TimeSpan.FromMilliseconds(ENet.Library.Time);
        }

        public Timestamp(TimeSpan netTime, TimeSpan monoTime)
        {
            this.NetworkTime = netTime;
            this.LocalTime = monoTime;
        }

        public int CompareTo(object obj)
            => obj is Timestamp ts ?
            CompareTo(ts) :
            0;

        public int CompareTo(Timestamp other)
            => other.LocalTime.CompareTo(LocalTime);

        public override bool Equals(object obj)
            => obj is Timestamp ts &&
            Equals(ts);

        public bool Equals(Timestamp other)
            => other.NetworkTime.Equals(NetworkTime) &&
            other.LocalTime.Equals(LocalTime);

        public override int GetHashCode()
            => NetworkTime.GetHashCode() ^ LocalTime.GetHashCode();

        public override string ToString()
            => $"Network(ticks): {NetworkTime.Ticks}\nLocal(ticks): {LocalTime.Ticks}";

        public static bool operator ==(Timestamp left, Timestamp right)
            => left.Equals(right);

        public static bool operator !=(Timestamp left, Timestamp right)
            => !left.Equals(right);

        public static bool operator >(Timestamp left, Timestamp right)
            => left.LocalTime > right.LocalTime;

        public static bool operator <(Timestamp left, Timestamp right)
            => left.LocalTime < right.LocalTime;

        public static bool operator >=(Timestamp left, Timestamp right)
            => left.LocalTime >= right.LocalTime;

        public static bool operator <=(Timestamp left, Timestamp right)
            => left.LocalTime <= right.LocalTime;
    }

}

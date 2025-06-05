/////////////////////////////////////////////
/// Filename: Channel.cs
/// Date: July 11, 2024
/// Author: Maverick Liberty
//////////////////////////////////////////////

using ENet;

using EppNet.Data.Datagrams;
using EppNet.Logging;
using EppNet.Utilities;

using System;
using System.Collections.Generic;
using System.Threading;

namespace EppNet.Messaging
{

    /// <summary>
    /// This enum represents E++Net reserved channels
    /// </summary>

    public enum Channels : byte
    {

        /// <summary>
        /// Handles messages related to connectivity and synchronization<br/>
        /// Examples: Ping datagrams, joins, exits
        /// </summary>
        Connectivity = 0x0,

        /// <summary>
        /// Handles messages that are guaranteed to arrive
        /// </summary>
        Reliable     = 0x1,

        /// <summary>
        /// Handles messages that aren't guaranteed to arrive<br/>
        /// Example: Snapshot messages
        /// </summary>
        Unreliable   = 0x2
    }

    public class Channel : ILoggable, IDisposable, IEquatable<Channel>
    {

        public readonly ChannelService Service;

        public Action<IDatagram> DatagramReceived;

        public ILoggable Notify { get => this; }

        public readonly byte Id;
        public ChannelFlags Flags { protected set; get; }

        // Statistics
        public int DatagramsReceived { get => _datagramsReceived; }
        public int DatagramsSent { get => _datagramsSent; }

        public long TotalBytesReceived { get => _totalBytesReceived; }

        public long TotalBytesSent { get => _totalBytesSent; }


        private readonly Queue<IDatagram> _buffer;
        private readonly object _bufferLock;

        protected int _datagramsReceived;
        protected int _datagramsSent;
        protected long _totalBytesReceived;
        protected long _totalBytesSent;

        public Channel(ChannelService service, byte id)
        {
            this.Service = service;
            this.Id = id;
            this.Flags = ChannelFlags.None;
            this._buffer = (Flags & ChannelFlags.ProcessImmediately) == 0 ? new() : null;
            this._bufferLock = _buffer != null ? new() : null;
        }

        public Channel(ChannelService service, byte id, ChannelFlags flags) : this(service, id)
        {
            this.Flags = flags;
        }

        public void ReceiveOrQueue(IDatagram datagram)
        {
            if (datagram == null)
            {
                // We received a null datagram! This shouldn't happen. Let the node know about it.
                Service.Node.HandleException(new ArgumentNullException($"Channel ID {Id} received a null datagram!"));
                return;
            }

            if (Flags.HasFlag(ChannelFlags.ProcessImmediately))
            {
                // Let's process this datagram immediately
                Receive(datagram);
            }
            else if (datagram.Collectible)
            {
                // Let's enqueue this datagram for reception later on.
                lock (_bufferLock)
                    _buffer.Enqueue(datagram);
            }
        }

        /// <summary>
        /// Receives the specified <see cref="IDatagram"/> <br/>
        /// NOTE: Dispose() is called at the end of this method!
        /// </summary>
        /// <param name="datagram"></param>

        public void Receive(IDatagram datagram)
        {
            Interlocked.Increment(ref _datagramsReceived);
            Interlocked.Add(ref _totalBytesReceived, datagram.Size);
            DatagramReceived?.GlobalInvoke(datagram);

            // All done
            datagram.Dispose();
        }

        /// <summary>
        /// Clears out the buffer (queue) and receives every datagram.<br/>
        /// See <see cref="Receive(IDatagram)"/> for more information.
        /// </summary>

        public void ReceiveQueue()
        {

            IDatagram[] datagrams = new IDatagram[_buffer.Count];

            lock (_bufferLock)
            {
                _buffer.CopyTo(datagrams, 0);
                _buffer.Clear();
            }

            for (int i = 0; i < datagrams.Length; i++)
                Receive(datagrams[i]);
        }

        /// <summary>
        /// Clears the buffered (queued) datagrams.
        /// </summary>

        public void Clear()
        {
            if (_buffer != null)
            {
                int datagramsDropped = 0;
                // Locks the buffer and clears it.
                lock (_bufferLock)
                {
                    // Since each datagram won't be handled as expected, let's dispose of them properly.
                    datagramsDropped = _buffer.Count;

                    // Let's dispose of each datagram
                    foreach (IDatagram datagram in _buffer)
                        datagram.Dispose();

                    _buffer.Clear();
                }

                if (datagramsDropped > 0)
                    this.Warning($"Cleared buffer of {datagramsDropped} Datagrams.");
            }
        }

        /// <summary>
        /// Tries to send an <see cref="ArraySegment"/> of bytes to the specified <see cref="Peer"/> 
        /// with the specified <see cref="PacketFlags"/>.<br/>
        /// Length of -1 means the entire buffer will be sent.<br/><br/>
        /// 
        /// Return:<br/>
        /// - 0 -> Success<br/>
        /// - 1 -> Invalid data<br/>
        /// - 2 -> An exception occurred<br/>
        /// </summary>
        /// <param name="peer"></param>
        /// <param name="buffer"></param>
        /// <param name="flags"></param>
        /// <returns></returns>

        public int TrySendTo(Peer peer, ArraySegment<byte> buffer, PacketFlags flags = PacketFlags.None)
        {
            if (buffer.Count < 1)
                return 1;

            return TrySendTo(peer, buffer.Array, buffer.Offset, buffer.Count, flags);
        }

        /// <summary>
        /// Tries to send a byte array to the specified <see cref="Peer"/> 
        /// with the specified <see cref="PacketFlags"/>.<br/>
        /// Length of -1 means the entire buffer will be sent.<br/><br/>
        /// 
        /// Return:<br/>
        /// - 0 -> Success<br/>
        /// - 1 -> Invalid data<br/>
        /// - 2 -> An exception occurred<br/>
        /// </summary>
        /// <param name="peer"></param>
        /// <param name="buffer"></param>
        /// <param name="flags"></param>
        /// <returns></returns>

        public int TrySendTo(Peer peer, byte[] bytes, int offset, int length = -1, PacketFlags flags = PacketFlags.None)
        {
            if (bytes == null)
                return 1;

            Packet packet = default;

            try
            {
                unsafe
                {
                    fixed (byte* bufferPtr = bytes)
                    {
                        IntPtr nativePtr = (IntPtr)(bufferPtr + offset);
                        packet.Create(nativePtr, length == -1 ? bytes.Length : length, flags);
                    }
                }

                // Send the packet to our ENet peer
                if (peer.Send(Id, ref packet))
                {
                    Interlocked.Increment(ref _datagramsReceived);
                    Interlocked.Add(ref _totalBytesSent, length);
                    return 0;
                }
            }
            catch (Exception ex)
            {
                Notify.Error($"Failed to send data to Peer {peer.ID}. Error {ex.Message}\n{ex.StackTrace}");
            }
            finally
            {
                // No matter what, dispose of this packet.
                packet.Dispose();
            }

            // An exception occurred.
            return 2;
        }

        /// <summary>
        /// Checks if a flag is enabled
        /// </summary>
        /// <param name="flag"></param>
        /// <returns></returns>
        public bool HasFlag(ChannelFlags flag) =>
            Flags.HasFlag(flag);

        /// <summary>
        /// Atomically resets all channel statistics back to 0.
        /// </summary>

        public void ResetStatistics()
        {
            Interlocked.Exchange(ref _datagramsReceived, 0);
            Interlocked.Exchange(ref _datagramsSent, 0);
            Interlocked.Exchange(ref _totalBytesReceived, 0);
            Interlocked.Exchange(ref _totalBytesSent, 0);
        }

        public void Dispose()
        {
            Clear();
            ResetStatistics();

            DatagramReceived = null;
        }

        public bool Equals(Channel other) =>
            other.Id == Id;

        public override bool Equals(object obj)
        {
            if (obj is Channels channelsEnum)
                return Id == (byte)channelsEnum;

            if (obj is Channel other)
                return Equals(other);

            return false;
        }

        public override int GetHashCode() =>
            Id;

        public static bool operator ==(Channel left, Channel right) =>
            left?.Equals(right) ?? false;

        public static bool operator !=(Channel left, Channel right) =>
            !left?.Equals(right) ?? true;

        public static explicit operator byte(Channel channel) =>
            channel.Id;
    }

}

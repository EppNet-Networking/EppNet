/////////////////////////////////////////////
/// Filename: ChannelService.cs
/// Date: July 11, 2024
/// Author: Maverick Liberty
//////////////////////////////////////////////

using ENet;

using EppNet.Data.Datagrams;
using EppNet.Logging;
using EppNet.Processes;
using EppNet.Processes.Events;
using EppNet.Services;
using EppNet.Utilities;

using System;
using System.Collections.Concurrent;

namespace EppNet.Messaging
{

    public class ChannelService : Service, IBufferEventHandler<PacketReceivedEvent>
    {

        protected readonly ConcurrentDictionary<byte, Channel> _channels;

        public ChannelService(ServiceManager svcMgr) : base(svcMgr)
        {
            this._channels = new();
        }

        public bool Handle(PacketReceivedEvent @event)
        {
            // Let's try to fetch the channel by id
            Channel channel = GetChannelById(@event.ChannelID);
            channel?.ReceiveOrQueue(@event.Datagram);

            return true;
        }

        /// <summary>
        /// Tries to send a Datagram to the specified <see cref="Peer"/> 
        /// with the specified <see cref="PacketFlags"/>.<br/><br/>
        /// 
        /// Return:<br/>
        /// - 0 -> Success<br/>
        /// - 1 -> Invalid data<br/>
        /// - 2 -> An exception occurred<br/>
        /// - 3 -> Channel not found
        /// </summary>
        /// <param name="peer"></param>
        /// <param name="buffer"></param>
        /// <param name="flags"></param>
        /// <returns></returns>

        public int TrySendTo(Peer peer, Datagram datagram, PacketFlags flags = PacketFlags.None)
        {

            if (datagram is null)
                return 1;

            if (!datagram.Written)
                datagram.Write();

            if (!datagram.Stream.TryGetBuffer(out ArraySegment<byte> buffer))
                return 1;

            return TrySendDataTo(peer, datagram.ChannelID, buffer.Array, buffer.Offset, buffer.Count, flags);
        }

        /// <summary>
        /// Tries to send a byte array to the specified <see cref="Peer"/> 
        /// with the specified offset, length, and <see cref="PacketFlags"/>.<br/>
        /// Length of -1 means the entire buffer will be sent.<br/><br/>
        /// 
        /// Return:<br/>
        /// - 0 -> Success<br/>
        /// - 1 -> Invalid data<br/>
        /// - 2 -> An exception occurred<br/>
        /// - 3 -> Channel does not exist
        /// </summary>
        /// <param name="peer"></param>
        /// <param name="buffer"></param>
        /// <param name="flags"></param>
        /// <returns></returns>

        public int TrySendDataTo(Peer peer, byte channelId, byte[] bytes, int offset, int length = -1, PacketFlags flags = PacketFlags.None)
        {
            Channel channel = GetChannelById(channelId);

            if (channel is not null)
                return channel.TrySendTo(peer, bytes, offset, length, flags);

            return 3;
        }

        public bool TryAddChannel(byte id)
            => TryAddChannel(id, out Channel _);

        public bool TryAddChannel(byte id, out Channel newChannel, ChannelFlags flags = ChannelFlags.None)
        {
            newChannel = null;
            
            if (id < (byte) Enum.GetValues(typeof(Channels)).Length)
            {
                // This is a reserved channel id
                string message = $"Channel ID \"{id}\" is reserved!";
                ChannelAlreadyExistsException exp = new(id, message);

                Notify.Error(new TemplatedMessage(message), exp);
                _serviceMgr.Node.HandleException(exp);
                return false;
            }

            return _Internal_TryAddChannel(id, out newChannel, flags);
        }

        /// <summary>
        /// Tries to obtain a <see cref="EppNet.Messaging.Channel"/> by its ID.
        /// </summary>
        /// <returns>A valid <see cref="Channel"/> instance or NULL</returns>

        public Channel GetChannelById(byte id)
        {
            _channels.TryGetValue(id, out Channel channel);

            if (channel == null)
                Notify.Debug(new TemplatedMessage("Failed to obtain Channel \"{channelId}\"!", id));

            return channel;
        }

        public override bool Start()
        {

            bool started = base.Start();

            if (started)
            {
                // Let's add our channels
                _Internal_TryAddChannel((byte)Channels.Connectivity, out var _, ChannelFlags.ProcessImmediately);
                _Internal_TryAddChannel((byte)Channels.Reliable, out var _);
                _Internal_TryAddChannel((byte)Channels.Unreliable, out var _);
            }

            return started;
        }

        public override bool Stop()
        {
            bool stopped = base.Stop();

            if (stopped)
            {
                // Reset every channel
                foreach (var channel in _channels.Values)
                {
                    channel.Clear();
                    channel.ResetStatistics();
                }
            }

            return stopped;
        }

        public override void Dispose(bool disposing)
        {
            // Dispose every channel
            foreach (var channel in _channels.Values)
                channel.Dispose();
        }

        private bool _Internal_TryAddChannel(byte id, out Channel newChannel, ChannelFlags flags = ChannelFlags.None)
        {
            newChannel = null;

            if (_channels.ContainsKey(id))
            {
                // The channel already exists
                string message = $"Channel \"{id}\" already exists!";
                ChannelAlreadyExistsException exp = new(id, message);

                Notify.Error(new TemplatedMessage(message), exp);
                _serviceMgr.Node.HandleException(exp);

                return false;
            }

            newChannel = new(this, id, flags);
            _channels[id] = newChannel;

            // Let's send out this debug message just in case.
            Notify.Debug(new TemplatedMessage("Created new Channel \"{channelId}\" with Flags {flags}", id, flags.ToListString()));

            return true;
        }

    }

}
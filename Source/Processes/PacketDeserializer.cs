/////////////////////////////////////////////
/// Filename: PacketDeserializer.cs
/// Date: August 6, 2024
/// Author: Maverick Liberty
//////////////////////////////////////////////

using ENet;

using EppNet.Logging;
using EppNet.Messaging;
using EppNet.Processes.Events;

using EppNet.Sockets;
using EppNet.Transport;

using System;
using System.Diagnostics.CodeAnalysis;

namespace EppNet.Processes
{

    public class PacketDeserializer : IBufferEventHandler<PacketReceivedEvent>, ILoggable
    {

        public int DroppedPackets { private set; get; }
        public long DroppedBytes { private set; get; }

        public ILoggable Notify { get => this; }

        protected ITransport _transport;

        protected MultithreadedBuffer<PacketReceivedEvent> _buffer;

        public PacketDeserializer([NotNull] ITransport transport, int bufferSize)
        {
            this._transport = transport ?? throw new ArgumentNullException(nameof(transport));

            MultithreadedBufferBuilder<PacketReceivedEvent> builder = new(transport.Node, bufferSize);
            this._buffer = builder.ThenUseHandlers(this).ThenUseHandlers(transport.Node.Services.GetService<ChannelService>()).Build();

            this.DroppedBytes = this.DroppedPackets = 0;
        }

        public bool Handle(PacketReceivedEvent data)
        {
            byte[] bytes = data.Data;
            byte header = bytes[0];


            return true;
        }

        public void Start() => _buffer.Start();
        public void Cancel() => _buffer.Stop();

        public void HandlePacket(Peer peer, Packet packet, byte channelID, int timeoutMs = 10)
        {
            byte[] data = new byte[packet.Length];
            packet.CopyTo(data);

            Action<PacketReceivedEvent> setupAction = (PacketReceivedEvent @event) => SetupPacket(@event, peer, data, channelID);
            _buffer.CreateAndWrite(setupAction);
        }

        public void SetupPacket(PacketReceivedEvent @event, Peer peer, byte[] data, byte channelID)
        {
            if (_socket is ServerSocket srvSocket)
                @event.Initialize(srvSocket.ConnectionService.Get(peer.ID), data, channelID);
            else
                @event.Initialize(_socket.Companion, data, channelID);
        }

        public void DropPacket(byte[] data)
        {
            DroppedBytes += data.Length;
            DroppedPackets++;
            Notify.Warning("Dropped packet!");
        }
    }


}

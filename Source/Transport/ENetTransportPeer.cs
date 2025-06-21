///////////////////////////////////////////////////////
/// Filename: ENetTransportPeer.cs
/// Date: June 21, 2025
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using System;
using ENet;

using EppNet.Connections;
using EppNet.Data;
using EppNet.Node;

namespace EppNet.Transport
{

    public class ENetTransportPeer : ITransportPeer<ENetTransportService, Peer>
    {
        public long ID { private set; get; }

        public Peer NativePeer { private set; get; }

        public ENetTransportService Transport { private set; get; }

        public Timestamp ConnectedTimestamp => throw new System.NotImplementedException();

        public bool IsSynchronized => throw new System.NotImplementedException();

        public bool IsAuthenticated => throw new System.NotImplementedException();

        public bool IsConnected => throw new System.NotImplementedException();

        public NetworkNode Node => throw new System.NotImplementedException();

        public ENetTransportPeer(ENetTransportService transport, Peer nativePeer)
        {
            this.Transport = transport ?? throw new ArgumentNullException(nameof(transport));
            this.NativePeer = nativePeer;
            this.ID = NativePeer.ID;
        }

        public void Disconnect(DisconnectReason disconnectReason)
        {
            throw new System.NotImplementedException();
        }

    }

}
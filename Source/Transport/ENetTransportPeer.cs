///////////////////////////////////////////////////////
/// Filename: ENetTransportPeer.cs
/// Date: June 21, 2025
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using System;
using System.Net;
using ENet;

using EppNet.Connections;
using EppNet.Data;
using EppNet.Node;

namespace EppNet.Transport
{

    public class ENetTransportPeer : ITransportPeer
    {
        public long ID { private set; get; }

        public IPEndPoint EndPoint { private set; get; }

        public object NativePeer { private set; get; }

        public Peer ENet_Peer { private set; get; }

        public BaseTransportService Transport { private set; get; }

        public Timestamp ConnectedTimestamp => throw new System.NotImplementedException();

        public bool IsSynchronized => throw new System.NotImplementedException();

        public bool IsAuthenticated => throw new System.NotImplementedException();

        public bool IsConnected => throw new System.NotImplementedException();

        public NetworkNode Node => throw new System.NotImplementedException();

        public ENetTransportPeer(ENetTransportService transport, Peer nativePeer)
        {
            this.Transport = (BaseTransportService) transport ?? throw new ArgumentNullException(nameof(transport));
            this.NativePeer = nativePeer;
            this.ID = nativePeer.ID;
            this.ENet_Peer = nativePeer;
            this.EndPoint = new(IPAddress.Parse(nativePeer.IP), nativePeer.Port);
        }

        public void DisconnectLater(DisconnectReason disconnectReason)
        {
            throw new System.NotImplementedException();
        }

        public void DisconnectNow(DisconnectReason reason) { }

        public void Dispose()
        {
            this.ENet_Peer.DisconnectNow(0);
        }

    }

}
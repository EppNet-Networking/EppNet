///////////////////////////////////////////////////////
/// Filename: ITransportPeer.cs
/// Date: June 10, 2025
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Data;
using EppNet.Connections;
using System;
using System.Net;

namespace EppNet.Transport
{

    public interface ITransportPeer : IDataHolder, IDisposable
    {
        /// <summary>
        /// The unique identifier for this peer
        /// </summary>
        long ID { get; }

        /// <summary>
        /// The peer's IP address + port
        /// </summary>
        IPEndPoint EndPoint { get; }

        /// <summary>
        /// The first time we made contact with this peer
        /// </summary>
        Timestamp ConnectedTimestamp { get; }

        /// <summary>
        /// The transport mechanism this peer is associated with
        /// </summary>
        BaseTransportService Transport { get; }

        /// <summary>
        /// The peer object native to the transport
        /// </summary>
        object NativePeer { get; }

        /// <summary>
        /// Whether or not this peer has synchronized to the current network time
        /// </summary>
        bool IsSynchronized { get; }

        /// <summary>
        /// Whether or not this peer has been authenticated
        /// </summary>
        bool IsAuthenticated { get; }

        /// <summary>
        /// Whether or not this peer is connected
        /// </summary>
        bool IsConnected { get; }

        void DisconnectLater(DisconnectReason disconnectReason);
        void DisconnectNow(DisconnectReason disconnectReason);

        string ToString() =>
            $"Peer {ID} \n{EndPoint.Address}:{EndPoint.Port}";
    }

}

///////////////////////////////////////////////////////
/// Filename: ITransportPeer.cs
/// Date: June 10, 2025
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Data;
using EppNet.Connections;
using EppNet.Transport;

namespace EppNet.Transport
{

    public interface ITransportPeer : IDataHolder
    {
        /// <summary>
        /// The unique identifier for this peer
        /// </summary>
        long ID { get; }

        /// <summary>
        /// The first time we made contact with this peer
        /// </summary>
        Timestamp ConnectedTimestamp { get; }

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

        void Disconnect(DisconnectReason disconnectReason);
    }

    public interface ITransportPeer<TTransport, TNativePeer> : ITransportPeer
        where TTransport : class, ITransport
    {

        /// <summary>
        /// The peer object native to the transport
        /// </summary>
        TNativePeer NativePeer { get; }

        /// <summary>
        /// The transport mechanism this peer is associated with
        /// </summary>
        TTransport Transport { get; }

    }

}

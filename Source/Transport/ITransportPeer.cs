///////////////////////////////////////////////////////
/// Filename: ITransportPeer.cs
/// Date: June 10, 2025
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Data;

public interface ITransportPeer : IDataHolder
{

    /// <summary>
    /// The unique identifier for this peer
    /// </summary>
    long Id { get; }

    /// <summary>
    /// The peer object native to the transport
    /// </summary>
    object NativePeer { get; }

    /// <summary>
    /// The transport mechanism this peer is associated with
    /// </summary>
    ITransport Transport { get; }

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

}

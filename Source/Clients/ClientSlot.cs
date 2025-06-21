///////////////////////////////////////////////////////
/// Filename: ClientSlot.cs
/// Date: June 25, 2025
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Collections;
using EppNet.Transport;

namespace EppNet.Clients
{

    public struct ClientSlot<T> : IPageable
        where T : class, ITransportPeer
    {
        public IPage Page { set; get; }
        public long ID { set; get; }
        public bool Allocated { set; get; }

        public T Client { set; get; }

        public void Dispose()
        {
            Client = null;
        }

    }

}
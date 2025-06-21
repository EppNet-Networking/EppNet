///////////////////////////////////////////////////////
/// Filename: ITransport.cs
/// Date: June 10, 2025
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Data;
using EppNet.Node;
using EppNet.Services;

namespace EppNet.Transport
{

    public interface ITransport : INodeDescendant, IService
    {

        Timestamp CreateTimestamp { get; }

    }

}

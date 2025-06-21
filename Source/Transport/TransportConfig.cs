///////////////////////////////////////////////////////
/// Filename: TransportConfig.cs
/// Date: June 20, 2025
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

namespace EppNet.Transport
{

    public sealed class TransportConfig
    {

        public const string LocalIP = "127.0.0.1";
        public const string LocalHost = "localhost";

        public const ushort DefaultPort = 7777;
        public const int DefaultClients = 64;
        
        public static TransportConfig Default = new();

        public string IP { set; get; } = LocalIP;

        public ushort Port { set; get; } = DefaultPort;

        /// <summary>
        /// The name of the host<br/>
        /// <strong>Client only</strong>
        /// </summary>
        public string HostName { set; get; } = LocalHost;

        /// <summary>
        /// The maximum number of clients<br/>
        /// <strong>Server only</strong>
        /// </summary>
        public int MaxClients { set; get; } = DefaultClients;

    }

}
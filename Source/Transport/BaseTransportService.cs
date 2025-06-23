///////////////////////////////////////////////////////
/// Filename: BaseTransportService.cs
/// Date: June 20, 2025
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Clients;
using EppNet.Collections;
using EppNet.Commands;
using EppNet.Connections;
using EppNet.Data;
using EppNet.Logging;
using EppNet.Processes;
using EppNet.Services;
using EppNet.Time;

using System.Diagnostics.CodeAnalysis;


namespace EppNet.Transport
{

    public abstract class BaseTransportService : Service, ITransport
    {

        public TransportConfig Config { get; }

        public Timestamp CreateTimestamp { private set; get; }
        public Timestamp LastPollTimestamp { protected set; get; }

        public int TimeoutMs { protected set; get; }

        public IClock Clock { protected set; get; }

        public bool IsServer =>
            Node.Distro == Distribution.Server;

        protected PacketDeserializer _packetDeserializer;

        protected PageList<ClientSlot<ITransportPeer>> _clients;
        protected ITransportPeer _serverPeer;

        protected BaseTransportService([NotNull] ServiceManager svcMgr, TransportConfig config = default, int sortOrder = 0)
            : base(svcMgr, sortOrder)
        {
            this.Config = config ?? TransportConfig.Default;
            this.CreateTimestamp = default;
            this.Clock = null;
            this._packetDeserializer = null;
            this._clients = null;
            this._serverPeer = null;
        }

        /// <summary>
        /// Function called when a new peer connects
        /// </summary>
        /// <param name="newPeer"></param>
        /// <returns></returns>
        public virtual bool OnPeerConnected(ITransportPeer newPeer)
        {
            return false;
        }

        protected virtual EnumCommandResult ValidateConfig()
        {
            Config.IP = string.IsNullOrEmpty(Config.IP)
                ? TransportConfig.LocalIP
                : Config.IP;

            Config.HostName = string.IsNullOrEmpty(Config.HostName)
                ? TransportConfig.LocalHost
                : Config.HostName;

            // This is a wraparound clamp for max clients
            // If not in range 1 <= n <= TransportMaxClients(),
            // the value is set to the maximum allowed clients
            int transportMaxClients = TransportMaxClients();
            Config.MaxClients = Config.MaxClients > 0 &&
                Config.MaxClients <= transportMaxClients
                ? Config.MaxClients
                : transportMaxClients;

            if (Config.Port < 1024)
            {
                Notify.Fatal("Port cannot be less than 1024!");
                return EnumCommandResult.BadArgument;
            }

            return EnumCommandResult.Ok;
        }

        protected virtual EnumCommandResult Setup()
        {
            if (Started)
                return EnumCommandResult.InvalidState;

            return EnumCommandResult.Ok;
        }

        public override bool Start()
        {
            if (!Started)
            {
                if (Node.Distro == Distribution.Server)
                {
                    _clients = new(PageList<ClientSlot<ITransportPeer>>.CalculateItemsPerPage(Config.MaxClients));
                    _clients.OnFree += obj => obj.Client.DisconnectNow(DisconnectReasons.Ejected);
                }

                // Let's begin the clock!
                Clock.Start();

                // TODO: Add means to adjust the buffer size
                this._packetDeserializer = new(this, 256);
                _packetDeserializer.Start();

                this.Status = ServiceState.Online;
                this.Started = true;
                return true;
            }

            return false;
        }

        public override bool Stop()
        {
            if (!Started || Status != ServiceState.Online)
                return false;

            Clock.Stop();
            _packetDeserializer.Cancel();
            _clients?.Clear();

            this.Status = ServiceState.Offline;
            return true;
        }

        public abstract int TransportMaxClients();

    }

}

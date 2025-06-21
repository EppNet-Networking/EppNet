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

using System.Diagnostics.CodeAnalysis;


namespace EppNet.Transport
{

    public abstract class BaseTransportService<TTransport, TNativePeer, TPeer> : Service, ITransport
        where TPeer : class, ITransportPeer
        where TTransport : class, ITransport
    {

        public TransportConfig Config { get; }

        public Timestamp CreateTimestamp { private set; get; }

        protected PacketDeserializer _packetDeserializer;

        protected PageList<ClientSlot<TPeer>> _clients;
        protected TPeer _serverPeer;

        protected BaseTransportService([NotNull] ServiceManager svcMgr, TransportConfig config = default, int sortOrder = 0)
            : base(svcMgr, sortOrder)
        {
            this.Config = config ?? TransportConfig.Default;
            this.CreateTimestamp = default;
            this._packetDeserializer = null;
            this._clients = null;
            this._serverPeer = null;
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
                    _clients = new(PageList<ClientSlot<TPeer>>.CalculateItemsPerPage(Config.MaxClients));
                    _clients.OnFree += obj => obj.Client.DisconnectNow(DisconnectReasons.Ejected);
                }

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

            _packetDeserializer.Cancel();
            _clients?.Clear();
            this.Status = ServiceState.Offline;
            return true;
        }

        public abstract int TransportMaxClients();

    }

}
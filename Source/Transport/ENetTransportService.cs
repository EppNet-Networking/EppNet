///////////////////////////////////////////////////////
/// Filename: ENetTransportService.cs
/// Date: June 20, 2025
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using ENet;

using EppNet.Commands;
using EppNet.Logging;
using EppNet.Services;
using System;

using System.Diagnostics.CodeAnalysis;

namespace EppNet.Transport
{

    public sealed class ENetTransportService : BaseTransportService<ENetTransportService, Peer, ENetTransportPeer>
    {

        private Host _enet_host;
        private Address _enet_addr;
        private Event _enet_event;

        public ENetTransportService([NotNull] ServiceManager svcMgr, TransportConfig config = default, int sortOrder = 0)
            : base(svcMgr, config, sortOrder)
        {
            this._enet_host = null;
            this._enet_addr = default;
            this._enet_event = default;
        }

        public override bool Start()
        {
            if (!Started && Status == ServiceState.Offline && ValidateConfig().IsOk())
            {
                Status = ServiceState.Starting;
                Library.Initialize();

                // Let's get our setup going for CSharp-ENet
                this._enet_addr = new()
                {
                    Port = Config.Port
                };

                this._enet_addr.SetHost(Config.HostName);
                this._enet_addr.SetIP(Config.IP);

                _enet_host = new();
                if (Node.Distro == Distribution.Server)
                {
                    _enet_host.Create(_enet_addr, Config.MaxClients);
                    Notify.Info($"Starting listening on {Config.IP}:{Config.Port}... Peer limit: {Config.MaxClients}");
                }
                else if (Node.Distro == Distribution.Client)
                {
                    _enet_host.Create();
                    _serverPeer = new(this, _enet_host.Connect(_enet_addr));
                    Notify.Info($"Trying to connect to {Config.IP}:{Config.Port}...");
                }
                else
                    throw new ArgumentException("Invalid Distribution type!");
                
                return base.Start();
            }

            return false;
        }

        public override bool Stop()
        {
            if (base.Stop())
            {
                Library.Deinitialize();
                return true;
            }

            return false;
        }

        public override int TransportMaxClients() =>
            (int)Library.maxPeers;

    }

}
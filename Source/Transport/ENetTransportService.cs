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

    public sealed class ENetTransportService : BaseTransportService
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
                    ServerPeer = new ENetTransportPeer(this, _enet_host.Connect(_enet_addr));
                    Notify.Info($"Trying to connect to {Config.IP}:{Config.Port}...");
                }
                else
                    throw new ArgumentException("Invalid Distribution type!");
                
                return base.Start();
            }

            return false;
        }

        public override bool Tick(float dt)
        {
            if (!Started)
                return false;

            bool polled = false;
            while (!polled)
            {
                if (_enet_host.CheckEvents(out _enet_event) <= 0)
                {
                    if (_enet_host.Service(TimeoutMs, out _enet_event) <= 0)
                        break;

                    polled = true;
                    LastPollTimestamp = new(this);
                }
            }

            switch (_enet_event.Type)
            {

                case EventType.Connect:
                    break;

                case EventType.Disconnect:
                    break;

                case EventType.Timeout:
                    break;

                case EventType.Receive:
                    break;

            }


            return true;
        }


        public override bool Stop()
        {
            if (!Started)
                return false;

            Library.Deinitialize();

            _enet_host.Flush();
            _enet_host.Dispose();

            return base.Stop();
        }

        public override int TransportMaxClients() =>
            (int)Library.maxPeers;

    }

}
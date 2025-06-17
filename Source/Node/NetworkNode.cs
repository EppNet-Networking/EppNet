///////////////////////////////////////////////////////
/// Filename: NetworkNode.cs
/// Date: July 9, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Data;
using EppNet.Exceptions;
using EppNet.IO;
using EppNet.Logging;
using EppNet.Services;
using EppNet.Sockets;

using Serilog;

using System;
using System.Runtime.CompilerServices;

namespace EppNet.Node
{

    /// <summary>
    /// NetworkNodes contain everything necessary for an instance of E++Net to function.
    /// 
    /// </summary>

    public class NetworkNode : ILoggable, IEquatable<NetworkNode>, INameable
    {

        public ILoggable Notify { get => this; }

        public string Name { set; get; }
        public readonly Guid UUID;
        public readonly Distribution Distro;

        public ExceptionStrategy ExceptionStrategy;
        public bool Started { private set; get; }

        /// <summary>
        /// Fetches the <see cref="ServiceManager"/> associated with this node
        /// </summary>
        public ServiceManager Services { get => _serviceMgr; }

        /// <summary>
        /// The <see cref="BaseSocket"/> associated with this Node.<br/>
        /// If you try to get the Socket without creating one first, it will<br/>
        /// generate a default node based on the <see cref="Distribution"/> type.<br/>
        /// - <see cref="Distribution.Client"/> => <see cref="ClientSocket"/><br/>
        /// - <see cref="Distribution.Server"/> => <see cref="ServerSocket"/>
        /// </summary>
        public BaseSocket Socket
        {
            get
            {
                if (_socket == null)
                {
                    switch (Distro)
                    {
                        case Distribution.Client:
                            new ClientSocket(this);
                            break;

                        case Distribution.Server:
                            new ServerSocket(this);
                            break;
                    }

                }

                return _socket;
            }

        }

        /// <summary>
        /// Fetches the Network Time as shown on the Network Clock<br/>
        /// Returns 0 if the socket is not running
        /// </summary>

        public TimeSpan Time
        {

            get
            {
                if (Socket != null && Socket.Clock != null)
                    return Socket.Clock.Time;

                return TimeSpan.Zero;
            }

        }

        /// <summary>
        /// A structure with the current network and monotonic times
        /// </summary>

        public Timestamp Timestamp
        {
            get
            {
                TimeSpan netTime = Time;
                return new(netTime);
            }
        }

        public NodeDataStorage DataStorage { get; }

        internal ServiceManager _serviceMgr;
        internal BaseSocket _socket;

        internal readonly int _index;

        private RuntimeFileMetadata _logMetadata;

        internal NetworkNode(string name, Distribution distro, ExceptionStrategy exceptStrat, BaseSocket socket, ServiceManager serviceManager)
        {

            if (distro == Distribution.Shared)
                throw new InvalidOperationException("Invalid Distribution type! Must be either Server or Client!");

            this.Name = name;
            this.UUID = Guid.NewGuid();
            this.Distro = distro;
            this.ExceptionStrategy = exceptStrat;
            this.Started = false;

            this._serviceMgr = serviceManager ?? new(this);
            this._socket = socket;
            this._logMetadata = null;

            Serilog.Debugging.SelfLog.Enable(Console.Error);
            Log.Logger = new LoggerConfiguration().WriteTo.Console().MinimumLevel.Verbose().CreateLogger();
            Notify.SetLogLevel(LogLevelFlags.InfoWarnErrorFatal);

            // Let's try to register this
            if (!NetworkNodeManager._Internal_TryRegisterNode(this, out _index))
                throw new InvalidOperationException("Node has already been added!");
        }

        public NetworkNode(Distribution distro) : this(string.Empty, distro, ExceptionStrategy.ThrowAll, null, null) { }

        public NetworkNode(string name, Distribution distro) : this(name, distro, ExceptionStrategy.ThrowAll, null, null) { }

        ~NetworkNode() => Dispose(false);

        /// <summary>
        /// Rents a <see cref="BytePayload"/> with the specified capacity in bytes.<br/>
        /// -1 implies the default capacity defined by <see cref="BytePayloadPool.DefaultCapacity"/>
        /// </summary>
        /// <param name="capacity"></param>
        /// <returns></returns>
        public BytePayload RentPayload(int capacity = -1) =>
            DataStorage.PayloadPool.Rent(capacity);

        public void Dispose(bool disposing)
        {
            NetworkNodeManager._Internal_TryUnregisterNode(this);
            TryStop(!disposing);

            // Disposes all services
            Services.Dispose(disposing);

            // Ensure our custom data is cleaned up
            DataStorage.Dispose();
        }

        public void Dispose() => Dispose(true);

        /// <summary>
        /// Tries to start this NetworkNode:
        /// <br/>- Opens the socket (creates if necessary; implied by <see cref="Socket"/>#get)
        /// <br/>- Starts the ServiceManager
        /// </summary>
        /// <returns>Whether or not we started</returns>

        public bool TryStart()
        {

            

            /*
            // Try to create our socket
            if (Socket.Create())
            {

                try
                {
                    // Start our services
                    Services.Start();
                    Started = true;

                    Console.Beep();
                    Notify.Debug("Started!");
                    return true;
                }
                catch (Exception ex)
                {
                    Notify.Error("Failed to start!");
                    HandleException(ex);
                }
            }*/

            return false;
        }

        /// <summary>
        /// Polls our socket for new network events
        /// </summary>
        /// <param name="timeoutMs"></param>
        /// <returns></returns>

        public bool Poll(int timeoutMs = 0)
        {
            if (!Started)
            {
                Notify.Error("Tried to poll without calling #TryStart()!");
                return false;
            }

            Socket.Poll(timeoutMs);
            return true;
        }

        /// <summary>
        /// Ticks for configuration changes, ticks the socket, and ticks all
        /// services
        /// </summary>
        /// <param name="delta"></param>
        /// <returns></returns>

        public bool Tick(float delta)
        {
            if (!Started)
            {
                Notify.Error("Tried to tick without calling #TryStart()!");
                return false;
            }

            if (DataStorage.Configuration.Dirty)
            {
                DataStorage.Configuration.WriteAsync();
                DataStorage.Configuration.Dirty = false;
            }

            Socket.Tick(delta);
            Services.Tick(delta);
            return true;
        }

        public bool TryStop()
            => TryStop(false);

        /// <summary>
        /// Tries to stop this NetworkNode
        /// </summary>
        /// <param name="finalizer">Whether or not a finalizer ran this</param>
        /// <returns></returns>

        public bool TryStop(bool finalizer)
        {
            if (!Started)
                return false;

            if (finalizer)
            {
                // Let's discourage this behavior! Bad, bad!
                const string warning = "It's bad practice to not manually stop a NetworkNode. Ensure you call NetworkNode#TryStop() when closing your application.";
                Notify.Warn(warning);
            }

            Console.Beep();
            Socket.Dispose(!finalizer);
            Services.Stop();
            Started = false;

            // Let's ensure we write the configuration fully before we halt
            DataStorage.Configuration.AsyncWriteTask?.Wait();

            Notify.Debug("Stopped!");
            
            return true;
        }

        public void HandleException(Exception exception,
            [CallerFilePath] string callerFilepath = null, 
            [CallerMemberName] string callerMemberName = null)
        {

            // No exception occurred. What is this rubbish!?
            if (exception == null)
                return;

            // TODO: NetworkNode: Perhaps behave differently depending on the severity of the exception?

            switch (ExceptionStrategy)
            {

                case ExceptionStrategy.ThrowAll:
                    // Stop all our services
                    // This will politely disconnect everyone as well.
                    _serviceMgr.Stop();

                    // Scream!
                    throw exception;

                case ExceptionStrategy.LogOnly:
                    RuntimeFileMetadata.GetMetadataFromPath(callerFilepath, out RuntimeFileMetadata metadata, cacheIfNecessary: false);
                    TemplatedMessage msgData = new("[{filename}#{memberName}()] A {exception} occurred with message {message}", metadata.Filename, 
                        ILoggableExtensions.ResolveMemberName(callerMemberName), exception.GetType().Name, exception.Message);
                    Notify.Error(msgData, exception);
                    break;

            }
        }

        public bool Equals(NetworkNode other) =>
            other?.UUID.Equals(UUID) ?? false;

        public override bool Equals(object obj) =>
            obj is NetworkNode other && Equals(other);

        public override int GetHashCode()
        {
            int hashCode = _index + UUID.GetHashCode();
            hashCode ^= _index + 1;
            return hashCode;
        }

        void ILoggable._Internal_SetMetadata(RuntimeFileMetadata metadata)
        {
            _logMetadata = metadata;
        }

        RuntimeFileMetadata ILoggable._Internal_GetMetadata() => _logMetadata;

        internal void _Internal_SetSocket(BaseSocket socket)
        {
            this._socket = socket;
        }
    }

}
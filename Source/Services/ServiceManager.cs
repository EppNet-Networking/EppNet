﻿///////////////////////////////////////////////////////
/// Filename: ServiceManager.cs
/// Date: July 10, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Node;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Collections.Generic;

namespace EppNet.Services
{

    public class ServiceManager : INodeDescendant, IRunnable, IDisposable
    {

        public NetworkNode Node { get => _node; }

        internal NetworkNode _node;

        /// <summary>
        /// Generic set storage because services aren't going to be
        /// added and removed during the lifetime of any delegate app. <br/>Services
        /// are expected to be added when the NetworkNode is instantiated and
        /// when the application is about to close. No need for threading.
        /// </summary>
        protected SortedSet<Service> _services;

        public bool Started { private set; get; }

        internal ServiceManager()
        {
            this._node = null;
            this._services = new();
            this.Started = false;
        }

        public ServiceManager([NotNull] NetworkNode node) : this()
        {
            this._node = node;
        }

        /// <summary>
        /// Sets <see cref="Started"/> to true and
        /// calls <see cref="Service.Start"/> on every service.<br></br>
        /// Does nothing if already started
        /// </summary>

        public bool Start()
        {
            if (Started)
                return false;

            foreach (Service service in _services)
                service.Start();

            this.Started = true;
            return true;
        }
        
        /// <summary>
        /// Calls <see cref="Service.Update"/> on every service
        /// </summary>

        public bool Tick(float dt)
        {
            if (!Started)
                return false;

            foreach (Service service in _services)
                service.Tick(dt);

            return true;
        }

        /// <summary>
        /// Sets <see cref="Started"/> to false and
        /// calls <see cref="Service.Stop"/> on every service.<br></br>
        /// Does nothing if not already started
        /// </summary>

        public bool Stop()
        {
            if (!Started)
                return false;

            foreach (Service service in _services)
                service.Stop();

            this.Started = false;
            return true;
        }

        public void Dispose()
            => Dispose(true);

        public void Dispose(bool disposing)
        {
            foreach (Service service in _services)
                service.Dispose(disposing);
        }

        /// <summary>
        /// Tries to fetch an existing service of type <typeparamref name="T"/> or creates one and returns it.<br/>
        /// <b>NOTE: </b>Service Manager must be offline to add additional services!
        /// </summary>
        /// <typeparam name="T">The type of service to create</typeparam>
        /// <returns>The fetched or created service</returns>

        public T GetOrCreate<T>() where T : Service
        {
            T existing = GetService<T>();

            if (existing == null)
                TryAddService(out existing);

            return existing;
        }

        public bool TryAddService<T>(out T created) where T : Service
        {
            created = null;

            if (Started)
                throw new InvalidOperationException("Cannot add a service while the ServiceManager is running!");

            T existing = GetService<T>();

            if (existing == null)
            {
                try
                {
                    // Every service needs a reference to the service manager
                    created = (T)Activator.CreateInstance(typeof(T), this);
                    _services.Add(created);
                    return true;

                }
                catch (Exception ex)
                {
                    Node.HandleException(ex);
                }
            }

            return false;
        }

        /// <summary>
        /// Tries to add the specified <see cref="Service"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="service"></param>
        /// <returns>Whether it was added or not</returns>

        public bool TryAddService<T>([NotNull] T service) where T : Service
        {
            if (service == null)
                return false;

            if (Started)
                throw new InvalidOperationException("Cannot add a service while the ServiceManager is running!");

            if (service.Node != null)
                throw new InvalidOperationException("Service is already associated with a different NetworkNode!");

            _services.Add(service);
            return true;
        }

        /// <summary>
        /// Fetches the specified <see cref="Service"/> by type<br/>
        /// Returns null if not found.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">Thrown if trying to fetch a Service of type <see cref="Service"/> rather than a derived type.</exception>

        public T GetService<T>() where T : Service
        {
            Type type = typeof(T);

            if (type == typeof(Service))
            {
                InvalidOperationException exp = new("Must specify a derived type of \"Service\"!");

                if (Node == null)
                    throw exp;
                else
                    Node.HandleException(exp);

                return null;
            }

            T result = null;
            foreach (Service service in _services)
            {
                if (service is T tService)
                {
                    result = tService;
                    break;
                }
            }

            return result;
        }

    }

}

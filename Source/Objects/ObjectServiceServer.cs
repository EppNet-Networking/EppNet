/////////////////////////////////////////////
/// Filename: ObjectServiceServer.cs
/// Date: June 19, 2025
/// Authors: Maverick Liberty
//////////////////////////////////////////////

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using EppNet.Collections;
using EppNet.Commands;
using EppNet.Services;

namespace EppNet.Objects
{

    public sealed class ObjectServiceServer : Service
    {

        internal PageList<ObjectSlot> _objects;

        /// <summary>
        /// Network IDs to ticks left before deletion
        /// </summary>
        internal ConcurrentDictionary<long, int> _objectsToDelete;

        public ObjectServiceServer(ServiceManager serviceMgr, int slotsPerPage = 64)
            : base(serviceMgr)
        {

            if (slotsPerPage % 64 != 0)
                throw new ArgumentException($"Slots per page must be a multiple of 64!");

            this._objects = new(slotsPerPage);
            this._objectsToDelete = new();
        }

        public EnumCommandResult TryCreateObject<TObject>(out TObject @object, long networkId = -1)
            where TObject : class, INetworkObject_Impl
        {
            @object = default;
            return EnumCommandResult.Ok;
        }

        public EnumCommandResult TryCreateObject(in Type objectType, out INetworkObject_Impl @object, long networkId = -1)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Tries to enqueue the specified object for deletion next tick
        /// </summary>
        /// <param name="networkId"></param>
        /// <returns></returns>

        public EnumCommandResult TryDeleteObject(long networkId)
        {
            EnumCommandResult result = TryGetObjectById(networkId, out INetworkObject_Impl @object);

            if (result == EnumCommandResult.Ok)
            {
                @object.State.Set(EnumObjectState.PendingDelete);
                _objectsToDelete.AddOrUpdate(networkId, 1, null);
            }

            return result;
        }

        public EnumCommandResult TryGetObjectById<TObject>(long networkId, out TObject @object)
            where TObject : class, INetworkObject_Impl
        {
            @object = null;

            if (_objects.TryGetById(networkId, out ObjectSlot slot))
                @object = (TObject)slot.Object;

            return @object is not null
                ? EnumCommandResult.Ok 
                : EnumCommandResult.NotFound;
        }

        public EnumCommandResult TrySetObjectState(long networkId, EnumObjectState state)
        {
            EnumCommandResult result = TryGetObjectById(networkId, out INetworkObject_Impl @object);

            if (result == EnumCommandResult.Ok)
            {
                @object.State.Set(state);
                return EnumCommandResult.Ok;
            }

            return result;
        }

        public override bool Tick(float dt)
        {
            bool canTick = base.Tick(dt);

            if (canTick)
            {
                foreach (KeyValuePair<long, int> kvp in _objectsToDelete)
                {
                    long networkId = kvp.Key;
                    int ticks = kvp.Value;

                    if (ticks-- <= 0)
                    {
                        // Let's delete this object
                        if (_objects.TryGetById(networkId, out ObjectSlot slot))
                        {
                            // TODO: Delete this object
                        }

                        _objectsToDelete.TryRemove(networkId, out _);
                    }
                    else
                        _objectsToDelete.AddOrUpdate(networkId, ticks, null);
                }
            }

            return canTick;
        }

    }

}
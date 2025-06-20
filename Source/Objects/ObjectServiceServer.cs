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

    public sealed class ObjectServiceServer : Service, IObjectService
    {

        internal PageList<ObjectSlot> _objects;

        /// <summary>
        /// Network IDs to ticks left before deletion
        /// </summary>
        internal List<INetworkObject_Impl> _objectsToDelete;

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

        public EnumCommandResult TryDeleteObject(long networkId, int ticksUntilDeletion = 1)
        {
            EnumCommandResult result = TryGetObjectById(networkId, out INetworkObject_Impl @object);

            if (result == EnumCommandResult.Ok)
            {
                @object.State.Set(EnumObjectState.PendingDelete);
                @object.TicksUntilDeletion.Set(ticksUntilDeletion);
                _objectsToDelete.Add(@object);
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
            bool doTick = base.Tick(dt);

            if (doTick)
            {
                Iterator<INetworkObject_Impl> iterator = _objectsToDelete.Iterator();

                while (iterator.HasNext())
                {
                    INetworkObject_Impl @object = iterator.Next();
                    long ticks = @object.TicksUntilDeletion.Decrement();

                    if (ticks <= 0)
                    {
                        // Time to go bye-bye!
                        iterator.Remove();

                        // TODO: Perhaps debug message if failed to free?
                        bool freed = _objects.TryFree(@object.SlotID);

                        // TODO: Call user deletion logic
                    }
                }

            }

            return doTick;
        }

    }

}
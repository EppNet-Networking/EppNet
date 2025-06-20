/////////////////////////////////////////////
/// Filename: IObjectService.cs
/// Date: March 23, 2025
/// Authors: Maverick Liberty
//////////////////////////////////////////////

using EppNet.Commands;
using EppNet.Services;

using System;

namespace EppNet.Objects
{

    public interface IObjectService : IService
    {

        public EnumCommandResult TryCreateObject<TObject>(out TObject @object, long networkId = -1)
            where TObject : class, INetworkObject_Impl;

        /// <summary>
        /// Creates an <see cref="INetworkObject"/> of the specified type with
        /// the specified network ID.
        /// </summary>
        /// <param name="objectType"></param>
        /// <param name="networkId"></param>
        /// <param name="object"></param>
        /// <returns></returns>
        public EnumCommandResult TryCreateObject(in Type objectType, out INetworkObject_Impl @object, long networkId = -1);

        /// <summary>
        /// Deletes an <see cref="INetworkObject"/> by network ID
        /// </summary>
        /// <param name="networkId"></param>
        /// <returns></returns>
        public EnumCommandResult TryDeleteObject(long networkId);

        /// <summary>
        /// Tries to fetch a Network Object (<see cref="INetworkObject_Impl"/>) by its network ID
        /// </summary>
        /// <param name="id"></param>
        /// <param name="object"></param>
        /// <returns></returns>

        public EnumCommandResult TryGetObjectById<TObject>(long networkId, out TObject @object)
            where TObject : class, INetworkObject_Impl;

        /// <summary>
        /// Tries to set the state of a Network Object (captured by network id)
        /// </summary>
        /// <param name="networkId"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public EnumCommandResult TrySetObjectState(long networkId, EnumObjectState state);

    }

}

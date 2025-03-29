/////////////////////////////////////////////
/// Filename: IObjectService.cs
/// Date: March 23, 2025
/// Authors: Maverick Liberty
//////////////////////////////////////////////

using EppNet.Commands;

namespace EppNet.Objects
{

    public interface IObjectService
    {

        public EnumCommandResult TryCreateObject<TObject>(out TObject @object, long networkId = -1) where TObject: INetworkObject_Impl;

        public EnumCommandResult TryDeleteObject(long networkId);

        /// <summary>
        /// Tries to fetch a Network Object (<see cref="INetworkObject_Impl"/>) by its network id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="object"></param>
        /// <returns></returns>

        public EnumCommandResult TryGetObjectById<TObject>(long networkId, out TObject @object) where TObject : INetworkObject_Impl;

        /// <summary>
        /// Tries to set the state of a Network Object (captured by network id)
        /// </summary>
        /// <param name="networkId"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public EnumCommandResult TrySetObjectState(long networkId, EnumObjectState state);

    }

}

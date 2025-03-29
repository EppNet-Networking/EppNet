/////////////////////////////////////////////
/// Filename: NetworkObjectDefinition.cs
/// Date: March 23, 2025
/// Authors: Maverick Liberty
//////////////////////////////////////////////

using EppNet.Data;

using System;
using System.Security.Principal;

namespace EppNet.Objects
{
    public enum EnumNetworkMemberType
    {
        None = 0,
        Field = 1,
        Property = 2,
        Method = 3
    }

    public class NetworkMember : INameable
    {

        public string Name { private set; get; }

        public int Id { private set; get; }

        public EnumNetworkMemberType Type { get; }

        public INetworkArgs Arguments { get; }

    }

    public class NetworkObjectDefinition
    {





    }

}

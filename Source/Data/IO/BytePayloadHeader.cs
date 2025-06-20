///////////////////////////////////////////////////////
/// Filename: BytePayloadHeader.cs
/// Date: June 12, 2025
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using System;

namespace EppNet.IO
{

    public static class BytePayloadHeader
    {

        public const byte MessageTypeMask = 0b0001_1111;
        public const byte FlagsMask = 0b1110_0000;

        [Flags]
        public enum Flags : byte
        {

            None = 0,
            Encrypted = 0b1,
            Compressed = 0b01,
            Reserved = 0b001
        }

        public enum MessageType : byte
        {
            Unknown = 0,

        }

        public static byte Compose(MessageType msgType, Flags flags) =>
            (byte)((byte) msgType | (byte)flags);

        public static (MessageType, Flags) Read(byte header)
        {
            MessageType messageType = (MessageType)(header & MessageTypeMask);
            Flags flags = (Flags)(header & FlagsMask);
            return (messageType, flags);
        }

        public static MessageType GetType(byte header) =>
            (MessageType)(header & MessageTypeMask);

        public static Flags GetFlags(byte header) =>
            (Flags)(header & FlagsMask);

    }

}

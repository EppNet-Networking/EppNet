///////////////////////////////////////////////////////
/// Filename: ShortResolver.cs
/// Date: August 28, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Attributes;

using System.Buffers.Binary;
using System.Runtime.CompilerServices;

namespace EppNet.Data
{

    [NetworkTypeResolver]
    public class ShortResolver : Resolver<short>
    {

        public static readonly ShortResolver Instance = new();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override ReadResult _Internal_Read(ref BytePayloadReader reader, out short output) =>
            BinaryPrimitives.TryReadInt16LittleEndian(reader.ReadBytes(Size), out output) 
                ? ReadResult.Success : ReadResult.Failed;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override bool _Internal_Write(ref BytePayloadWriter writer, short input) =>
            BinaryPrimitives.TryWriteInt16LittleEndian(writer.Reserve(Size, clear: false), input);
    }

    public static class ShortResolverExtensions
    {

        /// <summary>
        /// Writes a signed 16-bit integer to the stream.
        /// </summary>
        /// <param name="input"></param>
        public static void Write(this ref BytePayloadWriter writer, short input)
            => ShortResolver.Instance.Write(ref writer, input);

        /// <summary>
        /// Writes a signed 16-bit integer array to the stream.
        /// </summary>
        /// <param name="input"></param>
        public static void Write(this ref BytePayloadWriter writer, short[] input)
            => ShortResolver.Instance.Write(ref writer, input);

        /// <summary>
        /// Writes a signed 16-bit integer array to the stream.
        /// </summary>
        /// <param name="input"></param>
        public static void WriteArray(this ref BytePayloadWriter writer, short[] input)
            => ShortResolver.Instance.Write(ref writer, input);

        /// <summary>
        /// Writes a signed 16-bit integer to the stream.
        /// </summary>
        /// <param name="input"></param>
        public static void WriteShort(this ref BytePayloadWriter writer, short input)
            => ShortResolver.Instance.Write(ref writer, input);

        /// <summary>
        /// Writes a signed 16-bit integer array to the stream.
        /// </summary>
        /// <param name="input"></param>
        public static void WriteShortArray(this ref BytePayloadWriter writer, short[] input)
            => ShortResolver.Instance.Write(ref writer, input);

        /// <summary>
        /// Writes a signed 16-bit integer to the stream.
        /// </summary>
        /// <param name="input"></param>
        public static void WriteInt16(this ref BytePayloadWriter writer, short input)
            => ShortResolver.Instance.Write(ref writer, input);

        /// <summary>
        /// Writes a signed 16-bit integer array to the stream.
        /// </summary>
        /// <param name="input"></param>
        public static void WriteInt16Array(this ref BytePayloadWriter writer, short[] input)
            => ShortResolver.Instance.Write(ref writer, input);

        /// <summary>
        /// Reads a signed 16-bit integer from the stream.
        /// </summary>
        public static short ReadShort(this ref BytePayloadReader reader)
        {
            ShortResolver.Instance.Read(ref reader, out short output);
            return output;
        }

        /// <summary>
        /// Reads a signed 16-bit integer array from the stream.
        /// </summary>
        public static short[] ReadShortArray(this ref BytePayloadReader reader)
        {
            ShortResolver.Instance.Read(ref reader, out short[] output);
            return output;
        }

        /// <summary>
        /// Reads a signed 16-bit integer from the stream.
        /// </summary>
        public static short ReadInt16(this ref BytePayloadReader reader)
        {
            ShortResolver.Instance.Read(ref reader, out short output);
            return output;
        }

        /// <summary>
        /// Reads a signed 16-bit integer array from the stream.
        /// </summary>
        public static short[] ReadInt16Array(this ref BytePayloadReader reader)
        {
            ShortResolver.Instance.Read(ref reader, out short[] output);
            return output;
        }

    }

}


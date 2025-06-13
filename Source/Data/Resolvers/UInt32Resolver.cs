///////////////////////////////////////////////////////
/// Filename: UInt32Resolver.cs
/// Date: August 28, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Attributes;

using System.Buffers.Binary;
using System.Runtime.CompilerServices;

namespace EppNet.Data
{

    [NetworkTypeResolver]
    public class UInt32Resolver : Resolver<uint>
    {

        public static readonly UInt32Resolver Instance = new();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override ReadResult _Internal_Read(ref BytePayloadReader reader, out uint output) =>
            BinaryPrimitives.TryReadUInt32LittleEndian(reader.ReadBytes(Size), out output) ? ReadResult.Success : ReadResult.Failed;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override bool _Internal_Write(ref BytePayloadWriter writer, uint input) =>
            BinaryPrimitives.TryWriteUInt32LittleEndian(writer.Reserve(Size, clear: false), input);
    }

    public static class UInt32ResolverExtensions
    {

        /// <summary>
        /// Writes an unsigned 32-bit integer to the stream.
        /// </summary>
        /// <param name="input"></param>
        public static void Write(this ref BytePayloadWriter writer, uint input)
            => UInt32Resolver.Instance.Write(ref writer, input);

        /// <summary>
        /// Writes an unsigned 32-bit integer array to the stream.
        /// </summary>
        /// <param name="input"></param>
        public static void Write(this ref BytePayloadWriter writer, uint[] input)
            => UInt32Resolver.Instance.Write(ref writer, input);

        /// <summary>
        /// Writes an unsigned 32-bit integer array to the stream.
        /// </summary>
        /// <param name="input"></param>
        public static void WriteArray(this ref BytePayloadWriter writer, uint[] input)
            => UInt32Resolver.Instance.Write(ref writer, input);

        /// <summary>
        /// Writes an unsigned 32-bit integer to the stream.
        /// </summary>
        /// <param name="input"></param>
        public static void WriteUInt(this ref BytePayloadWriter writer, uint input)
            => UInt32Resolver.Instance.Write(ref writer, input);

        /// <summary>
        /// Writes an unsigned 32-bit integer array to the stream.
        /// </summary>
        /// <param name="input"></param>
        public static void WriteUIntArray(this ref BytePayloadWriter writer, uint[] input)
            => UInt32Resolver.Instance.Write(ref writer, input);

        /// <summary>
        /// Writes an unsigned 32-bit integer to the stream.
        /// </summary>
        /// <param name="input"></param>
        public static void WriteUInt32(this ref BytePayloadWriter writer, uint input)
            => UInt32Resolver.Instance.Write(ref writer, input);

        /// <summary>
        /// Writes an unsigned 32-bit integer array to the stream.
        /// </summary>
        /// <param name="input"></param>
        public static void WriteUInt32Array(this ref BytePayloadWriter writer, uint[] input)
            => UInt32Resolver.Instance.Write(ref writer, input);

        /// <summary>
        /// Reads an unsigned 32-bit integer from the stream.
        /// </summary>

        public static uint ReadUInt(this ref BytePayloadReader reader)
        {
            UInt32Resolver.Instance.Read(ref reader, out uint output);
            return output;
        }

        /// <summary>
        /// Reads an unsigned 32-bit integer array from the stream.
        /// </summary>
        public static uint[] ReadUIntArray(this ref BytePayloadReader reader)
        {
            UInt32Resolver.Instance.Read(ref reader, out uint[] output);
            return output;
        }

        /// <summary>
        /// Reads an unsigned 32-bit integer from the stream.
        /// </summary>
        public static uint ReadUInt32(this ref BytePayloadReader reader)
        {
            UInt32Resolver.Instance.Read(ref reader, out uint output);
            return output;
        }

        /// <summary>
        /// Reads an unsigned 32-bit integer array from the stream.
        /// </summary>
        public static uint[] ReadUInt32Array(this ref BytePayloadReader reader)
        {
            UInt32Resolver.Instance.Read(ref reader, out uint[] output);
            return output;
        }

    }

}


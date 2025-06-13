///////////////////////////////////////////////////////
/// Filename: ULongResolver.cs
/// Date: August 28, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Attributes;

using System.Buffers.Binary;
using System.Runtime.CompilerServices;

namespace EppNet.Data
{

    [NetworkTypeResolver]
    public class ULongResolver : Resolver<ulong>
    {

        public static readonly ULongResolver Instance = new();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override ReadResult _Internal_Read(ref BytePayloadReader reader, out ulong output) =>
            BinaryPrimitives.TryReadUInt64LittleEndian(reader.ReadBytes(Size), out output) ? ReadResult.Success : ReadResult.Failed;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override bool _Internal_Write(ref BytePayloadWriter writer, ulong input)
            => BinaryPrimitives.TryWriteUInt64LittleEndian(writer.Reserve(Size, clear: false), input);
    }

    public static class ULongResolverExtensions
    {

        /// <summary>
        /// Writes an unsigned 64-bit integer to the stream.
        /// </summary>
        /// <param name="input"></param>
        public static void Write(this ref BytePayloadWriter writer, ulong input)
            => ULongResolver.Instance.Write(ref writer, input);

        /// <summary>
        /// Writes an unsigned 64-bit integer array to the stream.
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="input"></param>
        public static void Write(this ref BytePayloadWriter writer, ulong[] input)
            => ULongResolver.Instance.Write(ref writer, input);

        /// <summary>
        /// Writes an unsigned 64-bit integer array to the stream.
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="input"></param>
        public static void WriteArray(this ref BytePayloadWriter writer, ulong[] input)
            => ULongResolver.Instance.Write(ref writer, input);

        /// <summary>
        /// Writes an unsigned 64-bit integer to the stream.
        /// </summary>
        /// <param name="input"></param>
        public static void WriteULong(this ref BytePayloadWriter writer, ulong input)
            => ULongResolver.Instance.Write(ref writer, input);

        /// <summary>
        /// Writes an unsigned 64-bit integer array to the stream.
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="input"></param>
        public static void WriteULongArray(this ref BytePayloadWriter writer, ulong[] input)
            => ULongResolver.Instance.Write(ref writer, input);

        /// <summary>
        /// Writes an unsigned 64-bit integer to the stream.
        /// </summary>
        /// <param name="input"></param>
        public static void WriteUInt64(this ref BytePayloadWriter writer, ulong input)
            => ULongResolver.Instance.Write(ref writer, input);

        /// <summary>
        /// Writes an unsigned 64-bit integer array to the stream.
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="input"></param>
        public static void WriteUInt64Array(this ref BytePayloadWriter writer, ulong[] input)
            => ULongResolver.Instance.Write(ref writer, input);

        /// <summary>
        /// Reads an unsigned 64-bit integer from the stream.
        /// </summary>
        public static ulong ReadULong(this ref BytePayloadReader reader)
        {
            ULongResolver.Instance.Read(ref reader, out ulong output);
            return output;
        }

        /// <summary>
        /// Reads an unsigned 64-bit integer array from the stream.
        /// </summary>
        public static ulong[] ReadULongArray(this ref BytePayloadReader reader)
        {
            ULongResolver.Instance.Read(ref reader, out ulong[] output);
            return output;
        }

        /// <summary>
        /// Reads an unsigned 64-bit integer from the stream.
        /// </summary>
        public static ulong ReadUInt64(this ref BytePayloadReader reader)
        {
            ULongResolver.Instance.Read(ref reader, out ulong output);
            return output;
        }

        /// <summary>
        /// Reads an unsigned 64-bit integer array from the stream.
        /// </summary>
        public static ulong[] ReadUInt64Array(this ref BytePayloadReader reader)
        {
            ULongResolver.Instance.Read(ref reader, out ulong[] output);
            return output;
        }

    }

}

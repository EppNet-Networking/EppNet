///////////////////////////////////////////////////////
/// Filename: LongResolver.cs
/// Date: August 28, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Attributes;

using System.Buffers.Binary;
using System.Runtime.CompilerServices;

namespace EppNet.Data
{

    [NetworkTypeResolver]
    public class LongResolver : Resolver<long>
    {

        public static readonly LongResolver Instance = new();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override ReadResult _Internal_Read(ref BytePayloadReader reader, out long output) =>
            BinaryPrimitives.TryReadInt64LittleEndian(reader.ReadBytes(Size), out output)
                ? ReadResult.Success : ReadResult.Failed;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override bool _Internal_Write(ref BytePayloadWriter writer, long input) =>
            BinaryPrimitives.TryWriteInt64LittleEndian(writer.Reserve(Size, clear: false), input);

    }

    public static class LongResolverExtensions
    {

        /// <summary>
        /// Writes a signed 64-bit integer to the stream.
        /// </summary>
        /// <param name="input"></param>
        public static void Write(this ref BytePayloadWriter writer, long input) =>
            LongResolver.Instance.Write(ref writer, input);

        /// <summary>
        /// Writes a signed 64-bit integer array to the stream.
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="input"></param>
        public static void Write(this ref BytePayloadWriter writer, long[] input) =>
            LongResolver.Instance.Write(ref writer, input);

        /// <summary>
        /// Writes a signed 64-bit integer array to the stream.
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="input"></param>
        public static void WriteArray(this ref BytePayloadWriter writer, long[] input) =>
            LongResolver.Instance.Write(ref writer, input);

        /// <summary>
        /// Writes a signed 64-bit integer to the stream.
        /// </summary>
        /// <param name="input"></param>
        public static void WriteLong(this ref BytePayloadWriter writer, long input) =>
            LongResolver.Instance.Write(ref writer, input);

        /// <summary>
        /// Writes a signed 64-bit integer array to the stream.
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="input"></param>
        public static void WriteLongArray(this ref BytePayloadWriter writer, long[] input) =>
            LongResolver.Instance.Write(ref writer, input);

        /// <summary>
        /// Writes a signed 64-bit integer to the stream.
        /// </summary>
        /// <param name="input"></param>
        public static void WriteInt64(this ref BytePayloadWriter writer, long input) =>
            LongResolver.Instance.Write(ref writer, input);

        /// <summary>
        /// Writes a signed 64-bit integer array to the stream.
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="input"></param>
        public static void WriteInt64Array(this ref BytePayloadWriter writer, long[] input) =>
            LongResolver.Instance.Write(ref writer, input);

        /// <summary>
        /// Reads a signed 64-bit integer from the stream.
        /// </summary>

        public static long ReadLong(this ref BytePayloadReader reader)
        {
            LongResolver.Instance.Read(ref reader, out long output);
            return output;
        }

        /// <summary>
        /// Reads a signed 64-bit integer array from the stream.
        /// </summary>

        public static long[] ReadLongArray(this ref BytePayloadReader reader)
        {
            LongResolver.Instance.Read(ref reader, out long[] output);
            return output;
        }

        /// <summary>
        /// Reads a signed 64-bit integer from the stream.
        /// </summary>
        public static long ReadInt64(this ref BytePayloadReader reader)
        {
            LongResolver.Instance.Read(ref reader, out long output);
            return output;
        }

        /// <summary>
        /// Reads a signed 64-bit integer array from the stream.
        /// </summary>
        public static long[] ReadInt64Array(this ref BytePayloadReader reader)
        {
            LongResolver.Instance.Read(ref reader, out long[] output);
            return output;
        }

    }

}

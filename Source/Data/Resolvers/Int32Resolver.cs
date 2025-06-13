///////////////////////////////////////////////////////
/// Filename: Int32Resolver.cs
/// Date: August 28, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Attributes;

using System;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;

namespace EppNet.Data
{

    [NetworkTypeResolver]
    public class Int32Resolver : Resolver<int>
    {

        public static readonly Int32Resolver Instance = new();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override ReadResult _Internal_Read(ref BytePayloadReader reader, out int output)
        {
            ReadOnlySpan<byte> buffer = reader.ReadBytes(Size);
            return BinaryPrimitives.TryReadInt32LittleEndian(buffer, out output) ?
                ReadResult.Success : ReadResult.Failed;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override bool _Internal_Write(ref BytePayloadWriter writer, int input) =>
            BinaryPrimitives.TryWriteInt32LittleEndian(writer.Reserve(Size, clear: false), input);
    }

    public static class Int32ResolverExtensions
    {

        /// <summary>
        /// Writes a signed 32-bit integer to the stream.
        /// </summary>
        /// <param name="input"></param>
        public static void Write(this ref BytePayloadWriter writer, int input) =>
            Int32Resolver.Instance.Write(ref writer, input);

        /// <summary>
        /// Writes a signed 32-bit integer array to the stream.
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="input"></param>
        public static void Write(this ref BytePayloadWriter writer, int[] input) =>
            Int32Resolver.Instance.Write(ref writer, input);

        /// <summary>
        /// Writes a signed 32-bit integer array to the stream.
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="input"></param>
        public static void WriteArray(this ref BytePayloadWriter writer, int[] input) =>
            Int32Resolver.Instance.Write(ref writer, input);

        /// <summary>
        /// Writes a signed 32-bit integer to the stream.
        /// </summary>
        /// <param name="input"></param>
        public static void WriteInt(this ref BytePayloadWriter writer, int input) =>
            Int32Resolver.Instance.Write(ref writer, input);

        /// <summary>
        /// Writes a signed 32-bit integer array to the stream.
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="input"></param>
        public static void WriteIntArray(this ref BytePayloadWriter writer, int[] input) =>
            Int32Resolver.Instance.Write(ref writer, input);

        /// <summary>
        /// Writes a signed 32-bit integer to the stream.
        /// </summary>
        /// <param name="input"></param>
        public static void WriteInt32(this ref BytePayloadWriter writer, int input) =>
            Int32Resolver.Instance.Write(ref writer, input);

        /// <summary>
        /// Writes a signed 32-bit integer array to the stream.
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="input"></param>
        public static void WriteInt32Array(this ref BytePayloadWriter writer, int[] input) =>
            Int32Resolver.Instance.Write(ref writer, input);

        /// <summary>
        /// Reads a signed 32-bit integer from the stream.
        /// </summary>

        public static int ReadInt(this ref BytePayloadReader reader)
        {
            Int32Resolver.Instance.Read(ref reader, out int output);
            return output;
        }

        /// <summary>
        /// Reads a signed 32-bit integer array from the stream.
        /// </summary>
        public static int[] ReadIntArray(this ref BytePayloadReader reader)
        {
            Int32Resolver.Instance.Read(ref reader, out int[] output);
            return output;
        }

        /// <summary>
        /// Reads a signed 32-bit integer from the stream.
        /// </summary>
        public static int ReadInt32(this ref BytePayloadReader reader)
        {
            Int32Resolver.Instance.Read(ref reader, out int output);
            return output;
        }

        /// <summary>
        /// Reads a signed 32-bit integer array from the stream.
        /// </summary>
        public static int[] ReadInt32Array(this ref BytePayloadReader reader)
        {
            Int32Resolver.Instance.Read(ref reader, out int[] output);
            return output;
        }

    }

}


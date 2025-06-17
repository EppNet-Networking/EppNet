///////////////////////////////////////////////////////
/// Filename: ByteResolver.cs
/// Date: August 28, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Attributes;
using EppNet.IO;

using System.Runtime.CompilerServices;

namespace EppNet.Data
{

    [NetworkTypeResolver]
    public class ByteResolver : Resolver<byte>
    {

        public static readonly ByteResolver Instance = new();

        public ByteResolver() : base(autoAdvance: false) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override ReadResult _Internal_Read(ref BytePayloadReader reader, out byte output)
        {
            bool read = reader.TryReadByte(out output);
            return read ? ReadResult.Success : ReadResult.Failed;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override bool _Internal_Write(ref BytePayloadWriter writer, byte input)
        {
            writer.WriteByte(input);
            return true;
        }

    }

    public static class ByteResolverExtensions
    {

        /// <summary>
        /// Writes an unsigned 8-bit integer to the stream.
        /// </summary>
        /// <param name="input"></param>
        public static void Write(this ref BytePayloadWriter writer, byte input) =>
            ByteResolver.Instance.Write(ref writer, input);

        /// <summary>
        /// Writes an unsigned 8-bit integer array to the stream.
        /// </summary>
        /// <param name="input"></param>
        public static void Write(this ref BytePayloadWriter writer, byte[] input) =>
            ByteResolver.Instance.Write(ref writer, input);

        /// <summary>
        /// Writes an unsigned 8-bit integer array to the stream.
        /// </summary>
        /// <param name="input"></param>
        public static void WriteArray(this ref BytePayloadWriter writer, byte[] input) =>
            ByteResolver.Instance.Write(ref writer, input);

        /// <summary>
        /// Writes an unsigned 8-bit integer to the stream.
        /// </summary>
        /// <param name="input"></param>
        public static void WriteByte(this ref BytePayloadWriter writer, byte input) =>
            ByteResolver.Instance.Write(ref writer, input);

        /// <summary>
        /// Writes an unsigned 8-bit integer array to the stream.
        /// </summary>
        /// <param name="input"></param>
        public static void WriteByteArray(this ref BytePayloadWriter writer, byte[] input) =>
            ByteResolver.Instance.Write(ref writer, input);

        /// <summary>
        /// Writes an unsigned 8-bit integer to the stream.
        /// </summary>
        /// <param name="input"></param>
        public static void WriteUInt8(this ref BytePayloadWriter writer, byte input) =>
            ByteResolver.Instance.Write(ref writer, input);

        /// <summary>
        /// Writes an unsigned 8-bit integer array to the stream.
        /// </summary>
        /// <param name="input"></param>
        public static void WriteUInt8Array(this ref BytePayloadWriter writer, byte[] input) =>
            ByteResolver.Instance.Write(ref writer, input);

        /// <summary>
        /// Reads an unsigned 8-bit integer from the stream.
        /// </summary>
        public static byte ReadByte(this ref BytePayloadReader reader)
        {
            ByteResolver.Instance.Read(ref reader, out byte output);
            return output;
        }

        /// <summary>
        /// Reads an unsigned 8-bit integer array from the stream.
        /// </summary>
        public static byte[] ReadByteArray(this ref BytePayloadReader reader)
        {
            ByteResolver.Instance.Read(ref reader, out byte[] output);
            return output;
        }

        /// <summary>
        /// Reads an unsigned 8-bit integer from the stream.
        /// </summary>
        public static byte ReadUInt8(this ref BytePayloadReader reader)
        {
            ByteResolver.Instance.Read(ref reader, out byte output);
            return output;
        }

        /// <summary>
        /// Reads an unsigned 8-bit integer array from the stream.
        /// </summary>
        public static byte[] ReadUInt8Array(this ref BytePayloadReader reader)
        {
            ByteResolver.Instance.Read(ref reader, out byte[] output);
            return output;
        }

    }

}

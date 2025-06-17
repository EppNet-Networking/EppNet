///////////////////////////////////////////////////////
/// Filename: SByteResolver.cs
/// Date: August 28, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Attributes;
using EppNet.IO;

using System.Runtime.CompilerServices;

namespace EppNet.Data
{

    [NetworkTypeResolver]
    public class SByteResolver : Resolver<sbyte>
    {

        public static readonly SByteResolver Instance = new();
        public SByteResolver() : base(autoAdvance: false) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override ReadResult _Internal_Read(ref BytePayloadReader reader, out sbyte output)
        {
            bool read = reader.TryReadByte(out byte result);
            output = (sbyte) result;
            return read ? ReadResult.Success : ReadResult.Failed;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override bool _Internal_Write(ref BytePayloadWriter writer, sbyte input)
        {
            writer.WriteByte((byte)input);
            return true;
        }

    }

    public static class SByteResolverExtensions
    {

        /// <summary>
        /// Writes a signed 8-bit integer to the stream.
        /// </summary>
        /// <param name="input"></param>
        public static void Write(this ref BytePayloadWriter writer, sbyte input) =>
            SByteResolver.Instance.Write(ref writer, input);

        /// <summary>
        /// Writes a signed 8-bit integer array to the stream.
        /// </summary>
        /// <param name="input"></param>
        public static void Write(this ref BytePayloadWriter writer, sbyte[] input) =>
            SByteResolver.Instance.Write(ref writer, input);

        /// <summary>
        /// Writes a signed 8-bit integer array to the stream.
        /// </summary>
        /// <param name="input"></param>
        public static void WriteArray(this ref BytePayloadWriter writer, sbyte[] input) =>
            SByteResolver.Instance.Write(ref writer, input);

        /// <summary>
        /// Writes a signed 8-bit integer to the stream.
        /// </summary>
        /// <param name="input"></param>
        public static void WriteSByte(this ref BytePayloadWriter writer, sbyte input) =>
            SByteResolver.Instance.Write(ref writer, input);

        /// <summary>
        /// Writes a signed 8-bit integer array to the stream.
        /// </summary>
        /// <param name="input"></param>
        public static void WriteSByteArray(this ref BytePayloadWriter writer, sbyte[] input) =>
            SByteResolver.Instance.Write(ref writer, input);

        /// <summary>
        /// Writes a signed 8-bit integer to the stream.
        /// </summary>
        /// <param name="input"></param>
        public static void WriteInt8(this ref BytePayloadWriter writer, sbyte input) =>
            SByteResolver.Instance.Write(ref writer, input);

        /// <summary>
        /// Writes a signed 8-bit integer array to the stream.
        /// </summary>
        /// <param name="input"></param>
        public static void WriteInt8Array(this ref BytePayloadWriter writer, sbyte[] input) =>
            SByteResolver.Instance.Write(ref writer, input);

        /// <summary>
        /// Reads a signed 8-bit integer from the stream.
        /// </summary>
        public static sbyte ReadSByte(this ref BytePayloadReader reader)
        {
            SByteResolver.Instance.Read(ref reader, out sbyte output);
            return output;
        }

        /// <summary>
        /// Reads a signed 8-bit integer array from the stream.
        /// </summary>
        public static sbyte[] ReadSByteArray(this ref BytePayloadReader reader)
        {
            SByteResolver.Instance.Read(ref reader, out sbyte[] output);
            return output;
        }

        /// <summary>
        /// Reads a signed 8-bit integer from the stream.
        /// </summary>
        public static sbyte ReadInt8(this ref BytePayloadReader reader)
        {
            SByteResolver.Instance.Read(ref reader, out sbyte output);
            return output;
        }

        /// <summary>
        /// Reads a signed 8-bit integer array from the stream.
        /// </summary>
        public static sbyte[] ReadInt8Array(this ref BytePayloadReader reader)
        {
            SByteResolver.Instance.Read(ref reader, out sbyte[] output);
            return output;
        }
        
    }

}


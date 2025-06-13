///////////////////////////////////////////////////////
/// Filename: UShortResolver.cs
/// Date: August 28, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Attributes;

using System;
using System.Buffers.Binary;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace EppNet.Data
{

    [NetworkTypeResolver]

    public class UShortResolver : Resolver<ushort>
    {

        public static readonly UShortResolver Instance = new();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override ReadResult _Internal_Read(ref BytePayloadReader reader, out ushort output) =>
            BinaryPrimitives.TryReadUInt16LittleEndian(reader.ReadBytes(Size), out output) 
            ? ReadResult.Success : ReadResult.Failed;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override bool _Internal_Write(ref BytePayloadWriter writer, ushort input) =>
            BinaryPrimitives.TryWriteUInt16LittleEndian(writer.Reserve(Size, clear: false), input);
    }

    public static class UShortResolverExtensions
    {

        /// <summary>
        /// Writes an unsigned 16-bit integer to the stream.
        /// </summary>
        /// <param name="input"></param>
        public static void Write(this ref BytePayloadWriter writer, ushort input) =>
            UShortResolver.Instance.Write(ref writer, input);

        /// <summary>
        /// Writes an unsigned 16-bit integer array to the stream.
        /// </summary>
        /// <param name="input"></param>
        public static void Write(this ref BytePayloadWriter writer, ushort[] input) =>
            UShortResolver.Instance.Write(ref writer, input);

        /// <summary>
        /// Writes an unsigned 16-bit integer collection to the stream.
        /// </summary>
        /// <param name="input"></param>
        public static void Write<TCollection>(this ref BytePayloadWriter writer, TCollection input) where TCollection : ICollection<ushort> =>
            UShortResolver.Instance.Write(ref writer, input);

        /// <summary>
        /// Writes an unsigned 16-bit integer array to the stream.
        /// </summary>
        /// <param name="input"></param>
        public static void WriteArray(this ref BytePayloadWriter writer, ushort[] input) =>
            UShortResolver.Instance.Write(ref writer, input);

        /// <summary>
        /// Writes an unsigned 16-bit integer to the stream.
        /// </summary>
        /// <param name="input"></param>
        public static void WriteUShort(this ref BytePayloadWriter writer, ushort input) =>
            UShortResolver.Instance.Write(ref writer, input);

        /// <summary>
        /// Writes an unsigned 16-bit integer array to the stream.
        /// </summary>
        /// <param name="input"></param>
        public static void WriteUShortArray(this ref BytePayloadWriter writer, ushort[] input) =>
            UShortResolver.Instance.Write(ref writer, input);

        /// <summary>
        /// Writes an unsigned 16-bit integer to the stream.
        /// </summary>
        /// <param name="input"></param>
        public static void WriteUInt16(this ref BytePayloadWriter writer, ushort input) =>
            UShortResolver.Instance.Write(ref writer, input);

        /// <summary>
        /// Writes an unsigned 16-bit integer array to the stream.
        /// </summary>
        /// <param name="input"></param>
        public static void WriteUInt16Array(this ref BytePayloadWriter writer, ushort[] input) =>
            UShortResolver.Instance.Write(ref writer, input);

        /// <summary>
        /// Reads an unsigned 16-bit integer collection from the stream.
        /// </summary>
        /// <param name="input"></param>
        public static TCollection Read<TCollection>(this ref BytePayloadReader reader) where TCollection : class, ICollection<ushort>, new()
        {
            UShortResolver.Instance.Read(ref reader, out TCollection output);
            return output;
        }

        /// <summary>
        /// Reads an unsigned 16-bit integer from the stream.
        /// </summary>
        public static ushort ReadUShort(this ref BytePayloadReader reader)
        {
            UShortResolver.Instance.Read(ref reader, out ushort output);
            return output;
        }

        /// <summary>
        /// Reads an unsigned 16-bit integer array from the stream.
        /// </summary>
        public static ushort[] ReadUShortArray(this ref BytePayloadReader reader)
        {
            UShortResolver.Instance.Read(ref reader, out ushort[] output);
            return output;
        }

        /// <summary>
        /// Reads an unsigned 16-bit integer from the stream.
        /// </summary>
        public static ushort ReadUInt16(this ref BytePayloadReader reader)
        {
            UShortResolver.Instance.Read(ref reader, out ushort output);
            return output;
        }

        /// <summary>
        /// Reads an unsigned 16-bit integer array from the stream.
        /// </summary>
        public static ushort[] ReadUInt16Array(this ref BytePayloadReader reader)
        {
            UShortResolver.Instance.Read(ref reader, out ushort[] output);
            return output;
        }

    }

}

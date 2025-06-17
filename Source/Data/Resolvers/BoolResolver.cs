///////////////////////////////////////////////////////
/// Filename: BoolResolver.cs
/// Date: August 28, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Attributes;
using EppNet.IO;

using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace EppNet.Data
{

    [NetworkTypeResolver]
    public class BoolResolver : Resolver<bool>
    {

        public static BoolResolver Instance = new();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override ReadResult _Internal_Read(ref BytePayloadReader reader, out bool output)
        {
            bool read = reader.TryReadByte(out byte result);
            output = result == 1;

            return read ? ReadResult.Success : ReadResult.Failed;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override bool _Internal_Write(ref BytePayloadWriter writer, bool input)
        {
            writer.WriteByte(input ? (byte) 1 : (byte) 0);
            return true;
        }
    }

    public static class BoolResolverExtensions
    {

        /// <summary>
        /// Writes an unsigned 8-bit integer to the stream denoting 1 (true) or 0 (false).
        /// </summary>
        /// <param name="input"></param>
        public static void Write(this ref BytePayloadWriter writer, bool input) =>
            BoolResolver.Instance.Write(ref writer, input);

        /// <summary>
        /// Writes an unsigned 8-bit integer array to the stream denoting 1 (true) or 0 (false).
        /// </summary>
        /// <param name="input"></param>
        public static void Write(this ref BytePayloadWriter writer, bool[] input) =>
            BoolResolver.Instance.Write(ref writer, input);

        /// <summary>
        /// Writes an unsigned 8-bit integer array to the stream denoting 1 (true) or 0 (false).
        /// </summary>
        /// <param name="input"></param>
        public static void Write<TCollection>(this ref BytePayloadWriter writer, TCollection input) where TCollection : ICollection<bool> =>
            BoolResolver.Instance.Write(ref writer, input);

        /// <summary>
        /// Writes an unsigned 8-bit integer array to the stream denoting 1 (true) or 0 (false).
        /// </summary>
        /// <param name="input"></param>
        public static void WriteArray(this ref BytePayloadWriter writer, bool[] input) =>
            BoolResolver.Instance.Write(ref writer, input);

        /// <summary>
        /// Writes an unsigned 8-bit integer to the stream denoting 1 (true) or 0 (false).
        /// </summary>
        /// <param name="input"></param>
        public static void WriteBool(this ref BytePayloadWriter writer, bool input) =>
            BoolResolver.Instance.Write(ref writer, input);

        /// <summary>
        /// Writes an unsigned 8-bit integer array to the stream denoting 1 (true) or 0 (false).
        /// </summary>
        /// <param name="input"></param>
        public static void WriteBoolArray(this ref BytePayloadWriter writer, bool[] input) =>
            BoolResolver.Instance.Write(ref writer, input);

        /// <summary>
        /// Reads an unsigned 8-bit integer collection from the stream denoting 1 (true) or 0 (false).
        /// </summary>
        /// <param name="input"></param>
        public static TCollection Read<TCollection>(this ref BytePayloadReader reader) where TCollection : class, ICollection<bool>, new()
        {
            BoolResolver.Instance.Read(ref reader, out TCollection output);
            return output;
        }

        /// <summary>
        /// Reads an unsigned 8-bit integer from the stream denoting 1 (true) or 0 (false).
        /// </summary>
        /// <param name="input"></param>
        public static bool ReadBool(this ref BytePayloadReader reader)
        {
            BoolResolver.Instance.Read(ref reader, out bool output);
            return output;
        }

        /// <summary>
        /// Reads an unsigned 8-bit integer array from the stream denoting 1 (true) or 0 (false).
        /// </summary>
        /// <param name="input"></param>
        public static bool[] ReadBoolArray(this ref BytePayloadReader reader)
        {
            BoolResolver.Instance.Read(ref reader, out bool[] output);
            return output;
        }

    }

}

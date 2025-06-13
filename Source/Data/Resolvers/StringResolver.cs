///////////////////////////////////////////////////////
/// Filename: StringResolver.cs
/// Date: June 13, 2025
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using System;

namespace EppNet.Data
{

    public class StringResolver : Resolver<string>
    {

        public static readonly StringResolver Instance = new();

        public const byte NullStringHeader = 0;
        public const byte EmptyStringHeader = 0b0000_0001;
        public const byte ByteStringHeader = 0b0000_0010;
        public const byte ShortStringHeader = 0b0000_0011;
        public const byte IntStringHeader = 0b0000_0100;

        protected StringResolver() : base(size: 0, autoAdvance: false) { }

        protected override ReadResult _Internal_Read(ref BytePayloadReader reader, out string output)
        {
            byte header = reader.ReadByte();
            int numBytes;

            switch (header)
            {

                case NullStringHeader:
                    output = null;
                    return ReadResult.Success;

                case EmptyStringHeader:
                    output = string.Empty;
                    return ReadResult.Success;

                case ByteStringHeader:
                    numBytes = reader.ReadByte();
                    break;

                case ShortStringHeader:
                    ShortResolver.Instance.Read(ref reader, out short bytes);
                    numBytes = bytes;
                    break;

                case IntStringHeader:
                    Int32Resolver.Instance.Read(ref reader, out numBytes);
                    break;

                default:
                    output = default;
                    return ReadResult.Failed;
            }

            if (numBytes > reader.Remaining || numBytes < 0)
            {
                output = default;
                return ReadResult.Failed;
            }

            output = reader.Encoder.GetString(reader.ReadBytes(numBytes));
            return ReadResult.Success;
        }

        protected override bool _Internal_Write(ref BytePayloadWriter writer, string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                writer.WriteByte(input == null ? NullStringHeader : EmptyStringHeader);
                return true;
            }

            int bytes = writer.Encoder.GetByteCount(input);

            if (bytes > ushort.MaxValue)
            {
                writer.WriteByte(IntStringHeader);
                Int32Resolver.Instance.Write(ref writer, bytes);
            }
            else if (bytes > byte.MaxValue)
            {
                writer.WriteByte(ShortStringHeader);
                ShortResolver.Instance.Write(ref writer, bytes);
            }
            else
            {
                writer.WriteByte(ByteStringHeader);
                writer.WriteByte((byte)bytes);
            }

            writer.Encoder.GetBytes(input.AsSpan(), writer.Reserve(bytes, clear: false));
            return true;
        }

    }

    public static class StringResolverExtensions
    {

        /// <summary>
        /// Writes a string to the stream.<br/><br/>
        /// 
        /// The following strings are considered special and cost 1 byte:<br/>
        /// - <see cref="string.Empty"/><br/>
        /// - null
        /// 
        /// <br/><br/>
        /// 
        /// Bandwidth cost: 1 byte to 2,147,483,647 bytes (~2GB)<br/><br/>
        /// <b><i>Notice</i></b><br/>
        /// For frequently sent strings consider a lookup table with an indice instead!
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="input"></param>

        public static void Write(this ref BytePayloadWriter writer, string input) =>
            StringResolver.Instance.Write(ref writer, input);

        /// <summary>
        /// Writes a string to the stream.<br/><br/>
        /// 
        /// The following strings are considered special and cost 1 byte:<br/>
        /// - <see cref="string.Empty"/><br/>
        /// - null
        /// 
        /// <br/><br/>
        /// 
        /// Bandwidth cost: 1 byte to 2,147,483,647 bytes (~2GB)<br/><br/>
        /// <b><i>Notice</i></b><br/>
        /// For frequently sent strings consider a lookup table with an indice instead!
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="input"></param>

        public static void WriteString(this ref BytePayloadWriter writer, string input) =>
            StringResolver.Instance.Write(ref writer, input);

        /// <summary>
        /// Writes an array of strings to the stream.<br/><br/>
        /// 
        /// The following strings are considered special and cost 1 byte:<br/>
        /// - <see cref="string.Empty"/><br/>
        /// - null
        /// 
        /// <br/><br/>
        /// 
        /// Bandwidth cost: 1 byte to ~2GB per string<br/><br/>
        /// <b><i>Notice</i></b><br/>
        /// For frequently sent strings consider a lookup table with an indice instead!
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="input"></param>

        public static void Write(this ref BytePayloadWriter writer, string[] input) =>
            StringResolver.Instance.Write(ref writer, input);

        /// <summary>
        /// Writes an array of strings to the stream.<br/><br/>
        /// 
        /// The following strings are considered special and cost 1 byte:<br/>
        /// - <see cref="string.Empty"/><br/>
        /// - null
        /// 
        /// <br/><br/>
        /// 
        /// Bandwidth cost: 1 byte to ~2GB per string<br/><br/>
        /// <b><i>Notice</i></b><br/>
        /// For frequently sent strings consider a lookup table with an indice instead!
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="input"></param>

        public static void WriteArray(this ref BytePayloadWriter writer, string[] input) =>
            StringResolver.Instance.Write(ref writer, input);

        /// <summary>
        /// Writes an array of strings to the stream.<br/><br/>
        /// 
        /// The following strings are considered special and cost 1 byte:<br/>
        /// - <see cref="string.Empty"/><br/>
        /// - null
        /// 
        /// <br/><br/>
        /// 
        /// Bandwidth cost: 1 byte to ~2GB per string<br/><br/>
        /// <b><i>Notice</i></b><br/>
        /// For frequently sent strings consider a lookup table with an indice instead!
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="input"></param>

        public static void WriteStringArray(this ref BytePayloadWriter writer, string[] input) =>
            StringResolver.Instance.Write(ref writer, input);

        /// <summary>
        /// Reads a string from the stream.
        /// </summary>
        /// <param name="reader"></param>
        /// <returns>The received string</returns>

        public static string ReadString(this ref BytePayloadReader reader)
        {
            StringResolver.Instance.Read(ref reader, out string output);
            return output;
        }

        /// <summary>
        /// Reads an array of strings from the stream.
        /// </summary>
        /// <param name="reader"></param>
        /// <returns>The received string</returns>

        public static string[] ReadArray(this ref BytePayloadReader reader)
        {
            StringResolver.Instance.Read(ref reader, out string[] output);
            return output;
        }

        /// <summary>
        /// Reads an array of strings from the stream.
        /// </summary>
        /// <param name="reader"></param>
        /// <returns>The received string</returns>

        public static string[] ReadStringArray(this ref BytePayloadReader reader) =>
            ReadArray(ref reader);
    }

}

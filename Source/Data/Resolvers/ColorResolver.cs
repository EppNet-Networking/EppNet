///////////////////////////////////////////////////////
/// Filename: ColorResolver.cs
/// Date: August 28, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Attributes;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;

namespace EppNet.Data
{

    [NetworkTypeResolver]
    public class ColorResolver : Resolver<Color>
    {

        public static ColorResolver Instance = new();

        public ColorResolver() : base(size: 4, autoAdvance: false) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override ReadResult _Internal_Read(ref BytePayloadReader reader, out Color output)
        {
            if (reader.TryReadBytes(Size, out ReadOnlySpan<byte> data))
            {
                output = Color.FromArgb(data[0], data[1], data[2], data[3]);
                return ReadResult.Success;
            }

            output = Color.White;
            return ReadResult.Failed;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override bool _Internal_Write(ref BytePayloadWriter writer, Color input)
        {
            Span<byte> bytes = writer.Reserve(Size, clear: false);
            bytes[0] = input.A;
            bytes[1] = input.R;
            bytes[2] = input.G;
            bytes[3] = input.B;
            return true;
        }

    }

    public static class ColorResolverExtensions
    {

        /// <summary>
        /// Writes 4 unsigned 8-bit integers to the stream denoting the Color
        /// </summary>
        /// <param name="input"></param>
        public static void Write(this ref BytePayloadWriter writer, Color input)
            => ColorResolver.Instance.Write(ref writer, input);

        /// <summary>
        /// Writes an array of <see cref="Color"/> to the stream. </br>
        /// See <see cref="Write(BytePayload, Color)"/> for more info on how each Color is written.
        /// </summary>
        /// <param name="input"></param>
        public static void Write(this ref BytePayloadWriter writer, Color[] input)
            => ColorResolver.Instance.Write(ref writer, input);

        /// <summary>
        /// Writes the specified collection input to the stream<br/>
        /// See <see cref="Write(BytePayload, Color)"/> for more info on how each Color is written
        /// </summary>
        /// <param name="input"></param>
        public static void Write<TCollection>(this ref BytePayloadWriter writer, TCollection input) where TCollection : ICollection<Color>
            => ColorResolver.Instance.Write(ref writer, input);

        /// <summary>
        /// Writes an array of <see cref="Color"/> to the stream. </br>
        /// See <see cref="Write(BytePayload, Color)"/> for more info on how each Color is written.
        /// </summary>
        /// <param name="input"></param>
        public static void WriteArray(this ref BytePayloadWriter writer, Color[] input)
            => ColorResolver.Instance.Write(ref writer, input);

        /// <summary>
        /// Writes 4 unsigned 8-bit integers to the stream denoting the Color
        /// </summary>
        /// <param name="input"></param>
        public static void WriteColor(this ref BytePayloadWriter writer, Color input)
            => ColorResolver.Instance.Write(ref writer, input);

        /// <summary>
        /// Writes the specified collection input to the stream<br/>
        /// See <see cref="Write(BytePayload, Color)"/> for more info on how each Color is written
        /// </summary>
        /// <param name="input"></param>
        public static void WriteColorArray(this ref BytePayloadWriter writer, Color[] input)
            => ColorResolver.Instance.Write(ref writer, input);

        /// <summary>
        /// Reads a Guid collection from the stream<br/>
        /// See <see cref="Write(BytePayload, Color)"/> for more info on how each Color is written
        /// </summary>

        public static TCollection Read<TCollection>(this ref BytePayloadReader reader) where TCollection : class, ICollection<Color>, new()
        {
            ColorResolver.Instance.Read(ref reader, out TCollection output);
            return output;
        }

        /// <summary>
        /// Reads 16 unsigned 8-bit integers from the stream denoting a Color
        /// </summary>
        public static Color ReadColor(this ref BytePayloadReader reader)
        {
            ColorResolver.Instance.Read(ref reader, out Color output);
            return output;
        }

        /// <summary>
        /// Reads a Color array from the stream<br/>
        /// See <see cref="Write(BytePayload, Color)"/> for more info on how each Color is written
        /// </summary>
        public static Color[] ReadGuidArray(this ref BytePayloadReader reader)
        {
            ColorResolver.Instance.Read(ref reader, out Color[] output);
            return output;
        }

    }

}

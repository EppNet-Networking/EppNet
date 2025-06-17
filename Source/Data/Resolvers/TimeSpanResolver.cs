///////////////////////////////////////////////////////
/// Filename: TimeSpanResolver.cs
/// Date: September 30, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Attributes;
using EppNet.IO;

using System;
using System.Buffers.Binary;

namespace EppNet.Data
{

    [NetworkTypeResolver]
    public class TimeSpanResolver : Resolver<TimeSpan>
    {
        public static readonly TimeSpanResolver Instance = new();

        public TimeSpanResolver() : base(sizeof(long)) { }

        protected override ReadResult _Internal_Read(ref BytePayloadReader reader, out TimeSpan output)
        {
            ReadResult result = BinaryPrimitives.TryReadInt64LittleEndian(reader.ReadBytes(Size), out long ticks)
                ? ReadResult.Success : ReadResult.Failed;

            output = TimeSpan.FromTicks(ticks);
            return result;
        }

        protected override bool _Internal_Write(ref BytePayloadWriter writer, TimeSpan input) =>
            BinaryPrimitives.TryWriteInt64LittleEndian(writer.Reserve(Size, clear: false), input.Ticks);
    }

    public static class TimeSpanResolverExtensions
    {

        /// <summary>
        /// Writes a <see cref="TimeSpan"/> to the wire.<br/>
        /// Internally, this writes the ticks as a long
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="input"></param>

        public static void Write(this ref BytePayloadWriter writer, TimeSpan input) =>
            TimeSpanResolver.Instance.Write(ref writer, input);

        /// <summary>
        /// Writes an array of <see cref="TimeSpan"/> to the wire.<br/>
        /// See <see cref="Write(BytePayload, TimeSpan)"/> for more information.
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="input"></param>
        public static void Write(this ref BytePayloadWriter writer, TimeSpan[] input) =>
            TimeSpanResolver.Instance.Write(ref writer, input);
        
        /// <summary>
        /// Writes an array of <see cref="TimeSpan"/> to the wire.<br/>
        /// See <see cref="Write(BytePayload, TimeSpan)"/> for more information.
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="input"></param>
        public static void WriteArray(this ref BytePayloadWriter writer, TimeSpan[] input) =>
            TimeSpanResolver.Instance.Write(ref writer, input);

        /// <summary>
        /// Reads a <see cref="TimeSpan"/> from the wire.<br/>
        /// </summary>
        /// <param name="payload"></param>

        public static TimeSpan Read(this ref BytePayloadReader reader)
        {
            TimeSpanResolver.Instance.Read(ref reader, out TimeSpan result);
            return result;
        }

        /// <summary>
        /// Reads a <see cref="TimeSpan"/> array from the wire.<br/>
        /// </summary>
        /// <param name="payload"></param>

        public static TimeSpan[] ReadArray(this ref BytePayloadReader reader)
        {
            TimeSpanResolver.Instance.Read(ref reader, out TimeSpan[] result);
            return result;
        }

    }

}

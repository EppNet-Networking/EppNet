///////////////////////////////////////////////////////
/// Filename: FloatResolver.cs
/// Date: August 28, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Attributes;
using EppNet.IO;
using EppNet.Utilities;

using System;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;

namespace EppNet.Data
{

    [NetworkTypeResolver]
    public class FloatResolver : Resolver<float>
    {

        public static readonly FloatResolver Instance = new();

        public FloatResolver() : base(sizeof(int)) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override ReadResult _Internal_Read(ref BytePayloadReader reader, out float output)
        {
            ReadOnlySpan<byte> buffer = reader.ReadBytes(Size);
            
            bool didRead = BinaryPrimitives.TryReadInt32LittleEndian(buffer, out int i32);
            output = didRead ? (float)(i32 * BytePayload.PrecisionReturnDecimalPlaces) : float.NaN;
            return didRead ? ReadResult.Success : ReadResult.Failed;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override bool _Internal_Write(ref BytePayloadWriter writer, float input)
        {
            double rounded = input.Round(BytePayload.FloatPrecision);
            int i32 = (int)(rounded * BytePayload.PrecisionDecimalPlaces);

            return BinaryPrimitives.TryWriteInt32LittleEndian(writer.Reserve(Size, clear: false), i32);
        }

    }

    public static class FloatResolverExtensions
    {

        /// <summary>
        /// Writes a provided float input as a 32-bit integer.<br/>
        /// Floats are sent with the precision specified by <see cref="BytePayload.FloatPrecision"/>
        /// </summary>
        /// <param name="input"></param>

        public static void Write(this ref BytePayloadWriter writer, float input) =>
            FloatResolver.Instance.Write(ref writer, input);

        /// <summary>
        /// Writes the provided float array as a signed 32-bit integer array to the stream.
        /// Floats are sent with the precision specified by <see cref="BytePayload.FloatPrecision"/>
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="input"></param>
        public static void Write(this ref BytePayloadWriter writer, float[] input) =>
            FloatResolver.Instance.Write(ref writer, input);

        /// <summary>
        /// Writes the provided float array as a signed 32-bit integer array to the stream.
        /// Floats are sent with the precision specified by <see cref="BytePayload.FloatPrecision"/>
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="input"></param>
        public static void WriteArray(this ref BytePayloadWriter writer, float[] input) =>
            FloatResolver.Instance.Write(ref writer, input);

        /// <summary>
        /// Writes a provided float input as a 32-bit integer.<br/>
        /// Floats are sent with the precision specified by <see cref="BytePayload.FloatPrecision"/>
        /// </summary>
        /// <param name="input"></param>

        public static void WriteSingle(this ref BytePayloadWriter writer, float input) =>
            FloatResolver.Instance.Write(ref writer, input);

        /// <summary>
        /// Writes the provided float array as a signed 32-bit integer array to the stream.
        /// Floats are sent with the precision specified by <see cref="BytePayload.FloatPrecision"/>
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="input"></param>
        public static void WriteSingleArray(this ref BytePayloadWriter writer, float[] input) =>
            FloatResolver.Instance.Write(ref writer, input);

        /// <summary>
        /// Writes a provided float input as a 32-bit integer.<br/>
        /// Floats are sent with the precision specified by <see cref="BytePayload.FloatPrecision"/>
        /// </summary>
        /// <param name="input"></param>

        public static void WriteFloat(this ref BytePayloadWriter writer, float input) =>
            FloatResolver.Instance.Write(ref writer, input);

        /// <summary>
        /// Writes the provided float array as a signed 32-bit integer array to the stream.
        /// Floats are sent with the precision specified by <see cref="BytePayload.FloatPrecision"/>
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="input"></param>
        public static void WriteFloatArray(this ref BytePayloadWriter writer, float[] input) =>
            FloatResolver.Instance.Write(ref writer, input);

        /// <summary>
        /// Reads a float from the stream.<br/>
        /// Floats are sent with the precision specified by <see cref="BytePayload.FloatPrecision"/>
        /// </summary>
        /// <returns><see cref="float.NaN"/> is returned if reading failed</returns>

        public static float ReadSingle(this ref BytePayloadReader reader)
        {
            FloatResolver.Instance.Read(ref reader, out float output);
            return output;
        }

        /// <summary>
        /// Reads a signed 32-bit integer array from the stream and converts each element to the
        /// proper float. <br/>
        /// Floats are sent with the precision specified by <see cref="BytePayload.FloatPrecision"/>
        /// </summary>
        /// <returns><see cref="float.NaN"/> is provided for each element that failed to be read</returns>
        public static float[] ReadSingleArray(this ref BytePayloadReader reader)
        {
            FloatResolver.Instance.Read(ref reader, out float[] output);
            return output;
        }

        /// <summary>
        /// Reads a float from the stream.<br/>
        /// Floats are sent with the precision specified by <see cref="BytePayload.FloatPrecision"/>
        /// </summary>
        /// <returns><see cref="float.NaN"/> is returned if reading failed</returns>

        public static float ReadFloat(this ref BytePayloadReader reader)
        {
            FloatResolver.Instance.Read(ref reader, out float output);
            return output;
        }

        /// <summary>
        /// Reads a signed 32-bit integer array from the stream and converts each element to the
        /// proper float. <br/>
        /// Floats are sent with the precision specified by <see cref="BytePayload.FloatPrecision"/>
        /// </summary>
        /// <returns><see cref="float.NaN"/> is provided for each element that failed to be read</returns>
        public static float[] ReadFloatArray(this ref BytePayloadReader reader)
        {
            FloatResolver.Instance.Read(ref reader, out float[] output);
            return output;
        }

    }

}

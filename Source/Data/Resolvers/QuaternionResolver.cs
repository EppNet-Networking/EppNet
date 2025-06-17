///////////////////////////////////////////////////////
/// Filename: QuaternionResolver.cs
/// Date: August 29, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Attributes;
using EppNet.IO;

using System;

using System.Numerics;

namespace EppNet.Data
{

    [NetworkTypeResolver]
    public class QuaternionResolver : QuaternionResolverBase<Quaternion>
    {

        public static readonly QuaternionResolver Instance = new();

        public QuaternionResolver() : base()
        {
            this.Zero = new(0, 0, 0, 0);
            this.Identity = new(0, 0, 0, 1);
        }

        public override void CopyTo(in Quaternion quaternion, Span<float> data)
        {
            data[0] = quaternion.X;
            data[1] = quaternion.Y;
            data[2] = quaternion.Z;
            data[3] = quaternion.W;
        }

        public override Quaternion ToNative(Span<float> data) =>
            data.Length == NumComponents ?
                new(data[0], data[1], data[2], data[3]) :
                Quaternion.Identity;
    }

    public static class QuaternionResolverExtensions
    {

        /// <summary>
        /// Writes a normalized <see cref="Quaternion"/> to the stream using the Smallest Three algorithm.<br/>
        /// Cost varies based on if <see cref="QuaternionResolver.ByteQuantization"/> is enabled, and the
        /// particular quaternion sent. <see cref="Quaternion.Zero"/> and <see cref="Quaternion.Identity"/> are
        /// the cheapest at 1 byte each. <br/><br/>
        /// With quantization enabled, the maximum size for a quaternion is 4 bytes; otherwise, it sends
        /// <br/>one single per value which is 4 bytes per value or 33 bytes including the header.<br/><br/>
        /// Effective margin of error: ±0.0156
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="input"></param>
        public static void Write(this ref BytePayloadWriter writer, Quaternion input) =>
            QuaternionResolver.Instance.Write(ref writer, input);

        /// <summary>
        /// Writes an array of <see cref="Quaternion"/> to the stream<br/>
        /// See <see cref="Write(BytePayload, Quaternion)"/> for more information on how each quaternion is written
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="input"></param>
        public static void Write(this ref BytePayloadWriter writer, Quaternion[] input) =>
            QuaternionResolver.Instance.Write(ref writer, input);

        /// <summary>
        /// Writes an array of <see cref="Quaternion"/> to the stream<br/>
        /// See <see cref="Write(BytePayload, Quaternion)"/> for more information on how each quaternion is written
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="input"></param>
        public static void WriteArray(this ref BytePayloadWriter writer, Quaternion[] input) =>
            QuaternionResolver.Instance.Write(ref writer, input);

        /// <summary>
        /// Writes a normalized <see cref="Quaternion"/> to the stream using the Smallest Three algorithm.<br/>
        /// Cost varies based on if <see cref="QuaternionResolver.ByteQuantization"/> is enabled, and the
        /// particular quaternion sent. <see cref="Quaternion.Zero"/> and <see cref="Quaternion.Identity"/> are
        /// the cheapest at 1 byte each. <br/><br/>
        /// With quantization enabled, the maximum size for a quaternion is 4 bytes; otherwise, it sends
        /// <br/>one single per value which is 4 bytes per value or 33 bytes including the header.<br/><br/>
        /// Effective margin of error: ±0.0156
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="input"></param>
        public static void WriteQuat(this ref BytePayloadWriter writer, Quaternion input) =>
            QuaternionResolver.Instance.Write(ref writer, input);

        /// <summary>
        /// Writes an array of <see cref="Quaternion"/> to the stream<br/>
        /// See <see cref="Write(BytePayload, Quaternion)"/> for more information on how each quaternion is written
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="input"></param>
        public static void WriteQuatArray(this ref BytePayloadWriter writer, Quaternion[] input) =>
            QuaternionResolver.Instance.Write(ref writer, input);

        /// <summary>
        /// Writes a normalized <see cref="Quaternion"/> to the stream using the Smallest Three algorithm.<br/>
        /// Cost varies based on if <see cref="QuaternionResolver.ByteQuantization"/> is enabled, and the
        /// particular quaternion sent. <see cref="Quaternion.Zero"/> and <see cref="Quaternion.Identity"/> are
        /// the cheapest at 1 byte each. <br/><br/>
        /// With quantization enabled, the maximum size for a quaternion is 4 bytes; otherwise, it sends
        /// <br/>one single per value which is 4 bytes per value or 33 bytes including the header.<br/><br/>
        /// Effective margin of error: ±0.0156
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="input"></param>
        public static void WriteQuaternion(this ref BytePayloadWriter writer, Quaternion input) =>
            QuaternionResolver.Instance.Write(ref writer, input);

        /// <summary>
        /// Writes an array of <see cref="Quaternion"/> to the stream<br/>
        /// See <see cref="Write(BytePayload, Quaternion)"/> for more information on how each quaternion is written
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="input"></param>
        public static void WriteQuaternionArray(this ref BytePayloadWriter writer, Quaternion[] input) =>
            QuaternionResolver.Instance.Write(ref writer, input);

        /// <summary>
        /// Reads a normalized <see cref="Quaternion"/> from the stream.<br/>
        /// For more information on how these are encoded, see: <see cref="Write(BytePayload, Quaternion)"/><br/>
        /// Effective margin of error: ±0.0156
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>

        public static Quaternion ReadQuat(this ref BytePayloadReader reader)
        {
            QuaternionResolver.Instance.Read(ref reader, out Quaternion output);
            return output;
        }

        /// <summary>
        /// Reads a normalized <see cref="Quaternion"/> array from the stream.<br/>
        /// For more information on how these are encoded, see: <see cref="Write(BytePayload, Quaternion)"/><br/>
        /// Effective margin of error: ±0.0156
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>

        public static Quaternion[] ReadQuatArray(this ref BytePayloadReader reader)
        {
            QuaternionResolver.Instance.Read(ref reader, out Quaternion[] output);
            return output;
        }

        /// <summary>
        /// Reads a normalized <see cref="Quaternion"/> from the stream.<br/>
        /// For more information on how these are encoded, see: <see cref="Write(BytePayload, Quaternion)"/><br/>
        /// Effective margin of error: ±0.0156
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>

        public static Quaternion ReadQuaternion(this ref BytePayloadReader reader)
        {
            QuaternionResolver.Instance.Read(ref reader, out Quaternion output);
            return output;
        }

        /// <summary>
        /// Reads a normalized <see cref="Quaternion"/> array from the stream.<br/>
        /// For more information on how these are encoded, see: <see cref="Write(BytePayload, Quaternion)"/><br/>
        /// Effective margin of error: ±0.0156
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>

        public static Quaternion[] ReadQuaternionArray(this ref BytePayloadReader reader)
        {
            QuaternionResolver.Instance.Read(ref reader, out Quaternion[] output);
            return output;
        }

    }

}

﻿///////////////////////////////////////////////////////
/// Filename: UnityVector3Resolver.cs
/// Date: September 2, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Data;

using UnityEngine;

namespace EppNet.Unity.Data
{

    public class UnityVector3Resolver : UnityVectorResolverBase<Vector3>
    {

        public static readonly UnityVector3Resolver Instance = new UnityVector3Resolver();

        public UnityVector3Resolver() : base(false)
        {
            this.Default = Vector3.zero;
            this.UnitX = Vector3.right;
            this.UnitY = Vector3.up;
            this.UnitZ = Vector3.forward;
            this.UnitW = this.Default;
            this.One = Vector3.one;
            this.NumComponents = 3;
        }

        public override Vector3 Put(Vector3 input, float value, int index)
        {
            input[index] = value;
            return input;
        }

    }

    public static class UnityVector3ResolverExtensions
    {

        /// <summary>
        /// Writes a <see cref="Vector3"/> to the stream.<br/><br/>
        /// 
        /// <b><i>Transmission Modes</i></b><br/>
        /// <paramref name="absolute"/>: Absolutely every component will be sent over the wire.<br/>
        /// !<paramref name="absolute"/>/delta: Only non-zero components will be sent over the wire.<br/><br/>
        /// 
        /// <b><i>Component Transmission</i></b><br/>
        /// Components can be sent as <see cref="sbyte"/>, <see cref="short"/>,
        /// <see cref="int"/>, or a <see cref="float"/> with lossy precision.<br/><br/>
        /// 
        /// Components are sent as the smallest possible type that can contain each
        /// component.<br/>
        /// 
        /// <b>Float Precision</b> is dictated by <see cref="BytePayload.FloatPrecision"/>.
        /// 
        /// <br/><br/>
        /// The following <see cref="Vector3"/>s are considered special and cost 1 byte:<br/>
        /// - <see cref="Vector3.zero"/><br/>
        /// - <see cref="Vector3.right"/><br/>
        /// - <see cref="Vector3.up"/><br/>
        /// - <see cref="Vector3.forward"/><br/>
        /// - <see cref="Vector3.one"/><br/><br/>
        /// 
        /// Bandwidth cost: 1 byte to 13 bytes
        /// </summary>
        /// <param name="payload">The BytePayload to write data to.</param>
        /// <param name="input">The Vector3 to transmit.</param>
        /// <param name="absolute">Should ABSOLUTELY all components be transmitted?</param>
        /// <returns>Whether or not the Vector3 was written</returns>

        public static bool Write(this BytePayload payload, Vector3 input, bool absolute = true)
            => UnityVector3Resolver.Instance.Write(payload, input, absolute);

        /// <summary>
        /// Writes a <see cref="Vector3"/> array to the stream.<br/><br/>
        /// 
        /// Empty and null arrays cost 1 byte to write to the wire.<br/>
        /// See <see cref="Write(BytePayload, Vector3, bool)"/> for more information on 
        /// how each is written.<br/><br/>
        /// 
        /// Bandwidth cost: 1 byte to N * 13 bytes where N is the number of array elements
        /// </summary>
        /// <param name="payload">The BytePayload to write data to.</param>
        /// <param name="input">The Vector3 to transmit.</param>
        /// <param name="absolute">Should ABSOLUTELY all components be transmitted?</param>
        /// <returns>Whether or not the Vector3 array was written</returns>

        public static bool Write(this BytePayload payload, Vector3[] input, bool absolute = true)
            => UnityVector3Resolver.Instance.WriteArray(payload, input, absolute);

        /// <summary>
        /// Writes a <see cref="Vector3"/> array to the stream.<br/><br/>
        /// 
        /// Empty and null arrays cost 1 byte to write to the wire.<br/>
        /// See <see cref="Write(BytePayload, Vector3, bool)"/> for more information on 
        /// how each is written.<br/><br/>
        /// 
        /// Bandwidth cost: 1 byte to N * 13 bytes where N is the number of array elements
        /// </summary>
        /// <param name="payload">The BytePayload to write data to.</param>
        /// <param name="input">The Vector3 to transmit.</param>
        /// <param name="absolute">Should ABSOLUTELY all components be transmitted?</param>
        /// <returns>Whether or not the Vector3 array was written</returns>

        public static bool WriteArray(this BytePayload payload, Vector3[] input, bool absolute = true)
            => UnityVector3Resolver.Instance.WriteArray(payload, input, absolute);

        /// <summary>
        /// Writes a <see cref="Vector3"/> in <b>DELTA MODE</b> to the stream.<br/>
        /// Only non-zero components are transmitted.<br/><br/>
        ///
        /// <paramref name="newVector"/> - <paramref name="oldVector"/> = delta Vector3<br/><br/>
        /// 
        /// <b><i>Component Transmission</i></b><br/>
        /// Components can be sent as <see cref="sbyte"/>, <see cref="short"/>,
        /// <see cref="int"/>, or a <see cref="float"/> with lossy precision.<br/><br/>
        /// 
        /// Components are sent as the smallest possible type that can contain each
        /// component.<br/>
        /// 
        /// <b>Float Precision</b> is dictated by <see cref="BytePayload.FloatPrecision"/>.
        /// 
        /// <br/><br/>
        /// The following <see cref="Vector3"/>s are considered special and cost 1 byte:<br/>
        /// - <see cref="Vector3.zero"/><br/>
        /// - <see cref="Vector3.right"/><br/>
        /// - <see cref="Vector3.up"/><br/>
        /// - <see cref="Vector3.forward"/><br/>
        /// - <see cref="Vector3.one"/><br/><br/>
        /// 
        /// Bandwidth cost: 1 byte to 13 bytes
        /// </summary>
        /// <param name="payload">The BytePayload to write data to.</param>
        /// <param name="newVector">The new vector</param>
        /// <param name="oldVector">The old vector</param>
        /// <returns>Whether or not the Vector3 was written</returns>

        public static bool Write(this BytePayload payload, Vector3 newVector, Vector3 oldVector)
        {
            Vector3 delta = newVector - oldVector;
            return UnityVector3Resolver.Instance.Write(payload, delta, absolute: false);
        }

        /// <summary>
        /// Writes a <see cref="Vector3"/> to the stream.<br/><br/>
        /// 
        /// <b><i>Transmission Modes</i></b><br/>
        /// <paramref name="absolute"/>: Absolutely every component will be sent over the wire.<br/>
        /// !<paramref name="absolute"/>/delta: Only non-zero components will be sent over the wire.<br/><br/>
        /// 
        /// <b><i>Component Transmission</i></b><br/>
        /// Components can be sent as <see cref="sbyte"/>, <see cref="short"/>,
        /// <see cref="int"/>, or a <see cref="float"/> with lossy precision.<br/><br/>
        /// 
        /// Components are sent as the smallest possible type that can contain each
        /// component.<br/>
        /// 
        /// <b>Float Precision</b> is dictated by <see cref="BytePayload.FloatPrecision"/>.
        /// 
        /// <br/><br/>
        /// The following <see cref="Vector3"/>s are considered special and cost 1 byte:<br/>
        /// - <see cref="Vector3.zero"/><br/>
        /// - <see cref="Vector3.right"/><br/>
        /// - <see cref="Vector3.up"/><br/>
        /// - <see cref="Vector3.forward"/><br/>
        /// - <see cref="Vector3.one"/><br/><br/>
        /// 
        /// Bandwidth cost: 1 byte to 13 bytes
        /// </summary>
        /// <param name="payload">The BytePayload to write data to.</param>
        /// <param name="input">The Vector3 to transmit.</param>
        /// <param name="absolute">Should ABSOLUTELY all components be transmitted?</param>
        /// <returns>Whether or not the Vector3 was written</returns>

        public static bool WriteVector3(this BytePayload payload, Vector3 input, bool absolute = true)
            => UnityVector3Resolver.Instance.Write(payload, input, absolute);

        /// <summary>
        /// Writes a <see cref="Vector3"/> in <b>DELTA MODE</b> to the stream.<br/>
        /// Only non-zero components are transmitted.<br/><br/>
        ///
        /// <paramref name="newVector"/> - <paramref name="oldVector"/> = delta Vector3<br/><br/>
        /// 
        /// <b><i>Component Transmission</i></b><br/>
        /// Components can be sent as <see cref="sbyte"/>, <see cref="short"/>,
        /// <see cref="int"/>, or a <see cref="float"/> with lossy precision.<br/><br/>
        /// 
        /// Components are sent as the smallest possible type that can contain each
        /// component.<br/>
        /// 
        /// <b>Float Precision</b> is dictated by <see cref="BytePayload.FloatPrecision"/>.
        /// 
        /// <br/><br/>
        /// The following <see cref="Vector3"/>s are considered special and cost 1 byte:<br/>
        /// - <see cref="Vector3.zero"/><br/>
        /// - <see cref="Vector3.right"/><br/>
        /// - <see cref="Vector3.up"/><br/>
        /// - <see cref="Vector3.forward"/><br/>
        /// - <see cref="Vector3.one"/><br/><br/>
        /// 
        /// Bandwidth cost: 1 byte to 13 bytes
        /// </summary>
        /// <param name="payload">The BytePayload to write data to.</param>
        /// <param name="newVector">The new vector</param>
        /// <param name="oldVector">The old vector</param>
        /// <returns>Whether or not the Vector3 was written</returns>

        public static bool WriteVector3(this BytePayload payload, Vector3 newVector, Vector3 oldVector)
        {
            Vector3 delta = newVector - oldVector;
            return UnityVector3Resolver.Instance.Write(payload, delta, absolute: false);
        }

        /// <summary>
        /// <b><i>This method is not recommended!</i></b><br/>
        /// Disregards if fetched vector is absolute or delta!<br/><br/>
        /// Only use this if you're sure the received vector is absolute.<br/><br/>
        /// 
        /// Reads an assumed to be absolute <see cref="Vector3"/> from the stream.
        /// </summary>
        /// <param name="payload">The BytePayload to read data from.</param>
        /// <returns></returns>

        public static Vector3 ReadVector3(this BytePayload payload)
        {
            UnityVector3Resolver.Instance.Read(payload, out Vector3 result);
            return result;
        }

        /// <summary>
        /// Reads a <see cref="Vector3"/> array from the stream. Outputs if it's an absolute vector array,
        /// </summary>
        /// <param name="payload">The BytePayload to read data from.</param>
        /// <param name="isAbsolute">If the received array contains absolute vectors</param>
        /// <returns>The received Vector3 array</returns>

        public static Vector3[] ReadVector3Array(this BytePayload payload, out bool isAbsolute)
        {
            ReadResult result = UnityVector3Resolver.Instance.Read(payload, out Vector3[] output);
            isAbsolute = result == ReadResult.Success;
            return output;
        }

        /// <summary>
        /// <b><i>Recommended over <see cref="ReadVector3(BytePayload)"/></i></b><br/><br/>
        /// Reads a <see cref="Vector3"/> from the stream, and, if in delta mode, adds<br/>
        /// <paramref name="oldVector"/> to the result.
        /// </summary>
        /// <param name="payload">The BytePayload to read data from.</param>
        /// <param name="oldVector">The vector to add if the received one is a delta vector</param>
        /// <returns>
        /// <b>Absolute Mode</b>
        /// <br/>Received vector<br/><br/>
        /// <b>Delta Mode</b>
        /// <br/>Received vector + <paramref name="oldVector"/>
        /// </returns>

        public static Vector3 ReadVector3(this BytePayload payload, Vector3 oldVector)
        {
            ReadResult result = UnityVector3Resolver.Instance.Read(payload, out Vector3 output);

            if (result == ReadResult.SuccessDelta)
                return oldVector + output;

            return output;
        }

        /// <summary>
        /// <b><i>Recommended over <see cref="ReadVector3(BytePayload)"/></i></b><br/><br/>
        /// Reads a <see cref="Vector3"/> from the stream. Outputs if it's an absolute vector.
        /// </summary>
        /// <param name="payload">The BytePayload to read data from.</param>
        /// <param name="isAbsolute">If the received vector is absolute</param>
        /// <returns>The received Vector3</returns>

        public static Vector3 ReadVector3(this BytePayload payload, out bool isAbsolute)
        {
            ReadResult result = UnityVector3Resolver.Instance.Read(payload, out Vector3 output);
            isAbsolute = result == ReadResult.Success;
            return output;
        }

    }

}

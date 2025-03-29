///////////////////////////////////////////////////////
/// Filename: ISignatureEquatable.cs
/// Date: March 28, 2025
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

namespace EppNet.Data
{

    /// <summary>
    /// When dealing with generics, this ensures that the types are equivalent
    /// </summary>
    /// <typeparam name="T"></typeparam>

    public interface ISignatureEquatable<T>
    {

        /// <summary>
        /// Checks for type equivalence
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        bool SignatureEquals(T other);
    }

}

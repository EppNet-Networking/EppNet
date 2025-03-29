///////////////////////////////////////////////////////
/// Filename: ICloneable.cs
/// Date: March 28, 2025
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

namespace EppNet.Data
{

    /// <summary>
    /// Not to be confused with <see cref="System.ICloneable"/>, this creates a type-safe clone.<br/>
    /// Output should <b>NEVER</b> be null
    /// </summary>
    /// <typeparam name="T"></typeparam>

    public interface ICloneable<T>
    {

        /// <summary>
        /// Creates a type-safe clone
        /// </summary>
        /// <returns>Copy; <b>NEVER</b> null</returns>
        T Clone();
    }

}


///////////////////////////////////////////////////////
/// Filename: IDataHolder.cs
/// Date: July 28, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using System.Diagnostics.CodeAnalysis;
using EppNet.Node;

namespace EppNet.Data
{

    /// <summary>
    /// Handy interface that allows external code to associate data with a
    /// specific instance. Generic functions are here simply for convenience and do not imply non-boxing.<br/>
    /// Boxing will occur no matter what function you elect to use.
    /// </summary>

    public interface IDataHolder : INodeDescendant
    {

        /// <summary>
        /// Maps the specified key to the specified value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">A non-null non-empty string</param>
        /// <param name="value">The value to associate with the key. Null is permitted</param>
        /// <returns>Whether or not the data was mapped</returns>

        public bool Set<T>([NotNull] string key, T value) =>
            Node.DataStorage.AddOrUpdateCustomDataEntry(this, key, value);

        /// <summary>
        /// Tries to map the specified key to the specified value<br/>
        /// if the key hasn't been map to a value already.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">A non-null non-empty string</param>
        /// <param name="value">The value to associate with the key. Null is permitted</param>
        /// <returns>Whether or not the data was mapped</returns>
        public bool SetIfAbsent<T>([NotNull] string key, T value)
        {
            if (!Has(key))
                return Set(key, value);

            return false;
        }

        /// <summary>
        /// Gets the value associated with the specified key and casts it to<br/>
        /// the specified type.<br/><br/>
        /// <strong>Careful!</strong> This assumes that the entry exists and the value can be casted!
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">A non-null non-empty string</param>
        /// <returns>The value associated with the specified key</returns>

        public T Get<T>([NotNull] string key) =>
            (T)Node.DataStorage.GetCustomDataEntry(this, key);

        /// <summary>
        /// Gets the value associated with the specified key
        /// </summary>
        /// <param name="key">A non-null non-empty string</param>
        /// <returns>The value associated with the specified key</returns>
        public object Get([NotNull] string key) =>
            Node.DataStorage.GetCustomDataEntry(this, key);

        /// <summary>
        /// Tries to get the value mapped with the specified key -- if it can be cast to T. If not, <br/>
        /// it returns the specified fallback value.
        /// </summary>
        /// <param name="key">A non-null non-empty string</param>
        /// <param name="value">The value associated with this entry</param>
        /// <returns>The value mapped to the specified key or the fallback value</returns>

        public T GetOrDefault<T>([NotNull] string key, T fallback) =>
            TryGet(key, out T result) ? result : fallback;

        /// <summary>
        /// Tries to get the value mapped with the specified key -- if it can be cast to T.
        /// </summary>
        /// <param name="key">A non-null non-empty string</param>
        /// <param name="value">The value associated with this entry</param>
        /// <returns>Whether or not a value was fetched successfully</returns>
        public bool TryGet<T>([NotNull] string key, out T value)
        {
            bool fetched = Node.DataStorage.TryGetCustomDataEntry(this, key, out object obj);

            if (fetched && obj is T casted)
            {
                value = casted;
                return true;
            }

            value = default;
            return false;
        }

        /// <summary>
        /// Tries to get the value mapped to the specified key
        /// </summary>
        /// <param name="key">A non-null non-empty string</param>
        /// <param name="value">The value associated with this entry</param>
        /// <returns>Whether or not a value was fetched successfully</returns>
        public bool TryGet([NotNull] string key, out object value) =>
            Node.DataStorage.TryGetCustomDataEntry(this, key, out value);

        /// <summary>
        /// Removes the specified custom data entry. Returns the value (if it existed)
        /// </summary>
        /// <param name="key">A non-null non-entry string</param>
        /// <param name="value">The value associated with the removed key</param>
        /// <returns>Whether or not an entry was deleted</returns>
        public bool Remove([NotNull] string key, out object value) =>
            Node.DataStorage.RemoveCustomDataEntry(this, key, out value);

        /// <summary>
        /// Whether or not this <see cref="IDataHolder"/> contains an entry with the specified name.
        /// </summary>
        /// <param name="key">A non-null non-empty string</param>
        /// <returns></returns>
        public bool Has([NotNull] string key) =>
            Node.DataStorage.TryGetCustomDataEntry(this, key, out _);

        /// <summary>
        /// Deletes all custom data associated with this <see cref="IDataHolder"/>
        /// </summary>
        /// <returns>Whether or not a custom data dictionary was deleted.</returns>
        public bool DeleteAll() =>
            Node.DataStorage.DeleteCustomData(this);

    }

}

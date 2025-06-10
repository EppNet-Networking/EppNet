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
    /// specific instance
    /// </summary>

    public interface IDataHolder : INodeDescendant
    {

        public bool Set<T>([NotNull] string key, T value) =>
            Node.DataStorage.AddOrUpdateCustomDataEntry(this, key, value);

        public bool SetIfAbsent<T>([NotNull] string key, T value)
        {
            if (!Has(key))
                return Set(key, value);

            return false;
        }

        public T Get<T>([NotNull] string key) =>
            (T)Node.DataStorage.GetCustomDataEntry(this, key);

        public object Get([NotNull] string key) =>
            Node.DataStorage.GetCustomDataEntry(this, key);

        public T GetOrDefault<T>([NotNull] string key, T fallback) =>
            TryGet(key, out T result) ? result : fallback;

        public bool TryGet<T>([NotNull] string key, out T value)
        {
            bool fetched = Node.DataStorage.TryGetCustomDataEntry(this, key, out object obj);
            value = default;

            if (fetched && obj is T casted)
            {
                value = casted;
                return true;
            }

            return false;
        }

        public bool TryGet([NotNull] string key, out object value) =>
            Node.DataStorage.TryGetCustomDataEntry(this, key, out value);

        public bool Remove([NotNull] string key, out object value) =>
            Node.DataStorage.RemoveCustomDataEntry(this, key, out value);

        public bool Has([NotNull] string key) =>
            Node.DataStorage.TryGetCustomDataEntry(this, key, out _);

    }
}

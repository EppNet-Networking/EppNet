///////////////////////////////////////////////////////
/// Filename: Iterator.cs
/// Date: September 10, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace EppNet.Collections
{

    public ref struct Iterator<T>
    {

        public int Index { private set; get; }

        private readonly List<T> _elements;

        public Iterator(List<T> list)
        {
            this.Index = -1;
            this._elements = list ?? throw new ArgumentNullException(nameof(list));
        }

        /// <summary>
        /// Checks if we have something next in the iterator
        /// </summary>
        /// <returns></returns>
        public readonly bool HasNext() =>
            Index + 1 < _elements.Count;

        /// <summary>
        /// Increments the internal index and returns the
        /// next element
        /// </summary>
        /// <returns></returns>
        public T Next() =>
            _elements[++Index];

        public readonly T Current() =>
            _elements[Index];

        /// <summary>
        /// Removes the element at the current index
        /// </summary>
        /// <returns></returns>
        public T Remove()
        {
            T removed = Current();
            _elements.RemoveAt(Index--);
            return removed;
        }
    }

    public static class IteratorExtensions
    {

        public static Iterator<T> Iterator<T>(this List<T> list) => new(list);

    }

}
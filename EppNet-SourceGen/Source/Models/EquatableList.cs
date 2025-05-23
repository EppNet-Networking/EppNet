﻿/////////////////////////////////////////////
/// Filename: EquatableList.cs
/// Date: ?????
/// Author: Microsoft
/// Source: https://github.com/dotnet/roslyn/blob/main/docs/features/incremental-generators.cookbook.md#use-forattributewithmetadataname
//////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

namespace EppNet.SourceGen.Models
{
    public class EquatableList<T> : List<T>, IEquatable<EquatableList<T>>
    {

        public EquatableList() : base() { }

        public EquatableList(List<T> source) : base(source) { }

        public bool Equals(EquatableList<T> other)
        {
            // If the other list is null or a different size, they're not equal
            if (other is null || Count != other.Count)
                return false;

            // Compare each pair of elements for equality
            for (int i = 0; i < Count; i++)
            {
                if (!EqualityComparer<T>.Default.Equals(this[i], other[i]))
                    return false;
            }

            // If we got this far, the lists are equal
            return true;
        }

        public override bool Equals(object obj) =>
            obj is not null &&
            obj is EquatableList<T> other &&
            Equals(other);

        public override int GetHashCode() =>
            this.Count == 0 ? 0 :
            this.Select(item => item?.GetHashCode() ?? 0)
            .Aggregate((x, y) => x ^ y);

        public static bool operator ==(EquatableList<T> list1, EquatableList<T> list2) =>
            ReferenceEquals(list1, list2) || 
            list1 is not null && 
            list2 is not null &&
            list1.Equals(list2);

        public static bool operator !=(EquatableList<T> list1, EquatableList<T> list2) =>
            !(list1 == list2);
    }

}

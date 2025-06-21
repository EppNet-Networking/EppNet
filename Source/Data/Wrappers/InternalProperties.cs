///////////////////////////////////////////////////////
/// Filename: InternalProperties.cs
/// Date: June 20, 2025
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using System;

namespace EppNet.Data.Wrappers
{

    /// <summary>
    /// Wrapper of a property only settable by this library<br/>
    /// Created to get around interface restrictions
    /// </summary>
    /// <typeparam name="T"></typeparam>

    public abstract class InternalProperty<T> : IEquatable<T>, IEquatable<InternalProperty<T>>
    {

        public T Value { private set; get; }

        /// <summary>
        /// Action called when this property changes<br/>
        /// (New value followed by the old one)
        /// </summary>
        public Action<T, T> OnChanged { set; get; }

        public InternalProperty()
        {
            this.Value = default;
            this.OnChanged = null;
        }

        public InternalProperty(T defaultValue)
        {
            this.Value = defaultValue;
            this.OnChanged = null;
        }

        public virtual T Get() => Value;

        public override bool Equals(object obj) =>
            obj is InternalProperty<T> other
            && Equals(other);

        public bool Equals(T other) =>
            this.Value.Equals(other);

        public bool Equals(InternalProperty<T> other) =>
            other is not null && Value.Equals(other.Value);

        internal void Set(T newValue)
        {
            T current = Value;

            if (!current.Equals(newValue))
            {
                this.Value = newValue;
                OnChanged?.Invoke(newValue, current);
            }
        }

        internal void Reset()
        {
            Set(default);
        }

        public override string ToString() =>
            Value?.ToString() ?? "null";

        public override int GetHashCode() =>
            Value?.GetHashCode() ?? 0;

    }

    public class ManagedInternalProperty<T> : InternalProperty<T>
        where T : class
    {

        public ManagedInternalProperty(T defaultValue = null) : base(defaultValue)
        { }

    }

    public abstract class NumericInternalProperty<T> : InternalProperty<T>
            where T : struct
        {

            protected NumericInternalProperty(T defaultValue = default) : base(defaultValue)
            {
            }

            internal abstract T Increment();
            internal abstract T Decrement();
            internal abstract T Add(T delta);
            internal abstract T Subtract(T delta);

        }

    public class IntInternalProperty : NumericInternalProperty<int>
    {

        public IntInternalProperty(int defaultValue) : base(defaultValue)
        { }

        internal override int Increment()
        {
            int newValue = Value + 1;
            Set(newValue);
            return newValue;
        }

        internal override int Decrement()
        {
            int newValue = Value - 1;
            Set(newValue);
            return newValue;
        }

        internal override int Add(int delta)
        {
            int newValue = Value + delta;
            Set(newValue);
            return newValue;
        }

        internal override int Subtract(int delta)
        {
            int newValue = Value - delta;
            Set(newValue);
            return newValue;
        }

    }

    public class LongInternalProperty : NumericInternalProperty<long>
    {

        public LongInternalProperty(long defaultValue = 0L) : base(defaultValue)
        { }

        internal override long Increment()
        {
            long newValue = Value + 1;
            Set(newValue);
            return newValue;
        }

        internal override long Decrement()
        {
            long newValue = Value - 1;
            Set(newValue);
            return newValue;
        }

        internal override long Add(long delta)
        {
            long newValue = Value + delta;
            Set(newValue);
            return newValue;
        }

        internal override long Subtract(long delta)
        {
            long newValue = Value - delta;
            Set(newValue);
            return newValue;
        }

    }

    public class FloatInternalProperty : NumericInternalProperty<float>
    {

        public FloatInternalProperty(float defaultValue = 0f) : base(defaultValue)
        { }

        internal override float Increment()
        {
            float newValue = Value + 1f;
            Set(newValue);
            return newValue;
        }

        internal override float Decrement()
        {
            float newValue = Value - 1f;
            Set(newValue);
            return newValue;
        }

        internal override float Add(float delta)
        {
            float newValue = Value + delta;
            Set(newValue);
            return newValue;
        }

        internal override float Subtract(float delta)
        {
            float newValue = Value - delta;
            Set(newValue);
            return newValue;
        }

    }

    public class DoubleInternalProperty : NumericInternalProperty<double>
    {

        public DoubleInternalProperty(double defaultValue = 0d) : base(defaultValue)
        { }

        internal override double Increment()
        {
            double newValue = Value + 1d;
            Set(newValue);
            return newValue;
        }

        internal override double Decrement()
        {
            double newValue = Value - 1d;
            Set(newValue);
            return newValue;
        }

        internal override double Add(double delta)
        {
            double newValue = Value + delta;
            Set(newValue);
            return newValue;
        }

        internal override double Subtract(double delta)
        {
            double newValue = Value - delta;
            Set(newValue);
            return newValue;
        }

    }

    public class BoolInternalProperty : InternalProperty<bool>
    {
        public BoolInternalProperty(bool defaultValue = false) : base(defaultValue)
        { }

        /// <summary>
        /// Toggles this property
        /// </summary>
        /// <returns></returns>
        internal bool Toggle()
        {
            bool current = Value;
            Set(!Value);
            return current;
        }

    }


}

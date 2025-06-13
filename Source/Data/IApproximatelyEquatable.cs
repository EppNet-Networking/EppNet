///////////////////////////////////////////////////////
/// Filename: IApproximatelyEquatable.cs
/// Date: June 13, 2025
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using System;
using EppNet.Utilities;

namespace EppNet.Data
{

    public interface IApproximatelyEquatable<T> where T : struct
    {
        bool ApproximatelyEquals(T other, float epsilon = FastMath.Epsilon);
    }

    public static class ApproximatelyEquatableExtensions
    {

        public static bool AreApproximatelyEqual<T>(float epsilon = FastMath.Epsilon, params T[] values)
            where T : struct, IComparable, IComparable<T>
        {
            if (values is null || values?.Length <= 1)
                return values is not null;

            if (values[0] is float fa)
            {
                float min = fa;
                float max = fa;

                for (int i = 1; i < values.Length; i++)
                {
                    if (values[i] is not float f)
                        return false;

                    if (f < min) min = f;
                    if (f > max) max = f;
                }

                return (max - min) <= epsilon;
            }

            if (values[0] is double da)
            {
                double min = da;
                double max = da;

                for (int i = 1; i < values.Length; i++)
                {
                    // ;)
                    if (values[i] is not double d)
                        return false;

                    if (d < min) min = d;
                    if (d > max) max = d;
                }

                return (max - min) <= (double) epsilon;
            }

            if (values[0] is decimal dea)
            {
                decimal min = dea;
                decimal max = dea;

                for (int i = 1; i < values.Length; i++)
                {
                    if (values[i] is not decimal d)
                        return false;

                    if (d < min) min = d;
                    if (d > max) max = d;
                }

                return (max - min) <= (decimal)epsilon;
            }

            return false;
        }

        public static bool AreApproximatelyEqual(float a, float b, float c, float epsilon = FastMath.Epsilon)
        {
            float min = MathF.Min(a, MathF.Min(b, c));
            float max = MathF.Max(a, MathF.Max(b, c));
            return (max - min) <= epsilon;
        }

        public static bool ApproximatelyEquals<T>(this T a, T b, float epsilon = FastMath.Epsilon)
            where T : IComparable<T>
        {
            if (a is float fa && b is float fb)
                return MathF.Abs(fa - fb) <= epsilon;

            if (a is double da && b is double db)
                return Math.Abs(da - db) <= epsilon;

            return a.CompareTo(b) == 0;
        }

        public static bool ApproximatelyEquals(this int a, int b, float epsilon = FastMath.Epsilon) =>
            ((float)a).ApproximatelyEquals(b, epsilon);

        public static bool Equals<T>(this T a, T b, float epsilon)
            where T : struct, IApproximatelyEquatable<T> =>
            a.ApproximatelyEquals(b, epsilon);

    }

}

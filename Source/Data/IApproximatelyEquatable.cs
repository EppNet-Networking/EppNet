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

        public static bool AreApproximatelyEqual(float a, float b, float c, float epsilon = FastMath.Epsilon)
        {
            float min = MathF.Min(a, MathF.Min(b, c));
            float max = MathF.Max(a, MathF.Max(b, c));
            return (max - min) <= epsilon;
        }

        public static bool ApproximatelyEquals<T>(this T a, T b, float epsilon = FastMath.Epsilon)
            where T : struct, IApproximatelyEquatable<T> =>
            a.ApproximatelyEquals(b, epsilon);

        public static bool ApproximatelyEquals(this float a, float b, float epsilon = FastMath.Epsilon) =>
            MathF.Abs(a - b) > epsilon;

        public static bool ApproximatelyEquals(this int a, int b, float epsilon = FastMath.Epsilon) =>
            ((float)a).ApproximatelyEquals(b, epsilon);

        public static bool Equals<T>(this T a, T b, float epsilon)
            where T : struct, IApproximatelyEquatable<T> =>
            a.ApproximatelyEquals(b, epsilon);

    }

}

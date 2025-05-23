﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
#if !NETSTANDARD
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;
using System.Runtime.Intrinsics.X86;

using Internal.Runtime.CompilerServices;
#endif

// Some routines inspired by the Stanford Bit Twiddling Hacks by Sean Eron Anderson:
// http://graphics.stanford.edu/~seander/bithacks.html

namespace System.Numerics
{
    /// <summary>
    /// Utility methods for intrinsic bit-twiddling operations.
    /// The methods use hardware intrinsics when available on the underlying platform,
    /// otherwise they use optimized software fallbacks.
    /// </summary>
    public static class BitOperations
    {
        // C# no-alloc optimization that directly wraps the data section of the dll (similar to string constants)
        // https://github.com/dotnet/roslyn/pull/24621

        private static ReadOnlySpan<byte> TrailingZeroCountDeBruijn => new byte[32]
        {
            00, 01, 28, 02, 29, 14, 24, 03,
            30, 22, 20, 15, 25, 17, 04, 08,
            31, 27, 13, 23, 21, 19, 16, 07,
            26, 12, 18, 06, 11, 05, 10, 09
        };

        private static ReadOnlySpan<byte> Log2DeBruijn => new byte[32]
        {
            00, 09, 01, 10, 13, 21, 02, 29,
            11, 14, 16, 18, 22, 25, 03, 30,
            08, 12, 20, 28, 15, 17, 24, 07,
            19, 27, 23, 06, 26, 05, 04, 31
        };

        /// <summary>
        /// Evaluate whether a given integral value is a power of 2.
        /// </summary>
        /// <param name="value">The value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPow2(int value) => (value & (value - 1)) == 0 && value > 0;

        /// <summary>
        /// Evaluate whether a given integral value is a power of 2.
        /// </summary>
        /// <param name="value">The value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPow2(uint value) => (value & (value - 1)) == 0 && value != 0;

        /// <summary>
        /// Evaluate whether a given integral value is a power of 2.
        /// </summary>
        /// <param name="value">The value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPow2(long value) => (value & (value - 1)) == 0 && value > 0;

        /// <summary>
        /// Evaluate whether a given integral value is a power of 2.
        /// </summary>
        /// <param name="value">The value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPow2(ulong value) => (value & (value - 1)) == 0 && value != 0;

        /// <summary>
        /// Evaluate whether a given integral value is a power of 2.
        /// </summary>
        /// <param name="value">The value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPow2(nint value) => (value & (value - 1)) == 0 && value > 0;

        /// <summary>
        /// Evaluate whether a given integral value is a power of 2.
        /// </summary>
        /// <param name="value">The value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPow2(nuint value) => (value & (value - 1)) == 0 && value != 0;

        /// <summary>Round the given integral value up to a power of 2.</summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The smallest power of 2 which is greater than or equal to <paramref name="value"/>.
        /// If <paramref name="value"/> is 0 or the result overflows, returns 0.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint RoundUpToPowerOf2(uint value)
        {
#if !NETSTANDARD
            if (Lzcnt.IsSupported || ArmBase.IsSupported || X86Base.IsSupported)
            {
#if TARGET_64BIT
                return (uint)(0x1_0000_0000ul >> LeadingZeroCount(value - 1));
#else
                int shift = 32 - LeadingZeroCount(value - 1);
                return (1u ^ (uint)(shift >> 5)) << shift;
#endif
            }
#endif

            // Based on https://graphics.stanford.edu/~seander/bithacks.html#RoundUpPowerOf2
            --value;
            value |= value >> 1;
            value |= value >> 2;
            value |= value >> 4;
            value |= value >> 8;
            value |= value >> 16;
            return value + 1;
        }

        /// <summary>
        /// Round the given integral value up to a power of 2.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The smallest power of 2 which is greater than or equal to <paramref name="value"/>.
        /// If <paramref name="value"/> is 0 or the result overflows, returns 0.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong RoundUpToPowerOf2(ulong value)
        {
#if !NETSTANDARD
            if (Lzcnt.X64.IsSupported || ArmBase.Arm64.IsSupported)
            {
                int shift = 64 - LeadingZeroCount(value - 1);
                return (1ul ^ (ulong)(shift >> 6)) << shift;
            }
#endif

            // Based on https://graphics.stanford.edu/~seander/bithacks.html#RoundUpPowerOf2
            --value;
            value |= value >> 1;
            value |= value >> 2;
            value |= value >> 4;
            value |= value >> 8;
            value |= value >> 16;
            value |= value >> 32;
            return value + 1;
        }

        /// <summary>
        /// Round the given integral value up to a power of 2.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The smallest power of 2 which is greater than or equal to <paramref name="value"/>.
        /// If <paramref name="value"/> is 0 or the result overflows, returns 0.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nuint RoundUpToPowerOf2(nuint value)
        {
#if TARGET_64BIT
            return (nuint)RoundUpToPowerOf2((ulong)value);
#else
            return (nuint)RoundUpToPowerOf2((uint)value);
#endif
        }

        /// <summary>
        /// Count the number of leading zero bits in a mask.
        /// Similar in behavior to the x86 instruction LZCNT.
        /// </summary>
        /// <param name="value">The value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int LeadingZeroCount(uint value)
        {
#if !NETSTANDARD
            if (Lzcnt.IsSupported)
            {
                // LZCNT contract is 0->32
                return (int)Lzcnt.LeadingZeroCount(value);
            }

            if (ArmBase.IsSupported)
            {
                return ArmBase.LeadingZeroCount(value);
            }
#endif

            // Unguarded fallback contract is 0->31, BSR contract is 0->undefined
            if (value == 0)
            {
                return 32;
            }

#if !NETSTANDARD
            if (X86Base.IsSupported)
            {
                // LZCNT returns index starting from MSB, whereas BSR gives the index from LSB.
                // 31 ^ BSR here is equivalent to 31 - BSR since the BSR result is always between 0 and 31.
                // This saves an instruction, as subtraction from constant requires either MOV/SUB or NEG/ADD.
                return 31 ^ (int)X86Base.BitScanReverse(value);
            }
#endif

            return 31 ^ Log2SoftwareFallback(value);
        }

        /// <summary>
        /// Count the number of leading zero bits in a mask.
        /// Similar in behavior to the x86 instruction LZCNT.
        /// </summary>
        /// <param name="value">The value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int LeadingZeroCount(ulong value)
        {
#if !NETSTANDARD
            if (Lzcnt.X64.IsSupported)
            {
                // LZCNT contract is 0->64
                return (int)Lzcnt.X64.LeadingZeroCount(value);
            }

            if (ArmBase.Arm64.IsSupported)
            {
                return ArmBase.Arm64.LeadingZeroCount(value);
            }

            if (X86Base.X64.IsSupported)
            {
                // BSR contract is 0->undefined
                return value == 0 ? 64 : 63 ^ (int)X86Base.X64.BitScanReverse(value);
            }
#endif

            uint hi = (uint)(value >> 32);

            if (hi == 0)
            {
                return 32 + LeadingZeroCount((uint)value);
            }

            return LeadingZeroCount(hi);
        }

        /// <summary>
        /// Returns the integer (floor) log of the specified value, base 2.
        /// Note that by convention, input value 0 returns 0 since log(0) is undefined.
        /// </summary>
        /// <param name="value">The value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Log2(uint value)
        {
            // The 0->0 contract is fulfilled by setting the LSB to 1.
            // Log(1) is 0, and setting the LSB for values > 1 does not change the log2 result.
            value |= 1;

#if !NETSTANDARD
            // value    lzcnt   actual  expected
            // ..0001   31      31-31    0
            // ..0010   30      31-30    1
            // 0010..    2      31-2    29
            // 0100..    1      31-1    30
            // 1000..    0      31-0    31
            if (Lzcnt.IsSupported)
            {
                return 31 ^ (int)Lzcnt.LeadingZeroCount(value);
            }

            if (ArmBase.IsSupported)
            {
                return 31 ^ ArmBase.LeadingZeroCount(value);
            }

            // BSR returns the log2 result directly. However BSR is slower than LZCNT
            // on AMD processors, so we leave it as a fallback only.
            if (X86Base.IsSupported)
            {
                return (int)X86Base.BitScanReverse(value);
            }
#endif

            // Fallback contract is 0->0
            return Log2SoftwareFallback(value);
        }

        /// <summary>
        /// Returns the integer (floor) log of the specified value, base 2.
        /// Note that by convention, input value 0 returns 0 since log(0) is undefined.
        /// </summary>
        /// <param name="value">The value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Log2(ulong value)
        {
            value |= 1;

#if !NETSTANDARD
            if (Lzcnt.X64.IsSupported)
            {
                return 63 ^ (int)Lzcnt.X64.LeadingZeroCount(value);
            }

            if (ArmBase.Arm64.IsSupported)
            {
                return 63 ^ ArmBase.Arm64.LeadingZeroCount(value);
            }

            if (X86Base.X64.IsSupported)
            {
                return (int)X86Base.X64.BitScanReverse(value);
            }
#endif

            uint hi = (uint)(value >> 32);

            if (hi == 0)
            {
                return Log2((uint)value);
            }

            return 32 + Log2(hi);
        }

        /// <summary>
        /// Returns the integer (floor) log of the specified value, base 2.
        /// Note that by convention, input value 0 returns 0 since Log(0) is undefined.
        /// Does not directly use any hardware intrinsics, nor does it incur branching.
        /// </summary>
        /// <param name="value">The value.</param>
        private static int Log2SoftwareFallback(uint value)
        {
            // No AggressiveInlining due to large method size
            // Has conventional contract 0->0 (Log(0) is undefined)

            // Fill trailing zeros with ones, eg 00010010 becomes 00011111
            value |= value >> 01;
            value |= value >> 02;
            value |= value >> 04;
            value |= value >> 08;
            value |= value >> 16;

            // uint.MaxValue >> 27 is always in range [0 - 31] so we use Unsafe.AddByteOffset to avoid bounds check
            return Unsafe.AddByteOffset(
                // Using deBruijn sequence, k=2, n=5 (2^5=32) : 0b_0000_0111_1100_0100_1010_1100_1101_1101u
                ref MemoryMarshal.GetReference(Log2DeBruijn),
                // uint|long -> IntPtr cast on 32-bit platforms does expensive overflow checks not needed here
                (IntPtr)(int)((value * 0x07C4ACDDu) >> 27));
        }

        /// <summary>
        /// Returns the population count (number of bits set) of a mask.
        /// Similar in behavior to the x86 instruction POPCNT.
        /// </summary>
        /// <param name="value">The value.</param>
#if !NETSTANDARD
        [Intrinsic]
#endif
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int PopCount(uint value)
        {
#if !NETSTANDARD
            if (Popcnt.IsSupported)
            {
                return (int)Popcnt.PopCount(value);
            }

            if (AdvSimd.Arm64.IsSupported)
            {
                // PopCount works on vector so convert input value to vector first.

                Vector64<uint> input = Vector64.CreateScalar(value);
                Vector64<byte> aggregated = AdvSimd.Arm64.AddAcross(AdvSimd.PopCount(input.AsByte()));
                return aggregated.ToScalar();
            }
#endif

            return SoftwareFallback(value);

            static int SoftwareFallback(uint value)
            {
                const uint c1 = 0x_55555555u;
                const uint c2 = 0x_33333333u;
                const uint c3 = 0x_0F0F0F0Fu;
                const uint c4 = 0x_01010101u;

                value -= (value >> 1) & c1;
                value = (value & c2) + ((value >> 2) & c2);
                value = (((value + (value >> 4)) & c3) * c4) >> 24;

                return (int)value;
            }
        }

        /// <summary>
        /// Returns the population count (number of bits set) of a mask.
        /// Similar in behavior to the x86 instruction POPCNT.
        /// </summary>
        /// <param name="value">The value.</param>
#if !NETSTANDARD
        [Intrinsic]
#endif
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int PopCount(ulong value)
        {
#if !NETSTANDARD
            if (Popcnt.X64.IsSupported)
            {
                return (int)Popcnt.X64.PopCount(value);
            }

            if (AdvSimd.Arm64.IsSupported)
            {
                // PopCount works on vector so convert input value to vector first.
                Vector64<ulong> input = Vector64.Create(value);
                Vector64<byte> aggregated = AdvSimd.Arm64.AddAcross(AdvSimd.PopCount(input.AsByte()));
                return aggregated.ToScalar();
            }
#endif

            if (IntPtr.Size == 4)
                return PopCount((uint)value) // lo
                    + PopCount((uint)(value >> 32)); // hi
            else
                return SoftwareFallback(value);

            static int SoftwareFallback(ulong value)
            {
                const ulong c1 = 0x_55555555_55555555ul;
                const ulong c2 = 0x_33333333_33333333ul;
                const ulong c3 = 0x_0F0F0F0F_0F0F0F0Ful;
                const ulong c4 = 0x_01010101_01010101ul;

                value -= (value >> 1) & c1;
                value = (value & c2) + ((value >> 2) & c2);
                value = (((value + (value >> 4)) & c3) * c4) >> 56;

                return (int)value;
            }

        }

        /// <summary>
        /// Count the number of trailing zero bits in an integer value.
        /// Similar in behavior to the x86 instruction TZCNT.
        /// </summary>
        /// <param name="value">The value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int TrailingZeroCount(int value)
            => TrailingZeroCount((uint)value);

        /// <summary>
        /// Count the number of trailing zero bits in an integer value.
        /// Similar in behavior to the x86 instruction TZCNT.
        /// </summary>
        /// <param name="value">The value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int TrailingZeroCount(uint value)
        {
#if !NETSTANDARD
            if (Bmi1.IsSupported)
            {
                // TZCNT contract is 0->32
                return (int)Bmi1.TrailingZeroCount(value);
            }

            if (ArmBase.IsSupported)
            {
                return ArmBase.LeadingZeroCount(ArmBase.ReverseElementBits(value));
            }
#endif

            // Unguarded fallback contract is 0->0, BSF contract is 0->undefined
            if (value == 0)
            {
                return 32;
            }

#if !NETSTANDARD
            if (X86Base.IsSupported)
            {
                return (int)X86Base.BitScanForward(value);
            }
#endif

            // uint.MaxValue >> 27 is always in range [0 - 31] so we use Unsafe.AddByteOffset to avoid bounds check
            return Unsafe.AddByteOffset(
                // Using deBruijn sequence, k=2, n=5 (2^5=32) : 0b_0000_0111_0111_1100_1011_0101_0011_0001u
                ref MemoryMarshal.GetReference(TrailingZeroCountDeBruijn),
                // uint|long -> IntPtr cast on 32-bit platforms does expensive overflow checks not needed here
                (IntPtr)(int)(((value & (uint)-(int)value) * 0x077CB531u) >> 27)); // Multi-cast mitigates redundant conv.u8
        }

        /// <summary>
        /// Count the number of trailing zero bits in a mask.
        /// Similar in behavior to the x86 instruction TZCNT.
        /// </summary>
        /// <param name="value">The value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int TrailingZeroCount(long value)
            => TrailingZeroCount((ulong)value);

        /// <summary>
        /// Count the number of trailing zero bits in a mask.
        /// Similar in behavior to the x86 instruction TZCNT.
        /// </summary>
        /// <param name="value">The value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int TrailingZeroCount(ulong value)
        {
#if !NETSTANDARD
            if (Bmi1.X64.IsSupported)
            {
                // TZCNT contract is 0->64
                return (int)Bmi1.X64.TrailingZeroCount(value);
            }

            if (ArmBase.Arm64.IsSupported)
            {
                return ArmBase.Arm64.LeadingZeroCount(ArmBase.Arm64.ReverseElementBits(value));
            }

            if (X86Base.X64.IsSupported)
            {
                // BSF contract is 0->undefined
                return value == 0 ? 64 : (int)X86Base.X64.BitScanForward(value);
            }
#endif

            uint lo = (uint)value;

            if (lo == 0)
            {
                return 32 + TrailingZeroCount((uint)(value >> 32));
            }

            return TrailingZeroCount(lo);
        }

        /// <summary>
        /// Rotates the specified value left by the specified number of bits.
        /// Similar in behavior to the x86 instruction ROL.
        /// </summary>
        /// <param name="value">The value to rotate.</param>
        /// <param name="offset">The number of bits to rotate by.
        /// Any value outside the range [0..31] is treated as congruent mod 32.</param>
        /// <returns>The rotated value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint RotateLeft(uint value, int offset)
            => (value << offset) | (value >> (32 - offset));

        /// <summary>
        /// Rotates the specified value left by the specified number of bits.
        /// Similar in behavior to the x86 instruction ROL.
        /// </summary>
        /// <param name="value">The value to rotate.</param>
        /// <param name="offset">The number of bits to rotate by.
        /// Any value outside the range [0..63] is treated as congruent mod 64.</param>
        /// <returns>The rotated value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong RotateLeft(ulong value, int offset)
            => (value << offset) | (value >> (64 - offset));

        /// <summary>
        /// Rotates the specified value right by the specified number of bits.
        /// Similar in behavior to the x86 instruction ROR.
        /// </summary>
        /// <param name="value">The value to rotate.</param>
        /// <param name="offset">The number of bits to rotate by.
        /// Any value outside the range [0..31] is treated as congruent mod 32.</param>
        /// <returns>The rotated value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint RotateRight(uint value, int offset)
            => (value >> offset) | (value << (32 - offset));

        /// <summary>
        /// Rotates the specified value right by the specified number of bits.
        /// Similar in behavior to the x86 instruction ROR.
        /// </summary>
        /// <param name="value">The value to rotate.</param>
        /// <param name="offset">The number of bits to rotate by.
        /// Any value outside the range [0..63] is treated as congruent mod 64.</param>
        /// <returns>The rotated value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong RotateRight(ulong value, int offset)
            => (value >> offset) | (value << (64 - offset));
    }
}
///////////////////////////////////////////////////////
/// Filename: TransportFlags.cs
/// Date: June 10, 2025
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using System;

[Flags]
public enum TransportFlags : byte
{
    None = 0,
    Reliable = 1 << 0,  // Guaranteed delivery
    Unreliable = 1 << 1,  // No guarantee of delivery
    Ordered = 1 << 2,  // Guarantee order
    Unordered = 1 << 3,  // Allow out-of-order arrival
    Instant = 1 << 4,  // Deliver immediately if possible
    Encrypted = 1 << 5,  // Prefer or require encryption
    Critical = 1 << 6,  // Priority packet (QoS hint)
}


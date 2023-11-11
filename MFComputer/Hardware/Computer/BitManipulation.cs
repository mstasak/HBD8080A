namespace HBD8080A.Hardware.Computer;

/// <summary>
/// Static functions to do some basic utility actions, like bit manipulation and downcasts.
/// </summary>
public struct BitManipulation {
    
    /// <summary>
    /// Construct a little endian word from two bytes
    /// </summary>
    /// <param name="h">high-order byte</param>
    /// <param name="l">low-order byte</param>
    /// <returns>A little endian ushort (word) value</returns>
    public static ushort MakeWord(byte h, byte l) {
        return (ushort)((h << 8) | l);
    }

    /// <summary>
    /// Extract high order byte from a little endian word
    /// </summary>
    /// <param name="word"></param>
    /// <returns></returns>
    public static byte HighByte(ushort word) {
        return (byte)((word >> 8) & 0xff);
    }

    /// <summary>
    /// Extract low order byte from a little endian word
    /// </summary>
    /// <param name="word"></param>
    /// <returns></returns>
    public static byte LowByte(ushort word) {
        return (byte)(word & 0xff);
    }

    //private static ushort lowWord(int i) {
    //    return (ushort)(i & 0xff);
    //}

    //private static ushort highWord(int i) {
    //    return (ushort)((i >> 16) & 0xff);
    //}

    /// <summary>
    /// Extract a high,low byte tuple from a little endian word
    /// </summary>
    /// <param name="w"></param>
    /// <returns></returns>
    public static (byte, byte) SplitWord(ushort w) {
        return (HighByte(w), LowByte(w));
    }

    //word version - not useful for this project
    //public static bool ParityEven(ushort d) {
    //    var rslt = true;
    //    while (d != 0)
    //    {
    //        if ((d & 1) != 0) {
    //            rslt = !rslt;
    //        }
    //        d = (ushort)(d >> 1);
    //    }
    //    return rslt;
    //}
    /// <summary>
    /// Determine if the byte parameter d has even parity
    /// </summary>
    /// <param name="d">Value to check parity of</param>
    /// <returns>True if d contains an even number of non-zero bits</returns>
    public static bool ParityEven(byte d) {
        var rslt = true;
        while (d != 0) {
            if ((d & 1) != 0) {
                rslt = !rslt;
            }
            d = (byte)(d >> 1);
        }
        return rslt;
    }

}

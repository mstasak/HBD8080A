namespace MFComputer.Hardware.Computer;

public struct BitManipulation {
    public static ushort MakeWord(byte h, byte l) {
        return (ushort)((h << 8) | l);
    }

    public static byte HighByte(ushort word) {
        return (byte)((word >> 8) & 0xff);
    }

    public static byte LowByte(ushort word) {
        return (byte)(word & 0xff);
    }

    //private static ushort lowWord(int i) {
    //    return (ushort)(i & 0xff);
    //}

    //private static ushort highWord(int i) {
    //    return (ushort)((i >> 16) & 0xff);
    //}

    public static (byte, byte) SplitWord(ushort w) {
        return (HighByte(w), LowByte(w));
    }

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

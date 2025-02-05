using System.Drawing;

public static class PrinterCommands
{
    public static byte[] GetImgPrintCmd(Bitmap img)
    {
        byte[] data = new byte[0];
        data = AddToByteArray(data, CMD_GET_DEV_STATE);
        data = AddToByteArray(data, CMD_SET_QUALITY_200_DPI);
        data = AddToByteArray(data, CmdSetEnergy(0xffff));
        data = AddToByteArray(data, CmdApplyEnergy());
        data = AddToByteArray(data, CMD_LATTICE_START);

        for (int y = 0;y < img.Height; y++)
        {
            List<int> row = new List<int>();
            for (int x = 0; x < img.Width; x++)
            {
                row.Add(img.GetPixel(x, y).ToArgb() & 0xFF);
            }
            data = AddToByteArray(data, CmdPrintRow(row));
        }

        data = AddToByteArray(data, CmdFeedPaper(25));
        data = AddToByteArray(data, CMD_SET_PAPER);
        data = AddToByteArray(data, CMD_SET_PAPER);
        data = AddToByteArray(data, CMD_LATTICE_END);
        data = AddToByteArray(data, CMD_GET_DEV_STATE);

        int index = 1;
        Console.WriteLine("Data to send: ");
        foreach (byte b in data)
        {
            Console.Write("0x{0:X}, ", b);
            if (index % 16 == 0)
            {
                Console.WriteLine();
            }
            index++;
        }
        Console.WriteLine("Data Length: " + data.Length);

        return data;
    }
    private static byte[] AddToByteArray(byte[] array, byte[] data)
    {
        byte[] newArray = new byte[array.Length + data.Length];
        Array.Copy(array, 0, newArray, 0, array.Length);
        Array.Copy(data, 0, newArray, array.Length, data.Length);
        return newArray;
    }

    public const int PRINT_WIDTH = 384;

    private static byte ToUnsignedByte(int val)
    {
        return (byte)(val >= 0 ? val : val & 0xff);
    }

    private static byte[] Bs(IEnumerable<int> lst)
    {
        return lst.Select(val => (byte)(val >= 0 ? val : val + 256)).ToArray();
    }

    private static readonly byte[] CMD_GET_DEV_STATE = Bs(new[] { 81, 120, -93, 0, 1, 0, 0, 0, -1 });
    private static readonly byte[] CMD_SET_QUALITY_200_DPI = Bs(new[] { 81, 120, -92, 0, 1, 0, 50, -98, -1 });
    private static readonly byte[] CMD_GET_DEV_INFO = Bs(new[] { 81, 120, -88, 0, 1, 0, 0, 0, -1 });
    private static readonly byte[] CMD_LATTICE_START = Bs(new[] { 81, 120, -90, 0, 11, 0, -86, 85, 23, 56, 68, 95, 95, 95, 68, 56, 44, -95, -1 });
    private static readonly byte[] CMD_LATTICE_END = Bs(new[] { 81, 120, -90, 0, 11, 0, -86, 85, 23, 0, 0, 0, 0, 0, 0, 0, 23, 17, -1 });
    private static readonly byte[] CMD_SET_PAPER = Bs(new[] { 81, 120, -95, 0, 2, 0, 48, 0, -7, -1 });
    private static readonly byte[] CMD_PRINT_IMG = Bs(new[] { 81, 120, -66, 0, 1, 0, 0, 0, -1 });
    private static readonly byte[] CMD_PRINT_TEXT = Bs(new[] { 81, 120, -66, 0, 1, 0, 1, 7, -1 });

    private static readonly byte[] CHECKSUM_TABLE = Bs(new[]
    {
        0, 7, 14, 9, 28, 27, 18, 21, 56, 63, 54, 49, 36, 35, 42, 45, 112, 119, 126, 121,
        108, 107, 98, 101, 72, 79, 70, 65, 84, 83, 90, 93, -32, -25, -18, -23, -4, -5,
        -14, -11, -40, -33, -42, -47, -60, -61, -54, -51, -112, -105, -98, -103, -116,
        -117, -126, -123, -88, -81, -90, -95, -76, -77, -70, -67, -57, -64, -55, -50,
        -37, -36, -43, -46, -1, -8, -15, -10, -29, -28, -19, -22, -73, -80, -71, -66,
        -85, -84, -91, -94, -113, -120, -127, -122, -109, -108, -99, -102, 39, 32, 41,
        46, 59, 60, 53, 50, 31, 24, 17, 22, 3, 4, 13, 10, 87, 80, 89, 94, 75, 76, 69, 66,
        111, 104, 97, 102, 115, 116, 125, 122, -119, -114, -121, -128, -107, -110, -101,
        -100, -79, -74, -65, -72, -83, -86, -93, -92, -7, -2, -9, -16, -27, -30, -21, -20,
        -63, -58, -49, -56, -35, -38, -45, -44, 105, 110, 103, 96, 117, 114, 123, 124, 81,
        86, 95, 88, 77, 74, 67, 68, 25, 30, 23, 16, 5, 2, 11, 12, 33, 38, 47, 40, 61, 58,
        51, 52, 78, 73, 64, 71, 82, 85, 92, 91, 118, 113, 120, 127, 106, 109, 100, 99, 62,
        57, 48, 55, 34, 37, 44, 43, 6, 1, 8, 15, 26, 29, 20, 19, -82, -87, -96, -89, -78,
        -75, -68, -69, -106, -111, -104, -97, -118, -115, -124, -125, -34, -39, -48, -41,
        -62, -59, -52, -53, -26, -31, -24, -17, -6, -3, -12, -13,
    });

    private static byte ChkSum(byte[] bArr, int i, int i2)
    {
        byte b2 = 0;
        for (int i3 = i; i3 < i + i2; i3++)
        {
            b2 = CHECKSUM_TABLE[(b2 ^ bArr[i3]) & 0xff];
        }
        return b2;
    }

    public static byte[] CmdFeedPaper(int howMuch)
    {
        var bArr = new byte[9];
        bArr[0] = 81;
        bArr[1] = 120;
        bArr[2] = ToUnsignedByte(-67);
        bArr[3] = 0;
        bArr[4] = 1;
        bArr[5] = 0;
        bArr[6] = (byte)(howMuch & 0xff);
        bArr[7] = ChkSum(bArr, 6, 1);
        bArr[8] = ToUnsignedByte(-1);
        return bArr;
    }

    public static byte[] CmdSetEnergy(int val)
    {
        var bArr = Bs(new[] { 81, 120, -81, 0, 2, 0, (val >> 8) & 0xff, val & 0xff, 0, 0xff });
        bArr[8] = ChkSum(bArr, 6, 2);
        return bArr;
    }

    public static byte[] CmdApplyEnergy()
    {
        var bArr = Bs(new[] { 81, 120, -66, 0, 1, 0, 1, 0, 0xff });
        bArr[7] = ChkSum(bArr, 6, 1);
        return bArr;
    }

    private static List<int> EncodeRunLengthRepetition(int n, int val)
    {
        var res = new List<int>();
        while (n > 0x7f)
        {
            res.Add(0x7f | (val << 7));
            n -= 0x7f;
        }
        if (n > 0)
        {
            res.Add((val << 7) | n);
        }
        return res;
    }

    private static List<int> RunLengthEncode(IEnumerable<int> imgRow)
    {
        var res = new List<int>();
        int count = 0;
        int lastVal = -1;
        foreach (var val in imgRow)
        {
            if (val == lastVal)
            {
                count += 1;
            }
            else
            {
                res.AddRange(EncodeRunLengthRepetition(count, lastVal));
                count = 1;
            }
            lastVal = val;
        }
        if (count > 0)
        {
            res.AddRange(EncodeRunLengthRepetition(count, lastVal));
        }
        return res;
    }

    private static List<int> ByteEncode(IList<int> imgRow)
    {
        int BitEncode(int chunkStart, int bitIndex)
        {
            return imgRow[chunkStart + bitIndex] != 0 ? 1 << bitIndex : 0;
        }

        var res = new List<int>();
        for (int chunkStart = 0; chunkStart < imgRow.Count; chunkStart += 8)
        {
            int byteVal = 0;
            for (int bitIndex = 0; bitIndex < 8; bitIndex++)
            {
                byteVal |= BitEncode(chunkStart, bitIndex);
            }
            res.Add(byteVal);
        }
        return res;
    }

    public static byte[] CmdPrintRow(IList<int> imgRow)
    {
        var encodedImg = RunLengthEncode(imgRow);

        if (encodedImg.Count > PRINT_WIDTH / 8)
        {
            encodedImg = ByteEncode(imgRow);
            var bArr = Bs(new[] { 81, 120, -94, 0, encodedImg.Count, 0 }.Concat(encodedImg).Concat(new[] { 0, 0xff }).ToArray());
            bArr[bArr.Length - 2] = ChkSum(bArr, 6, encodedImg.Count);
            return bArr;
        }

        var bArr2 = Bs(new[] { 81, 120, -65, 0, encodedImg.Count, 0 }.Concat(encodedImg).Concat(new[] { 0, 0xff }).ToArray());
        bArr2[bArr2.Length - 2] = ChkSum(bArr2, 6, encodedImg.Count);
        return bArr2;
    }

    public static byte[] CmdsPrintImg(IEnumerable<IList<int>> img, int energy = 0xffff)
    {
        var data = new List<byte>();
        data.AddRange(CMD_GET_DEV_STATE);
        data.AddRange(CMD_SET_QUALITY_200_DPI);
        data.AddRange(CmdSetEnergy(energy));
        data.AddRange(CmdApplyEnergy());
        data.AddRange(CMD_LATTICE_START);

        foreach (var row in img)
        {
            data.AddRange(CmdPrintRow(row));
        }

        data.AddRange(CmdFeedPaper(25));
        data.AddRange(CMD_SET_PAPER);
        data.AddRange(CMD_SET_PAPER);
        data.AddRange(CMD_SET_PAPER);
        data.AddRange(CMD_LATTICE_END);
        data.AddRange(CMD_GET_DEV_STATE);

        return data.ToArray();
    }
}
using System;
using System.Globalization;
using System.Threading;

public class Util
{


    public static void SetNumberCulture(int numberSection = 3)
    {
        // 現在のスレッドのカルチャを書き換える
        CultureInfo ci = (CultureInfo)CultureInfo.CurrentCulture.Clone();
        ci.NumberFormat.NumberGroupSizes = new int[] { numberSection };
        Thread.CurrentThread.CurrentCulture = ci;
    }
    
    public static bool[,] LeftRot(bool[,] mino)
    {
        bool[,] minoMirrored = new bool[mino.GetLength(0), mino.GetLength(1)];
        for (int x = 0; x < mino.GetLength(0); x++)
        {
            for (int y = 0; y < mino.GetLength(1); y++)
            {
                minoMirrored[y, mino.GetLength(0) - x - 1] = mino[x, y];
            }
        }
        return minoMirrored;
    }

    public static bool[,] RightRot(bool[,] mino)
    {
        bool[,] minoMirrored = new bool[mino.GetLength(0), mino.GetLength(1)];
        for (int x = 0; x < mino.GetLength(0); x++)
        {
            for (int y = 0; y < mino.GetLength(1); y++)
            {
                minoMirrored[mino.GetLength(1) - y - 1, x] = mino[x, y];
            }
        }
        return minoMirrored;
    }
}
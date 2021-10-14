﻿using System.Linq;

namespace FFDConverter
{

    class Ulities
    {
        public static string StringBetween(string STR, string FirstString, string LastString)
        {
            string FinalString;
            int Pos1 = STR.IndexOf(FirstString) + FirstString.Length;
            int Pos2 = STR.IndexOf(LastString, Pos1);
            if (Pos2 == -1)
            {
                Pos2 = STR.LastIndexOf(STR.Last()) + 1;
            }
            FinalString = STR[Pos1..Pos2];
            return FinalString;
        }

        public static float intUVmappingFloat(int number, int widthHeightImg)
        {
            return (float)number / (float)widthHeightImg;
        }

        public static int intScaleInt(int number, float Scale)
        {
            return (int)((float)number * Scale);
        }
    }
}

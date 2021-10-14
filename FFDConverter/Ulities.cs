using System.Linq;

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

        public static (float,float,float,float) getUVmapping(int x, int y, int width, int height, int WidthImg, int HeightImg)
        {
            float UVLeft = x / (float)WidthImg;
            float UVTop = y / (float)HeightImg;
            float UVRight = (x + width) / (float)WidthImg;
            float UVBottom = (y + height) / (float)HeightImg;
            return (UVLeft, UVTop, UVRight, UVBottom);
        }

        public static int intScaleInt(int number, float Scale)
        {
            return (int)((float)number * Scale);
        }
    }
}

/*
MIT License

Copyright (c) 2021 eprilx

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

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

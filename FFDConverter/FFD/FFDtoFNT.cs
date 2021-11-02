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

using System;
using System.Collections.Generic;

namespace FFDConverter
{
    public class FFDtoFNT
    {
        public static void ConvertFFDtoFNT(string inputFFD, string outputFNT, string versionGame)
        {
            //get default config
            Config config = DefaultConfig.Get(versionGame);

            //Load FFD
            generalInfoFFD infoFFD = new();
            infoFFD.BitmapName = new();
            List<charDescFFD> FFDDescList = new();
            List<xadvanceDescFFD> FFDxadvanceList = new();
            List<kernelDescFFD> FFDkernelList = new();
            UnknownStuff unkFFD = new();
            FFDFormat.LoadFFD(inputFFD, ref infoFFD, FFDDescList, FFDxadvanceList, FFDkernelList, ref unkFFD, ref config);

            //generalInfoBMF BMFinfo, List< charDescBMF > charDescList, List<kernelDescBMF> kernelDescList)

            //convert infoFFD 2 infoBMF
            generalInfoBMF infoBMF = new();
            infoBMF.setDefault();
            infoBMF.face = infoFFD.fontName;
            infoBMF.charsCount = infoFFD.charsCount;
            infoBMF.kernsCount = infoFFD.kernsCount;
            infoBMF.pages = infoFFD.pagesCount;
            for (int i = 0; i < infoBMF.pages; i++)
            {
                infoBMF.idImg.Add(i);
                infoBMF.fileImg.Add(infoFFD.BitmapName[i]);
            }

            // Get width/height image font from user
            (infoBMF.WidthImg, infoBMF.HeightImg) = getWidthHeightImageFont();


            //convert charDescFFD 2 charDescBMF
            List<charDescBMF> charDescList = new();
            foreach (charDescFFD charFFD in FFDDescList)
            {
                (float x, float y, float width, float height) = Ulities.getPointFromUVmapping(charFFD.UVLeft, charFFD.UVTop, charFFD.UVRight, charFFD.UVBottom, infoBMF.WidthImg, infoBMF.HeightImg);
                float xoffset = Ulities.floatRevScaleInt(charFFD.xoffset, config.scaleXoffset);
                float yoffset = Ulities.floatRevScaleInt(charFFD.yoffset, config.scaleYoffset);

                charDescBMF charBMF = new();
                charBMF.setDefault();
                charBMF.id = charFFD.id;
                charBMF.x = x;
                charBMF.y = y;
                charBMF.width = width;
                charBMF.height = height;
                charBMF.xoffset = xoffset;
                charBMF.yoffset = yoffset;
                charBMF.xadvance = Ulities.floatRevScaleInt(charFFD.xadvance.xadvanceScale, config.scaleXadvance);
                charBMF.page = charFFD.page;
                charDescList.Add(charBMF);
            }

            // convert kernel
            List<kernelDescBMF> kernelDescList = new();
            foreach (kernelDescFFD kernelFFD in FFDkernelList)
            {
                kernelDescList.Add(new kernelDescBMF
                {
                    first = kernelFFD.first,
                    second = kernelFFD.second,
                    amount = (kernelFFD.amountScale / (float)200)
                });
            }
            BMFontFormat.CreateTextBMF(outputFNT, infoBMF, charDescList, kernelDescList);
        }

        private static (int, int) getWidthHeightImageFont()
        {
            int width = 0;
            int height = 0;
            Console.WriteLine("Please input width, height of image fonts:");
            Console.Write("Width = ");
            Int32.TryParse(Console.ReadLine(), out width);
            Console.Write("Height = ");
            Int32.TryParse(Console.ReadLine(), out height);
            return (width, height);
        }
    }
}

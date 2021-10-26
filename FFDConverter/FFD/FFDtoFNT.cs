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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFDConverter
{
    class FFDtoFNT
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
            FFDFormat.LoadFFD(inputFFD, ref infoFFD, FFDDescList, FFDxadvanceList, FFDkernelList, ref unkFFD, config);

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
            int WidthImg = 1024;
            int HeightImg = 1024;

            //convert charDescFFD 2 charDescBMF
            List<charDescBMF> charDescList = new();
            foreach (charDescFFD charFFD in FFDDescList)
            {
                (float x, float y, float width, float height) = Ulities.getPointFromUVmapping(charFFD.UVLeft, charFFD.UVTop, charFFD.UVRight, charFFD.UVBottom, WidthImg, HeightImg);
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
                charBMF.xadvance = charFFD.xadvance.xadvanceScale;
                charBMF.page = charFFD.page;
                charDescList.Add(charBMF);
            }

            // convert kernel
            List<kernelDescBMF> kernelDescList = new();
            BMFontFormat.CreateTextBMF(outputFNT, infoBMF, charDescList, kernelDescList);
        }
    }
}

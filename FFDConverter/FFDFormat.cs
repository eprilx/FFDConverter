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

using Gibbed.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace FFDConverter
{
    class FFDFormat
    {
        public static void LoadFFD(string inputFFD, ref generalInfoFFD infoFFD, List<charDescFFD> FFDDescList, List<xadvanceDescFFD> FFDxadvanceList, ref UnknownStuff_FC5 unkFFD, Config config)
        {
            var input = File.OpenRead(inputFFD);
            input.Position = 0;
            unkFFD.unkHeader1 = input.ReadBytes(config.unkHeader1);
            infoFFD.pagesCount = input.ReadValueU8();
            infoFFD.charsCount = input.ReadValueU16();
            unkFFD.unkHeader2 = input.ReadBytes(config.unkHeader2);

            uint OffsetBitmapNames = input.ReadValueU32(); // OffsetBitmapNames_Real = OffsetBitmapNames + CurrentPosition
            unkFFD.unkHeader3 = input.ReadBytes(config.unkHeader3);
            byte sizeFontName = input.ReadValueU8();
            infoFFD.fontName = input.ReadString(sizeFontName);
            infoFFD.charsCount = input.ReadValueU16();

            int checkNull = input.ReadValueU16();
            input.Position -= 2;
            if (checkNull == 0)
            {
                for (int i = 0; i <= infoFFD.charsCount; i++)
                {
                    input.ReadValueU32(); // = 0
                }
                infoFFD.table1EqualZero = true;
            }
            else
            {
                for (int i = 0; i <= infoFFD.charsCount; i++)
                {
                    input.ReadValueU16(); // = charCount * 2 + 2 + i * 2
                }
                infoFFD.table1EqualZero = false;
            }

            for (int i = 0; i < infoFFD.charsCount; i++)
            {
                infoFFD.table2Value = input.ReadValueS16(); // = 17
            }
            for (int i = 0; i < infoFFD.charsCount; i++)
            {
                input.ReadValueU16(); // = id
            }

            unkFFD.unk6bytes = input.ReadBytes(6); // ascender ???

            for (int i = 0; i < infoFFD.charsCount; i++)
            {
                FFDxadvanceList.Add(new xadvanceDescFFD
                {
                    unk = input.ReadValueU8(),
                    xadvanceScale = input.ReadValueU8()
                });
            }

            for (int i = 0; i < infoFFD.charsCount; i++)
            {
                infoFFD.table5Value = input.ReadValueS16(); // = 8
            }

            int sizeKernel = input.ReadValueU16();
            if (sizeKernel > 0)
                input.ReadBytes(sizeKernel); // data kernel (not use)
            else
                infoFFD.kernsCount = 0;
            
            for (int i = 0; i < infoFFD.pagesCount; i ++)
            {
                infoFFD.BitmapName.Add(input.ReadStringZ());
            }

            for (int i = 0; i < infoFFD.charsCount; i++)
            {
                FFDDescList.Add(new charDescFFD
                {
                    id = input.ReadValueU16(), // = id
                    page = input.ReadValueU8(),
                    UVLeft = input.ReadValueF32(),
                    UVTop = input.ReadValueF32(),
                    UVRight = input.ReadValueF32(),
                    UVBottom = input.ReadValueF32(),
                    xoffset = input.ReadValueS16(),
                    yoffset = input.ReadValueS16(),
                    widthScale = input.ReadValueU16(),
                    heightScale = input.ReadValueU16(),
                });
            }
            unkFFD.unkFooter = input.ReadBytes((int)(input.Length - input.Position));
        }
        public static void CreateFFD(string inputFFD, string inputBMF, string outputFFD, string versionGame)
        {
            //get default config
            Config config = DefaultConfig.Get(versionGame);

            //Load FFD
            generalInfoFFD infoFFD = new();
            infoFFD.CreateListBitmapName();
            List<charDescFFD> FFDDescList = new();
            List<xadvanceDescFFD> FFDxadvanceList = new();
            UnknownStuff_FC5 unkFFD = new();
            
            LoadFFD(inputFFD, ref infoFFD, FFDDescList, FFDxadvanceList, ref unkFFD, config);

            //Load BMFont
            List<charDescBMF> BMFcharDescList = new();
            List<kernelDescBMF> BMFkernelDescList = new();
            generalInfoBMF infoBMF = new();
            (infoBMF, BMFcharDescList, BMFkernelDescList) = BMFontFormat.LoadBMF(inputBMF);

            //Create FFD
            var output = File.Create(outputFFD);

            output.WriteBytes(unkFFD.unkHeader1);
            output.WriteValueU8(infoFFD.pagesCount);
            output.WriteValueU16((ushort)infoBMF.charsCount);
            output.WriteBytes(unkFFD.unkHeader2);

            // Calculate offsetbitmapNames
            int sizefontName = infoFFD.fontName.Length + 1;
            int sizeTable1 = (infoBMF.charsCount * 2) + 2 + 2;
            if (infoFFD.table1EqualZero)
                sizeTable1 = infoBMF.charsCount * 4;
            int sizeTable2 = infoBMF.charsCount * 2;
            int sizeTable3 = infoBMF.charsCount * 2;
            int sizeTable4 = (infoBMF.charsCount * 2) + 6;
            int sizeTable5 = infoBMF.charsCount * 2;
            int sizeTable6 = 2;
            if (infoFFD.kernsCount > 0)
                sizeTable6 = (infoBMF.kernsCount * 6) + 2;

            uint OffsetBitmapNames = (uint)(4 + sizefontName + sizeTable1 + sizeTable2 + sizeTable3 + sizeTable4 + sizeTable5 + sizeTable6);

            output.WriteValueU32(OffsetBitmapNames);
            output.WriteBytes(unkFFD.unkHeader3);
            output.WriteValueU8((byte)infoFFD.fontName.Length);
            output.WriteString(infoFFD.fontName);
            output.WriteValueU16((ushort)infoBMF.charsCount);

            for(int i = 0; i <= infoBMF.charsCount; i++)
            {
                output.WriteValueU16((ushort)((infoBMF.charsCount * 2) + 2 + (i * 2)));
            }

            for (int i = 0; i < infoBMF.charsCount; i++)
            {
                output.WriteValueU16((ushort)infoFFD.table2Value);
            }

            foreach(charDescBMF infoChar in BMFcharDescList)
            {
                output.WriteValueU16((ushort)infoChar.id);
            }

            output.WriteBytes(unkFFD.unk6bytes);

            foreach (charDescBMF infoChar in BMFcharDescList)
            {
                output.WriteByte(0);
                output.WriteByte((byte)(Ulities.intScaleInt(infoChar.xadvance, config.scaleXadvance)));
            }

            for (int i = 0; i < infoBMF.charsCount; i++)
            {
                output.WriteValueU16((ushort)infoFFD.table5Value);
            }

            if (infoFFD.kernsCount > 0)
            {
                output.WriteValueU16((ushort)infoBMF.kernsCount);
                //TODO kernel stuff
            }
            else
            {
                output.WriteValueU16(0);
            }

            for (int i = 0; i < infoFFD.pagesCount; i++)
            {
                output.WriteStringZ(infoFFD.BitmapName[i]);
            }
            
            foreach (charDescBMF infoChar in BMFcharDescList)
            {
                output.WriteValueU16((ushort)infoChar.id);
                output.WriteByte((byte)infoChar.page);
                (float UVleft, float UVtop, float UVright, float UVbottom) = Ulities.getUVmapping(infoChar.x, infoChar.y, infoChar.width, infoChar.height, infoBMF.WidthImg, infoBMF.HeightImg);
                output.WriteValueF32(UVleft);
                output.WriteValueF32(UVtop);
                output.WriteValueF32(UVright);
                output.WriteValueF32(UVbottom);
                output.WriteValueU16((ushort)Ulities.intScaleInt(infoChar.xoffset, config.scaleXoffset));
                output.WriteValueU16((ushort)Ulities.intScaleInt(infoChar.yoffset + config.addCustomYoffset, config.scaleYoffset));
                output.WriteValueU16((ushort)(Ulities.intScaleInt(infoChar.width, config.scaleWidth)));
                output.WriteValueU16((ushort)(Ulities.intScaleInt(infoChar.height, config.scaleHeight)));
            }
            output.WriteBytes(unkFFD.unkFooter);
            output.Close();
        }
    }
}

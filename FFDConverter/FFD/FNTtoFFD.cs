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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFDConverter
{
    class FNTtoFFD
    {
        public static void CreateFFDfromFNT(string inputFFD, string inputBMF, string outputFFD, string versionGame)
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

            //Load BMFont
            List<charDescBMF> BMFcharDescList = new();
            List<kernelDescBMF> BMFkernelDescList = new();
            generalInfoBMF infoBMF = new();
            (infoBMF, BMFcharDescList, BMFkernelDescList) = BMFontFormat.LoadBMF(inputBMF);
            
            //Create FFD
            var output = File.Create(outputFFD);

            // Write Header FFD
            if (config.unkHeaderAC > 0)
            {
                output.WriteBytes(unkFFD.unkHeaderAC);
                uint asizeFFD = GetAsizeFFD(infoFFD, infoBMF, config);
                output.WriteValueU32(asizeFFD);
            }
            output.WriteBytes(unkFFD.unkHeader1);
            output.WriteValueU8(infoFFD.pagesCount);
            output.WriteValueU16((ushort)infoBMF.charsCount);
            output.WriteBytes(unkFFD.unkHeader2);

            uint OffsetBitmapNames = GetOffsetBitmapNames(infoFFD, infoBMF);
            output.WriteValueU32(OffsetBitmapNames);
            output.WriteBytes(unkFFD.unkHeader3);
            output.WriteValueU8((byte)infoFFD.fontName.Length);
            output.WriteString(infoFFD.fontName);
            output.WriteValueU16((ushort)infoBMF.charsCount);

            //Write table 1, 2
            if (!infoFFD.table1EqualZero)
            {
                // write table 1 and 2
                for (int i = 0; i <= infoBMF.charsCount; i++)
                {
                    output.WriteValueU16((ushort)((infoBMF.charsCount * 2) + 2 + (i * 2)));
                }
                for (int i = 0; i < infoBMF.charsCount; i++)
                {
                    output.WriteValueU16((ushort)infoFFD.table2Value);
                }
            }
            else
            {
                // write only table1 = 0, table2 = null
                if (infoFFD.table1Type == "U32")
                {
                    for (int i = 0; i < infoBMF.charsCount; i++)
                    {
                        output.WriteValueU32(0);
                    }
                    // write size table34
                    uint sizeTable34 = (uint)((infoBMF.charsCount * 4) + 4);
                    output.WriteValueU32(sizeTable34);
                }
                else if (infoFFD.table1Type == "U16")
                {
                    for (int i = 0; i < infoBMF.charsCount; i++)
                    {
                        output.WriteValueU16(0);
                    }
                    // write size table34
                    uint sizeTable34 = (uint)((infoBMF.charsCount * 2) + 2);
                    output.WriteValueU16((ushort)sizeTable34);
                }

            }

            foreach (charDescBMF infoChar in BMFcharDescList)
            {
                output.WriteValueU16((ushort)infoChar.id);
            }

            output.WriteBytes(unkFFD.unk6bytes);

            foreach (charDescBMF infoChar in BMFcharDescList)
            {
                output.WriteByte(0);
                output.WriteByte((byte)(Ulities.floatScaleInt(infoChar.xadvance, config.scaleXadvance)));
            }

            for (int i = 0; i < infoBMF.charsCount; i++)
            {
                output.WriteValueU16((ushort)infoFFD.table5Value);
            }

            if (infoFFD.kernsCount > 0 && infoBMF.kernsCount > 0)
            {
                //kernel stuff
                output.WriteValueU16((ushort)infoBMF.kernsCount);
                foreach (kernelDescBMF kerning in BMFkernelDescList)
                {
                    output.WriteValueU16((ushort)kerning.first);
                    output.WriteValueU16((ushort)kerning.second);
                    output.WriteValueS16((short)(kerning.amount * 200));
                }
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
                (float UVleft, float UVtop, float UVright, float UVbottom) = Ulities.getUVmappingFromPoint(infoChar.x, infoChar.y, infoChar.width, infoChar.height, infoBMF.WidthImg, infoBMF.HeightImg);
                output.WriteValueF32(UVleft);
                output.WriteValueF32(UVtop);
                output.WriteValueF32(UVright);
                output.WriteValueF32(UVbottom);
                output.WriteValueU16((ushort)Ulities.floatScaleInt(infoChar.xoffset, config.scaleXoffset));
                output.WriteValueU16((ushort)Ulities.floatScaleInt(infoChar.yoffset + config.addCustomYoffset, config.scaleYoffset));
                if (infoChar.width < 0)
                {
                    output.WriteValueU16((ushort)(Ulities.floatScaleInt(infoChar.height, config.scaleHeight)));
                    output.WriteValueU16((ushort)(Ulities.floatScaleInt(-infoChar.width, config.scaleWidth)));
                }
                else
                {
                    output.WriteValueU16((ushort)(Ulities.floatScaleInt(infoChar.width, config.scaleWidth)));
                    output.WriteValueU16((ushort)(Ulities.floatScaleInt(infoChar.height, config.scaleHeight)));
                }
            }
            output.WriteBytes(unkFFD.unkFooter);
            output.Close();
        }

        private static uint GetOffsetBitmapNames(generalInfoFFD infoFFD, generalInfoBMF infoBMF)
        {
            int sizefontName = infoFFD.fontName.Length + 1;
            int sizeTable1 = (infoBMF.charsCount * 2) + 2 + 2;
            int sizeTable2 = infoBMF.charsCount * 2;
            int sizeTable3 = infoBMF.charsCount * 2;
            if (infoFFD.table1EqualZero)
            {
                if (infoFFD.table1Type == "U32")
                {
                    sizeTable1 = infoBMF.charsCount * 4 + 2;
                    sizeTable2 = 0;
                    sizeTable3 += 4;
                }
                else
                {
                    sizeTable1 = infoBMF.charsCount * 2 + 2;
                    sizeTable2 = 0;
                    sizeTable3 += 2;
                }
            }

            int sizeTable4 = (infoBMF.charsCount * 2) + 6;
            int sizeTable5 = infoBMF.charsCount * 2;
            int sizeTable6 = 2;
            if (infoFFD.kernsCount > 0)
                sizeTable6 = (infoBMF.kernsCount * 6) + 2;
            uint OffsetBitmapNames = (uint)(4 + sizefontName + sizeTable1 + sizeTable2 + sizeTable3 + sizeTable4 + sizeTable5 + sizeTable6);
            return OffsetBitmapNames;
        }

        private static uint GetAsizeFFD(generalInfoFFD infoFFD, generalInfoBMF infoBMF, Config config)
        {
            uint asize = 0;
            asize += (uint)(config.unkHeader1 + 3 + config.unkHeader2 + 4);
            asize += GetOffsetBitmapNames(infoFFD, infoBMF);
            for (int i = 0; i < infoFFD.pagesCount; i++)
            {
                asize += (uint)(infoFFD.BitmapName[i].Length + 1);
            }
            asize += (uint)(infoBMF.charsCount * 27 + (config.unkHeaderAC - 9));
            return asize;
        }
    }
}

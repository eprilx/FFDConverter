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
using System.IO;

namespace FFDConverter
{
    public static class FFDFunction
    {
        public static void ConvertFFDtoFNT(string inputFFD, string outputFNT, string versionGame)
        {
            //get default config
            Config config = DefaultConfig.Get(versionGame);

            //Load FFD
            FFDStruct ffd = FFDFormat.Load(inputFFD, ref config);

            //generalInfoBMF BMFinfo, List< charDescBMF > charDescList, List<kernelDescBMF> kernelDescList)

            // create BMF
            BMFontStruct bmf = new();
            //convert infoFFD 2 infoBMF
            bmf.generalInfo.face = ffd.generalInfo.fontName;
            bmf.generalInfo.charsCount = ffd.generalInfo.charsCount;
            bmf.generalInfo.kernsCount = ffd.generalInfo.kernsCount;
            bmf.generalInfo.pages = ffd.generalInfo.pagesCount;
            for (int i = 0; i < bmf.generalInfo.pages; i++)
            {
                bmf.generalInfo.idImg.Add(i);
                bmf.generalInfo.fileImg.Add(ffd.generalInfo.BitmapName[i]);
            }

            // Get width/height image font from user
            (bmf.generalInfo.WidthImg, bmf.generalInfo.HeightImg) = GetWidthHeightImageFont();

            //convert charDescFFD 2 charDescBMF
            foreach (FFDStruct.charDesc charFFD in ffd.charDescList)
            {
                (float x, float y, float width, float height) = Ulities.getPointFromUVmapping(charFFD.UVLeft, charFFD.UVTop, charFFD.UVRight, charFFD.UVBottom, bmf.generalInfo.WidthImg, bmf.generalInfo.HeightImg);
                float xoffset = Ulities.floatRevScale(charFFD.xoffset, config.scaleXoffset);
                float yoffset = Ulities.floatRevScale(charFFD.yoffset, config.scaleYoffset);

                BMFontStruct.charDesc charBMF = new();
                charBMF.id = charFFD.id;
                charBMF.x = x;
                charBMF.y = y;
                charBMF.width = width;
                charBMF.height = height;
                charBMF.xoffset = xoffset;
                charBMF.yoffset = yoffset;
                charBMF.xadvance = Ulities.floatRevScale(charFFD.xadvance.xadvanceScale, config.scaleXadvance);
                charBMF.page = charFFD.page;
                bmf.charDescList.Add(charBMF);
            }

            // convert kernel
            foreach (FFDStruct.kernelDesc kernelFFD in ffd.kernelDescList)
            {
                bmf.kernelDescList.Add(new BMFontStruct.kernelDesc
                {
                    first = kernelFFD.first,
                    second = kernelFFD.second,
                    amount = kernelFFD.amountScale / (float)200
                });
            }
            BMFontFormat.CreateText(outputFNT, bmf);
        }

        public static void CreateFFDfromFNT(string inputFFD, string inputBMF, string outputFFD, string versionGame)
        {
            //get default config
            Config config = DefaultConfig.Get(versionGame);

            //Load FFD
            FFDStruct ffd = FFDFormat.Load(inputFFD, ref config);

            //Load BMFont
            BMFontStruct bmf = BMFontFormat.Load(inputBMF);
            bmf.SortCharDescListById();

            //Create FFD
            var output = File.Create(outputFFD);

            // Write Header FFD
            if (config.unkHeaderAC > 0)
            {
                output.WriteBytes(ffd.UnknownStuff.unkHeaderAC);
                uint asizeFFD = GetAsizeFFD(ffd.generalInfo, bmf.generalInfo, config);
                output.WriteValueU32(asizeFFD);
            }
            output.WriteBytes(ffd.UnknownStuff.unkHeader1);
            output.WriteValueU8(ffd.generalInfo.pagesCount);
            output.WriteValueU16((ushort)bmf.generalInfo.charsCount);
            output.WriteBytes(ffd.UnknownStuff.unkHeader2);

            uint OffsetBitmapNames = GetOffsetBitmapNames(ffd.generalInfo, bmf.generalInfo);
            output.WriteValueU32(OffsetBitmapNames);
            output.WriteBytes(ffd.UnknownStuff.unkHeader3);
            output.WriteValueU8((byte)ffd.generalInfo.fontName.Length);
            output.WriteString(ffd.generalInfo.fontName);
            output.WriteValueU16((ushort)bmf.generalInfo.charsCount);

            //Write table 1, 2
            if (!ffd.generalInfo.table1EqualZero)
            {
                // write table 1 and 2
                for (int i = 0; i <= bmf.generalInfo.charsCount; i++)
                {
                    output.WriteValueU16((ushort)((bmf.generalInfo.charsCount * 2) + 2 + (i * 2)));
                }
                for (int i = 0; i < bmf.generalInfo.charsCount; i++)
                {
                    output.WriteValueU16((ushort)ffd.generalInfo.table2Value);
                }
            }
            else
            {
                // write only table1 = 0, table2 = null
                if (ffd.generalInfo.table1Type == FFDStruct.general.Type.U32)
                {
                    for (int i = 0; i < bmf.generalInfo.charsCount; i++)
                    {
                        output.WriteValueU32(0);
                    }
                    // write size table34
                    uint sizeTable34 = (uint)((bmf.generalInfo.charsCount * 4) + 4);
                    output.WriteValueU32(sizeTable34);
                }
                else if (ffd.generalInfo.table1Type == FFDStruct.general.Type.U16)
                {
                    for (int i = 0; i < bmf.generalInfo.charsCount; i++)
                    {
                        output.WriteValueU16(0);
                    }
                    // write size table34
                    uint sizeTable34 = (uint)((bmf.generalInfo.charsCount * 2) + 2);
                    output.WriteValueU16((ushort)sizeTable34);
                }
            }

            foreach (BMFontStruct.charDesc infoChar in bmf.charDescList)
            {
                output.WriteValueU16((ushort)infoChar.id);
            }

            output.WriteBytes(ffd.UnknownStuff.unk6bytes);

            foreach (BMFontStruct.charDesc infoChar in bmf.charDescList)
            {
                output.WriteByte(0);
                output.WriteByte((byte)Ulities.floatScaleInt(infoChar.xadvance, config.scaleXadvance));
            }

            for (int i = 0; i < bmf.generalInfo.charsCount; i++)
            {
                output.WriteValueU16((ushort)ffd.generalInfo.table5Value);
            }

            if (ffd.generalInfo.kernsCount > 0 && bmf.generalInfo.kernsCount > 0)
            {
                //kernel stuff
                output.WriteValueU16((ushort)bmf.generalInfo.kernsCount);
                foreach (BMFontStruct.kernelDesc kerning in bmf.kernelDescList)
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

            for (int i = 0; i < ffd.generalInfo.pagesCount; i++)
            {
                output.WriteStringZ(ffd.generalInfo.BitmapName[i]);
            }

            foreach (BMFontStruct.charDesc infoChar in bmf.charDescList)
            {
                output.WriteValueU16((ushort)infoChar.id);
                output.WriteByte((byte)infoChar.page);
                (float UVleft, float UVtop, float UVright, float UVbottom) = Ulities.getUVmappingFromPoint(infoChar.x, infoChar.y, infoChar.width, infoChar.height, bmf.generalInfo.WidthImg, bmf.generalInfo.HeightImg);
                output.WriteValueF32(UVleft);
                output.WriteValueF32(UVtop);
                output.WriteValueF32(UVright);
                output.WriteValueF32(UVbottom);
                output.WriteValueU16((ushort)Ulities.floatScaleInt(infoChar.xoffset, config.scaleXoffset));
                output.WriteValueU16((ushort)Ulities.floatScaleInt(infoChar.yoffset + config.addCustomYoffset, config.scaleYoffset));
                if (infoChar.width < 0)
                {
                    output.WriteValueU16((ushort)(Ulities.floatScaleInt(infoChar.height, config.scaleHeight)));
                    output.WriteValueU16((ushort)Ulities.floatScaleInt(-infoChar.width, config.scaleWidth));
                }
                else
                {
                    output.WriteValueU16((ushort)(Ulities.floatScaleInt(infoChar.width, config.scaleWidth)));
                    output.WriteValueU16((ushort)(Ulities.floatScaleInt(infoChar.height, config.scaleHeight)));
                }
            }
            output.WriteBytes(ffd.UnknownStuff.unkFooter);
            output.Close();
        }

        private static (int, int) GetWidthHeightImageFont()
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

        private static uint GetOffsetBitmapNames(FFDStruct.general infoFFD, BMFontStruct.general infoBMF)
        {
            int sizefontName = infoFFD.fontName.Length + 1;
            int sizeTable1 = (infoBMF.charsCount * 2) + 2 + 2;
            int sizeTable2 = infoBMF.charsCount * 2;
            int sizeTable3 = infoBMF.charsCount * 2;
            if (infoFFD.table1EqualZero)
            {
                if (infoFFD.table1Type == FFDStruct.general.Type.U32)
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

        private static uint GetAsizeFFD(FFDStruct.general infoFFD, BMFontStruct.general infoBMF, Config config)
        {
            uint asize = 0;
            asize += (uint)(config.unkHeader1 + 3 + config.unkHeader2 + 4);
            asize += GetOffsetBitmapNames(infoFFD, infoBMF);
            for (int i = 0; i < infoFFD.pagesCount; i++)
            {
                asize += (uint)(infoFFD.BitmapName[i].Length + 1);
            }
            asize += (uint)((infoBMF.charsCount * 27) + (config.unkHeaderAC - 9));
            return asize;
        }
    }
}

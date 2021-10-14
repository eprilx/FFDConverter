using Gibbed.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace FFDConverter
{
    class FFDFormat
    {
        public static void LoadFFD(FileStream input, ref generalInfoFFD infoFFD, List<charDescFFD> FFDDescList, List<xadvanceDescFFD> FFDxadvanceList, ref UnknownStuff_FC5 unkFFD)
        {
            input.Position = 0;
            unkFFD.unkHeader1 = input.ReadBytes(3);
            infoFFD.pagesCount = input.ReadValueU8();
            infoFFD.charsCount = input.ReadValueU16();
            unkFFD.unkHeader2 = input.ReadBytes(34);

            uint OffsetBitmapNames = input.ReadValueU32(); // OffsetBitmapNames_Real = OffsetBitmapNames + CurrentPosition
            unkFFD.unkHeader3 = input.ReadBytes(4);
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
        public static void CreateFFD(string inputFFD, string inputBMF, string outputFFD )
        {
            //Load FFD
            generalInfoFFD infoFFD = new();
            infoFFD.CreateListBitmapName();
            List<charDescFFD> FFDDescList = new();
            List<xadvanceDescFFD> FFDxadvanceList = new();
            UnknownStuff_FC5 unkFFD = new();
            var input = File.OpenRead(inputFFD);
            LoadFFD(input, ref infoFFD, FFDDescList, FFDxadvanceList, ref unkFFD);

            //Load BMFont
            List<charDescBMF> BMFcharDescList = new();
            List<kernelDescBMF> BMFkernelDescList = new();
            generalInfoBMF infoBMF = new();
            (infoBMF, BMFcharDescList, BMFkernelDescList ) = BMFontFormat.LoadBMF(inputBMF);

            //Create FFD
            var output = File.Create(outputFFD);

            output.WriteBytes(unkFFD.unkHeader1);
            output.WriteValueU8(infoFFD.pagesCount);
            output.WriteValueU16((ushort)infoBMF.charsCount);
            output.WriteBytes(unkFFD.unkHeader2);

            // Calculate offsetbitmapNames
            int sizefontName = infoFFD.fontName.Length + 1;
            int sizeTable1 = infoBMF.charsCount * 2 + 2 + 2;
            if (infoFFD.table1EqualZero)
                sizeTable1 = infoBMF.charsCount * 4;
            int sizeTable2 = infoBMF.charsCount * 2;
            int sizeTable3 = infoBMF.charsCount * 2;
            int sizeTable4 = infoBMF.charsCount * 2 + 6;
            int sizeTable5 = infoBMF.charsCount * 2;
            int sizeTable6 = 2;
            if (infoFFD.kernsCount > 0)
                sizeTable6 = infoBMF.kernsCount * 6 + 2;

            uint OffsetBitmapNames = (uint)(4 + sizefontName + sizeTable1 + sizeTable2 + sizeTable3 + sizeTable4 + sizeTable5 + sizeTable6);

            output.WriteValueU32(OffsetBitmapNames);
            output.WriteBytes(unkFFD.unkHeader3);
            output.WriteValueU8((byte)infoFFD.fontName.Length);
            output.WriteString(infoFFD.fontName);
            output.WriteValueU16((ushort)infoBMF.charsCount);

            for(int i = 0; i <= infoBMF.charsCount; i++)
            {
                output.WriteValueU16((ushort)(infoBMF.charsCount * 2 + 2 + i * 2));
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
                output.WriteByte((byte)(infoChar.xadvance * 2));
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
                output.WriteValueF32(Ulities.intUVmappingFloat(infoChar.x, infoBMF.WidthImg));
                output.WriteValueF32(Ulities.intUVmappingFloat(infoChar.y, infoBMF.HeightImg));
                output.WriteValueF32(Ulities.intUVmappingFloat(infoChar.width, infoBMF.WidthImg));
                output.WriteValueF32(Ulities.intUVmappingFloat(infoChar.height, infoBMF.HeightImg));
                output.WriteValueU16((ushort)infoChar.xoffset);
                output.WriteValueU16((ushort)infoChar.yoffset);
                output.WriteValueU16((ushort)(Ulities.intScaleInt(infoChar.width, 8)));
                output.WriteValueU16((ushort)(Ulities.intScaleInt(infoChar.width, 8)));
            }
            output.WriteBytes(unkFFD.unkFooter);
            output.Close();
        }
    }
}

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
using System.IO;

namespace FFDConverter
{
    public class FFDFormat : FFDStruct
    {
        public static FFDStruct Load(string inputFFD, ref Config config)
        {
            FFDStruct ffd = new();
            var input = File.OpenRead(inputFFD);
            input.Position = 0;

            //Read header
            ReadHeader(input, ref ffd.generalInfo, ref ffd.UnknownStuff, ref config);

            // Read Table1
            ReadTable1FFD(input, ref ffd.generalInfo);

            // if table 1 = zero then table 2 = null
            if (ffd.generalInfo.table1EqualZero)
            {
                if (ffd.generalInfo.table1Type == general.Type.U32)
                {
                    uint sizeTable34 = input.ReadValueU32(); // = charCount * 4 + 4
                }
                else if (ffd.generalInfo.table1Type == general.Type.U16)
                {
                    uint sizeTable34 = input.ReadValueU16(); // = charCount * 2 + 2
                }
            }
            else
            {
                for (int i = 0; i < ffd.generalInfo.charsCount; i++)
                {
                    ffd.generalInfo.table2Value = input.ReadValueS16(); // = 17
                }
            }

            // read table 3 id
            for (int i = 0; i < ffd.generalInfo.charsCount; i++)
            {
                input.ReadValueU16(); // = id
            }

            // read table 4
            ffd.UnknownStuff.unk6bytes = input.ReadBytes(6); // ascender ???

            for (int i = 0; i < ffd.generalInfo.charsCount; i++)
            {
                ffd.xadvanceDescList.Add(new xadvanceDesc
                {
                    unk = input.ReadValueU8(),
                    xadvanceScale = input.ReadValueU8()
                });
            }

            // read table 5
            for (int i = 0; i < ffd.generalInfo.charsCount; i++)
            {
                ffd.generalInfo.table5Value = input.ReadValueS16(); // = 8
            }

            // read table 6 kernel
            ffd.generalInfo.kernsCount = input.ReadValueU16();
            if (ffd.generalInfo.kernsCount > 0)
            {
                // data kernel
                for (int i = 0; i < ffd.generalInfo.kernsCount; i++)
                {
                    ffd.kernelDescList.Add(new kernelDesc
                    {
                        first = input.ReadValueU16(),
                        second = input.ReadValueU16(),
                        amountScale = input.ReadValueS16()
                    });
                }
            }

            //read textures name
            for (int i = 0; i < ffd.generalInfo.pagesCount; i++)
            {
                ffd.generalInfo.BitmapName.Add(input.ReadStringZ());
            }

            // read charDescFFD
            for (int i = 0; i < ffd.generalInfo.charsCount; i++)
            {
                ffd.charDescList.Add(new charDesc
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
                    xadvance = ffd.xadvanceDescList[i],

                });
            }

            // read footer
            ffd.UnknownStuff.unkFooter = input.ReadBytes((int)(input.Length - input.Position));
            input.Close();

            return ffd;
        }

        private static void ReadHeader(FileStream input, ref general infoFFD, ref Unknown unkFFD, ref Config config)
        {
            if (config.unkHeaderAC > 0)
            {
                unkFFD.unkHeaderAC = input.ReadBytes(config.unkHeaderAC);
                uint asizeFFD = input.ReadValueU32();
            }

            // some hard code
            if (config.unkHeader1 == 1)
            {
                input.ReadByte();
                ushort scaleFont = input.ReadValueU16();
                input.Position -= 3;
                unkFFD.unkHeader1 = input.ReadBytes(3);
                switch (config.nameGame)
                {
                    case "Anno 2025 - Anno2025":
                        if (scaleFont > 8000)
                        {
                            config.scaleXoffset = 8;
                            config.scaleYoffset = 8;
                            config.scaleWidth *= 8;
                            config.scaleHeight *= 8;
                        }
                        else
                        {
                            config.scaleXoffset = 1;
                            config.scaleYoffset = 1;
                            config.scaleWidth *= 1;
                            config.scaleHeight *= 1;
                        }
                        break;
                    default:
                        if (scaleFont > 8000)
                        {
                            config.scaleXoffset = 16;
                            config.scaleYoffset = 16;
                            config.scaleWidth *= 16;
                            config.scaleHeight *= 16;
                        }
                        else if (scaleFont > 1000)
                        {
                            config.scaleXoffset = 8;
                            config.scaleYoffset = 8;
                            config.scaleWidth *= 8;
                            config.scaleHeight *= 8;
                        }
                        else
                        {
                            config.scaleXoffset = 1;
                            config.scaleYoffset = 1;
                            config.scaleWidth *= 1;
                            config.scaleHeight *= 1;
                        }
                        break;
                }
            }
            else
            {
                unkFFD.unkHeader1 = input.ReadBytes(config.unkHeader1);
            }

            infoFFD.pagesCount = input.ReadValueU8();
            infoFFD.charsCount = input.ReadValueU16();
            unkFFD.unkHeader2 = input.ReadBytes(config.unkHeader2);

            uint OffsetBitmapNames = input.ReadValueU32(); // OffsetBitmapNames_Real = OffsetBitmapNames + CurrentPosition
            unkFFD.unkHeader3 = input.ReadBytes(config.unkHeader3);
            byte sizeFontName = input.ReadValueU8();
            infoFFD.fontName = input.ReadString(sizeFontName);
        }

        private static void ReadTable1FFD(FileStream input, ref general infoFFD)
        {
            infoFFD.charsCount = input.ReadValueU16();
            long startOffsetTable1 = input.Position;
            int checkNull = input.ReadValueU16();
            input.Position = startOffsetTable1;
            if (checkNull == 0)
            {
                infoFFD.table1EqualZero = true;
                infoFFD.table1Type = general.Type.U32;
                for (int i = 0; i < infoFFD.charsCount; i++)
                {
                    var checkNull2 = input.ReadValueU32(); // = 0
                    if (checkNull2 != 0)
                    {
                        infoFFD.table1Type = general.Type.U16;
                        break;
                    }
                }
            }
            else
            {
                infoFFD.table1EqualZero = false;
                infoFFD.table1Type = general.Type.U16;
            }

            input.Position = startOffsetTable1;

            if (infoFFD.table1Type == general.Type.U16 && !infoFFD.table1EqualZero)
                for (int i = 0; i <= infoFFD.charsCount; i++)
                    input.ReadValueU16(); // = charCount * 2 + 2 + i * 2
            else if (infoFFD.table1Type == general.Type.U16)
                for (int i = 0; i < infoFFD.charsCount; i++)
                    input.ReadValueU16(); // = 0
            else if (infoFFD.table1Type == general.Type.U16)
                for (int i = 0; i < infoFFD.charsCount; i++)
                    input.ReadValueU32(); // = 0
        }
    }
}

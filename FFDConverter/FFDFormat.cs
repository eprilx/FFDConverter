using Gibbed.IO;
using System.Collections.Generic;
using System.IO;

namespace FFDConverter
{
    class FFDFormat
    {
        public static List<charDescFFD> LoadFFD(string inputFFD)
        {
            List<charDescFFD> FFDDescList = new();
            List<xadvanceDescFFD> FFDxadvanceList = new();
            var input = File.OpenRead(inputFFD);

            input.ReadValueU32();
            int charCount = input.ReadValueU16();
            input.ReadBytes(34);

            uint OffsetBitmapNames = input.ReadValueU32(); // OffsetBitmapNames_Real = OffsetBitmapNames + CurrentPosition
            input.ReadValueU32();
            byte sizeFontName = input.ReadValueU8();
            string FontName = input.ReadString(sizeFontName);
            charCount = input.ReadValueU16();

            int checkNull = input.ReadValueU16();
            input.Position -= 2;
            if (checkNull == 0)
            {
                for (int i = 0; i < charCount; i++)
                {
                    input.ReadValueU32(); // = 0
                }
                input.ReadValueU32();
            }
            else
            {
                for (int i = 0; i < charCount; i++)
                {
                    input.ReadValueU16(); // = charCount * 2 + 2 + i * 2
                }
                input.ReadValueU16();
            }

            for (int i = 0; i < charCount; i++)
            {
                input.ReadValueU16(); // = 17
            }
            for (int i = 0; i < charCount; i++)
            {
                int tempid = input.ReadValueU16(); // = id
            }

            input.ReadBytes(6); // ascender ???

            for (int i = 0; i < charCount; i++)
            {
                FFDxadvanceList.Add(new xadvanceDescFFD
                {
                    unk = input.ReadValueU8(),
                    xadvanceScale = input.ReadValueU8()
                });
            }

            for (int i = 0; i < charCount; i++)
            {
                input.ReadValueU16(); // = 8
            }

            int sizeKernel = input.ReadValueU16();
            input.ReadBytes(sizeKernel); // data kernel (not use)
            string BitmapNames = input.ReadStringZ();
            string BitmapNames2 = input.ReadStringZ();

            for (int i = 0; i < charCount; i++)
            {
                FFDDescList.Add(new charDescFFD
                {
                    id = input.ReadValueU16(), // = id
                    zero = input.ReadValueU8(), // = 0
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

            foreach (charDescFFD ffdItem in FFDDescList)
            {
                //Console.WriteLine(String.Format("id={0,-10} uvleft={1,-10} uvtop={2,-10} uvright={3,-10} uvbottom={4,-10}", 
                //    ffdItem.id, ffdItem.UVLeft, ffdItem.UVTop, ffdItem.UVRight, ffdItem.UVBottom));
            }
            return FFDDescList;
        }
    }
}

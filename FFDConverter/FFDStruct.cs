﻿/*
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

using System.Collections.Generic;

namespace FFDConverter
{
    public struct generalInfoFFD
    {
        public string fontName;
        public byte pagesCount;
        public ushort charsCount;
        public bool table1EqualZero;
        public short table2Value;
        public short table5Value;
        public short kernsCount;
        public List<string> BitmapName;

        public void CreateListBitmapName()
        {
            this.BitmapName = new();
        }
    }

    public struct charDescFFD
    {
        public ushort id;
        public byte page;
        public float UVLeft;
        public float UVTop;
        public float UVRight;
        public float UVBottom;
        public short xoffset;
        public short yoffset;
        public ushort widthScale;
        public ushort heightScale;
    }

    public struct xadvanceDescFFD
    {
        public byte unk;
        public byte xadvanceScale;
    }

    public struct kernelDescFFD
    {
        public short first;
        public short second;
        public short amountScale;
    }

    public struct UnknownStuff
    {
        public byte[] unkHeader1;
        public byte[] unkHeader2;
        public byte[] unkHeader3;
        public byte[] unk6bytes;
        public byte[] unkFooter;
    }
}

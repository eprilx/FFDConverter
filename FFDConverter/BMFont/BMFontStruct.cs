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

namespace FFDConverter
{
    public struct generalInfoBMF
    {
        public int lineHeight;
        public int _base;
        public int WidthImg; // width image
        public int HeightImg; // height image

        public string face;
        public int size;
        public int bold;
        public int italic;
        public int charsCount;
        public int kernsCount;
    }

    public struct charDescBMF
    {
        public int id;
        public int x;
        public int y;
        public int width;
        public int height;
        public int xoffset;
        public int yoffset;
        public int xadvance;
        public int page;
        public int chnl;
    }
    public struct kernelDescBMF
    {
        public int first;
        public int second;
        public int amount;
    }
}

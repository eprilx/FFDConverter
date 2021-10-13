﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
    public struct charDescBMF
    {
        public int id;
        public int x;
        public int y;
        public int w;
        public int h;
        public int xoff;
        public int yoff;
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

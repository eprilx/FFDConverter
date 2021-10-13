using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFDConverter
{
    public struct charDescFFD
    {
        public ushort id;
        public byte zero;
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
}

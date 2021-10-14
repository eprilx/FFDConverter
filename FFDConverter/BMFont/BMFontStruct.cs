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

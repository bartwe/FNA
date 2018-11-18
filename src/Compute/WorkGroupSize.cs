namespace FNAExt.Compute {
    public struct WorkGroupSize {
        public int Width;
        public int Height;
        public int Depth;

        public WorkGroupSize(int width, int height, int depth) {
            Width = width;
            Height = height;
            Depth = depth;
        }
    }
}
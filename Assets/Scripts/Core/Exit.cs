namespace ColorBlockJam.Core
{
    public sealed class Exit
    {
        public Side Side;

        public int From;
        public int To;
        public int ColorId;

        public Exit(Side side, int from, int to, int colorId)
        {
            Side = side;
            From = from;
            To = to;
            ColorId = colorId;
        }
    }
}
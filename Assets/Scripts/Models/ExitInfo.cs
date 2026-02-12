using System;
using ColorBlockJam.Core;

namespace ColorBlockJam.Models
{
    [Serializable]
    public struct ExitInfo
    {
        public Side side;

        public int colorId;
        public int from;
        public int to;
    }
}
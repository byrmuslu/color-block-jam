using System;
using UnityEngine;

namespace ColorBlockJam.Models
{
    [Serializable]
    public struct LevelInfo
    {
        public int width;
        public int height;

        public Vector2[] obstacles;

        public BlockInfo[] blockInfos;
        public ExitInfo[] exitInfos;
    }
}
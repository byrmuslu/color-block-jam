using System;
using UnityEngine;

namespace ColorBlockJam.Models
{
    [Serializable]
    public struct BlockInfo
    {
        public int colorId;

        public Vector2[] cells;
    }
}
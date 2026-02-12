using UnityEngine;

namespace ColorBlockJam.Signals
{
    public readonly struct TouchBeginSignal
    {
        public readonly Vector2 TouchPosition;

        public TouchBeginSignal(Vector2 touchPosition)
        {
            TouchPosition = touchPosition;
        }
    }
}
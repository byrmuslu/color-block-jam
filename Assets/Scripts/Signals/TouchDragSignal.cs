using UnityEngine;

namespace ColorBlockJam.Signals
{
    public readonly struct TouchDragSignal
    {
        public readonly Vector2 TouchPosition;

        public TouchDragSignal(Vector2 touchPosition)
        {
            TouchPosition = touchPosition;
        }
    }
}
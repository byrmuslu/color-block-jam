using System;

namespace ColorBlockJam.Core
{
    public enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }

    public static class DirectionExtension
    {
        public static Cell ToDelta(this Direction direction) =>
            direction switch
            {
                Direction.Up => new Cell(0, 1),
                Direction.Down => new Cell(0, -1),
                Direction.Left => new Cell(-1, 0),
                Direction.Right => new Cell(1, 0),
                _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
            };

        public static bool IsHorizontal(this Direction direction) =>
            direction == Direction.Left || direction == Direction.Right;

        public static bool IsVertical(this Direction direction) =>
            direction == Direction.Up || direction == Direction.Down;
    }
}
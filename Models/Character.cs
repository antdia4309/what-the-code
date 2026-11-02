namespace WhatTheCode.Models
{
    public enum Direction { Up, Right, Down, Left }

    public enum MoveResult { Moved, HitEdge, HitWall }

    public class Character
    {
        public int X { get; private set; }
        public int Y { get; private set; }
        public Direction Facing { get; private set; }

        public Character(int startX, int startY, Direction startFacing = Direction.Right)
        {
            Reset(startX, startY, startFacing);
        }

        public static Direction LeftOf(Direction d) => d switch
        {
            Direction.Up    => Direction.Left,
            Direction.Left  => Direction.Down,
            Direction.Down  => Direction.Right,
            _               => Direction.Up
        };

        public static Direction RightOf(Direction d) => d switch
        {
            Direction.Up    => Direction.Right,
            Direction.Right => Direction.Down,
            Direction.Down  => Direction.Left,
            _               => Direction.Up
        };

        public void TurnLeft()  => Facing = LeftOf(Facing);
        public void TurnRight() => Facing = RightOf(Facing);

        /// <summary>The neighbouring cell in a given direction.</summary>
        public Cell CellToward(Direction d) => d switch
        {
            Direction.Up    => new Cell(X, Y - 1),
            Direction.Down  => new Cell(X, Y + 1),
            Direction.Left  => new Cell(X - 1, Y),
            _               => new Cell(X + 1, Y)
        };

        /// <summary>The cell directly in front of the character.</summary>
        public Cell NextCell() => CellToward(Facing);

        /// <summary>Moves one cell forward unless the grid edge or a wall is in the way.</summary>
        public MoveResult MoveForward(Grid grid)
        {
            var next = NextCell();

            if (!grid.IsInBounds(next.X, next.Y)) return MoveResult.HitEdge;
            if (grid.IsWall(next.X, next.Y))      return MoveResult.HitWall;

            X = next.X;
            Y = next.Y;
            return MoveResult.Moved;
        }

        public void Reset(int startX, int startY, Direction startFacing = Direction.Right)
        {
            X = startX;
            Y = startY;
            Facing = startFacing;
        }
    }
}

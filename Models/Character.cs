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

        public void TurnLeft()
        {
            Facing = Facing switch
            {
                Direction.Up    => Direction.Left,
                Direction.Left  => Direction.Down,
                Direction.Down  => Direction.Right,
                Direction.Right => Direction.Up,
                _               => Facing
            };
        }

        public void TurnRight()
        {
            Facing = Facing switch
            {
                Direction.Up    => Direction.Right,
                Direction.Right => Direction.Down,
                Direction.Down  => Direction.Left,
                Direction.Left  => Direction.Up,
                _               => Facing
            };
        }

        /// <summary>The cell directly in front of the character.</summary>
        public Cell NextCell() => Facing switch
        {
            Direction.Up    => new Cell(X, Y - 1),
            Direction.Down  => new Cell(X, Y + 1),
            Direction.Left  => new Cell(X - 1, Y),
            _               => new Cell(X + 1, Y)
        };

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

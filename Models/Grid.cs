namespace WhatTheCode.Models
{
    public readonly record struct Cell(int X, int Y);

    public class Grid
    {
        private readonly HashSet<Cell> _walls;

        public int Columns { get; }
        public int Rows { get; }

        /// <summary>Cells the character has driven over (the trail).</summary>
        public bool[,] Visited { get; }

        public Grid(int columns = 8, int rows = 8, IEnumerable<Cell>? walls = null)
        {
            Columns = columns;
            Rows = rows;
            Visited = new bool[columns, rows];
            _walls = walls is null ? new HashSet<Cell>() : new HashSet<Cell>(walls);
        }

        public bool IsInBounds(int x, int y) =>
            x >= 0 && x < Columns && y >= 0 && y < Rows;

        public bool IsWall(int x, int y) => _walls.Contains(new Cell(x, y));

        /// <summary>True if the bot could drive into this cell.</summary>
        public bool IsOpen(Cell c) => IsInBounds(c.X, c.Y) && !IsWall(c.X, c.Y);

        public void MarkVisited(int x, int y)
        {
            if (IsInBounds(x, y))
                Visited[x, y] = true;
        }

        public void Reset() => Array.Clear(Visited);
    }
}

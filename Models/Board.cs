namespace WhatTheCode.Models
{
    /// <summary>
    /// One playfield: size, walls, where the bot starts and where the flag is.
    /// </summary>
    public class Board
    {
        public required int Columns { get; init; }
        public required int Rows { get; init; }
        public required Cell Start { get; init; }
        public Direction StartFacing { get; init; } = Direction.Right;
        public required Cell Flag { get; init; }
        public IReadOnlyList<Cell> Walls { get; init; } = Array.Empty<Cell>();

        /// <summary>
        /// Builds a board from a text map, one string per row:
        /// <c>.</c> open, <c>#</c> wall, <c>F</c> flag, and <c>&gt; &lt; ^ v</c> the bot facing that way.
        /// </summary>
        public static Board FromMap(params string[] rows)
        {
            if (rows.Length == 0) throw new ArgumentException("A map needs at least one row.", nameof(rows));
            int columns = rows[0].Length;

            var walls = new List<Cell>();
            Cell? start = null, flag = null;
            var facing = Direction.Right;

            for (int y = 0; y < rows.Length; y++)
            {
                if (rows[y].Length != columns)
                    throw new ArgumentException($"Row {y} is {rows[y].Length} wide; expected {columns}.", nameof(rows));

                for (int x = 0; x < columns; x++)
                {
                    switch (rows[y][x])
                    {
                        case '.': break;
                        case '#': walls.Add(new Cell(x, y)); break;
                        case 'F': flag = new Cell(x, y); break;
                        case '>': start = new Cell(x, y); facing = Direction.Right; break;
                        case '<': start = new Cell(x, y); facing = Direction.Left; break;
                        case '^': start = new Cell(x, y); facing = Direction.Up; break;
                        case 'v': start = new Cell(x, y); facing = Direction.Down; break;
                        default: throw new ArgumentException($"Unknown map character '{rows[y][x]}' at ({x}, {y}).", nameof(rows));
                    }
                }
            }

            return new Board
            {
                Columns = columns,
                Rows = rows.Length,
                Start = start ?? throw new ArgumentException("Map has no start (> < ^ v).", nameof(rows)),
                StartFacing = facing,
                Flag = flag ?? throw new ArgumentException("Map has no flag (F).", nameof(rows)),
                Walls = walls
            };
        }
    }
}

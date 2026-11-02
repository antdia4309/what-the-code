namespace WhatTheCode.Models
{
    public class Level
    {
        public required int Number { get; init; }
        public required string Title { get; init; }

        /// <summary>One-line nudge shown while playing.</summary>
        public required string Tip { get; init; }

        /// <summary>Shown when a level introduces a new block or idea.</summary>
        public string? NewIdea { get; init; }

        public required int Columns { get; init; }
        public required int Rows { get; init; }
        public required Cell Start { get; init; }
        public Direction StartFacing { get; init; } = Direction.Right;
        public required Cell Flag { get; init; }
        public IReadOnlyList<Cell> Walls { get; init; } = Array.Empty<Cell>();

        /// <summary>Fewest blocks needed for three stars.</summary>
        public required int Par { get; init; }

        public required IReadOnlyList<BlockKind> Blocks { get; init; }

        public int StarsFor(int blocksUsed) =>
            blocksUsed <= Par ? 3 : blocksUsed <= Par + 2 ? 2 : 1;
    }
}

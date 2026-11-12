namespace WhatTheCode.Models
{
    public class Level
    {
        /// <summary>Position in the built-in levels. Player-made levels use 0.</summary>
        public required int Number { get; init; }

        /// <summary>True for levels made in the level editor (shared by link or being tested).</summary>
        public bool IsCustom => Number == 0;
        public required string Title { get; init; }

        /// <summary>One-line nudge shown while playing.</summary>
        public required string Tip { get; init; }

        /// <summary>Shown when a level introduces a new block or idea.</summary>
        public Concept? Concept { get; init; }

        /// <summary>
        /// The boards this level is played on. With more than one, a single program
        /// has to reach the flag on every board.
        /// </summary>
        public required IReadOnlyList<Board> Boards { get; init; }

        /// <summary>Fewest blocks needed for three stars.</summary>
        public required int Par { get; init; }

        public required IReadOnlyList<BlockKind> Blocks { get; init; }

        /// <summary>Two hints, gentle first. Neither should give the whole answer away.</summary>
        public required IReadOnlyList<string> Hints { get; init; }

        /// <summary>
        /// Builds a fresh copy of a par solution. Shown to players who ask for the answer,
        /// and checked by the tests against every board.
        /// </summary>
        public required Func<List<Instruction>> Solution { get; init; }

        public int StarsFor(int blocksUsed) =>
            blocksUsed <= Par ? 3 : blocksUsed <= Par + 2 ? 2 : 1;
    }
}

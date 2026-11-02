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

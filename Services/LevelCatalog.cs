using WhatTheCode.Models;

namespace WhatTheCode.Services
{
    /// <summary>
    /// Hand-built levels. Each one introduces at most one new idea:
    /// sequence → turning → planning around walls → loops → patterns → nested loops.
    /// </summary>
    public static class LevelCatalog
    {
        private static readonly BlockKind[] MoveOnly  = { BlockKind.MoveForward };
        private static readonly BlockKind[] WithTurns = { BlockKind.MoveForward, BlockKind.TurnLeft, BlockKind.TurnRight };
        private static readonly BlockKind[] MoveLoop  = { BlockKind.MoveForward, BlockKind.Repeat };
        private static readonly BlockKind[] AllBlocks = { BlockKind.MoveForward, BlockKind.TurnLeft, BlockKind.TurnRight, BlockKind.Repeat };

        public static IReadOnlyList<Level> All { get; } = new List<Level>
        {
            new()
            {
                Number = 1, Title = "First steps",
                NewIdea = "A program is a list of steps. Your bot runs them in order, top to bottom.",
                Tip = "Add Move forward blocks until the bot would land on the flag, then press Run.",
                Columns = 5, Rows = 5, Start = new(0, 2), Flag = new(3, 2),
                Par = 3, Blocks = MoveOnly
            },
            new()
            {
                Number = 2, Title = "Turn the corner",
                NewIdea = "Turning doesn't move the bot. It only changes which way the bot faces.",
                Tip = "Watch the pointer on the bot. Turns are from the bot's point of view, not yours.",
                Columns = 5, Rows = 5, Start = new(0, 4), Flag = new(3, 1),
                Par = 7, Blocks = WithTurns
            },
            new()
            {
                Number = 3, Title = "Around the wall",
                Tip = "Walls stop the bot cold. Trace your path with a finger before you build.",
                Columns = 6, Rows = 5, Start = new(0, 2), Flag = new(4, 2),
                Walls = new Cell[] { new(2, 2), new(2, 3), new(2, 4) },
                Par = 10, Blocks = WithTurns
            },
            new()
            {
                Number = 4, Title = "Long hallway",
                NewIdea = "New block: Repeat. Blocks inside it run again and again, so you don't have to copy them.",
                Tip = "Seven moves in a row? Put one Move inside a Repeat and set the count.",
                Columns = 8, Rows = 3, Start = new(0, 1), Flag = new(7, 1),
                Walls = Enumerable.Range(0, 8).SelectMany(x => new Cell[] { new(x, 0), new(x, 2) }).ToArray(),
                Par = 2, Blocks = MoveLoop
            },
            new()
            {
                Number = 5, Title = "Staircase",
                Tip = "Spot the pattern: move, turn, move, turn. One stair's worth of blocks goes inside the Repeat.",
                Columns = 6, Rows = 6, Start = new(0, 5), Flag = new(5, 0),
                Walls = new Cell[]
                {
                    new(2, 5), new(3, 4), new(4, 3), new(5, 2),
                    new(0, 4), new(1, 3), new(2, 2), new(3, 1), new(4, 0)
                },
                Par = 5, Blocks = AllBlocks
            },
            new()
            {
                Number = 6, Title = "Big steps",
                NewIdea = "Loops can go inside loops. Tap a Repeat's inner area to add blocks there.",
                Tip = "Each stair is two moves across and two moves up. Can a Repeat handle each pair?",
                Columns = 7, Rows = 7, Start = new(0, 6), Flag = new(6, 0),
                Walls = new Cell[]
                {
                    new(1, 5), new(1, 4), new(3, 3), new(3, 2), new(5, 1), new(5, 0),
                    new(3, 6), new(3, 5), new(5, 4), new(5, 3)
                },
                Par = 7, Blocks = AllBlocks
            }
        };
    }
}

using WhatTheCode.Models;
using WhatTheCode.Services;
using static WhatTheCode.Tests.Blocks;

namespace WhatTheCode.Tests;

public class LevelTests
{
    private static readonly Instruction[] WallFollower =
    {
        Until(
            If(Condition.PathRight,
                Do(Right(), Move()),
                If(Condition.PathAhead, Do(Move()), Left())))
    };

    /// <summary>A known par solution for every level. Adding a level means adding one here.</summary>
    private static readonly Dictionary<int, Instruction[]> Solutions = new()
    {
        [1]  = new[] { Move(), Move(), Move() },
        [2]  = new[] { Move(), Move(), Move(), Left(), Move(), Move(), Move() },
        [3]  = new[] { Move(), Left(), Move(), Right(), Move(), Move(), Right(), Move(), Left(), Move() },
        [4]  = new[] { Repeat(7, Move()) },
        [5]  = new[] { Repeat(5, Move(), Left(), Move(), Right()) },
        [6]  = new[] { Repeat(3, Repeat(2, Move()), Left(), Repeat(2, Move()), Right()) },
        [7]  = new[] { Repeat(9, If(Condition.PathAhead, Do(Move()), Left())) },
        [8]  = new[] { Until(If(Condition.PathAhead, Do(Move()), Left())) },
        [9]  = new[] { Until(If(Condition.PathAhead, Do(Move()), If(Condition.PathLeft, Do(Left()), Right()))) },
        [10] = new[] { Until(If(Condition.PathAhead, Do(Move()), Right())) },
        [11] = WallFollower,
        [12] = WallFollower,
    };

    public static TheoryData<int> LevelNumbers()
    {
        var data = new TheoryData<int>();
        foreach (var level in LevelCatalog.All) data.Add(level.Number);
        return data;
    }

    private static Level Get(int number) => LevelCatalog.All.Single(l => l.Number == number);

    [Theory, MemberData(nameof(LevelNumbers))]
    public void Reference_solution_reaches_the_flag_on_every_board(int number)
    {
        var level = Get(number);
        Assert.True(Solutions.ContainsKey(number), $"Level {number} needs a reference solution in LevelTests.");

        for (int i = 0; i < level.Boards.Count; i++)
            Assert.Equal(RunResult.ReachedFlag, Play.Run(level.Boards[i], Solutions[number]));
    }

    [Theory, MemberData(nameof(LevelNumbers))]
    public void Par_is_achievable(int number)
    {
        Assert.Equal(Get(number).Par, Instruction.CountBlocks(Solutions[number]));
    }

    [Theory, MemberData(nameof(LevelNumbers))]
    public void Reference_solution_only_uses_blocks_the_level_offers(int number)
    {
        var offered = Get(number).Blocks.ToHashSet();
        foreach (var kind in Flatten(Solutions[number]).Select(i => i.Kind))
            Assert.Contains(kind, offered);
    }

    [Theory, MemberData(nameof(LevelNumbers))]
    public void Flag_is_reachable_on_every_board(int number)
    {
        foreach (var board in Get(number).Boards)
        {
            var open = new HashSet<Cell>();
            var walls = board.Walls.ToHashSet();
            for (int y = 0; y < board.Rows; y++)
                for (int x = 0; x < board.Columns; x++)
                    if (!walls.Contains(new Cell(x, y))) open.Add(new Cell(x, y));

            var seen = new HashSet<Cell> { board.Start };
            var queue = new Queue<Cell>(seen);
            while (queue.Count > 0)
            {
                var c = queue.Dequeue();
                foreach (var n in new[] { c with { X = c.X + 1 }, c with { X = c.X - 1 }, c with { Y = c.Y + 1 }, c with { Y = c.Y - 1 } })
                    if (open.Contains(n) && seen.Add(n)) queue.Enqueue(n);
            }

            Assert.Contains(board.Flag, seen);
        }
    }

    [Fact]
    public void Level_numbers_run_from_one_without_gaps()
    {
        Assert.Equal(Enumerable.Range(1, LevelCatalog.All.Count), LevelCatalog.All.Select(l => l.Number));
    }

    [Fact]
    public void Wall_following_level_cannot_be_solved_with_the_simpler_level_9_strategy()
    {
        // Level 11 should teach something new, not reward reusing the last answer.
        var level11 = Get(11);
        Assert.Contains(level11.Boards, b => Play.Run(b, Solutions[9]) != RunResult.ReachedFlag);
    }

    private static IEnumerable<Instruction> Flatten(IEnumerable<Instruction> program) =>
        program.SelectMany(i => new[] { i }.Concat(i.ChildLists.SelectMany(Flatten)));
}

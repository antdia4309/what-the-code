using WhatTheCode.Models;
using WhatTheCode.Services;
using static WhatTheCode.Services.Build;

namespace WhatTheCode.Tests;

public class LevelTests
{
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
        foreach (var board in level.Boards)
            Assert.Equal(RunResult.ReachedFlag, Play.Run(board, level.Solution()));
    }

    [Theory, MemberData(nameof(LevelNumbers))]
    public void Par_is_achievable(int number)
    {
        Assert.Equal(Get(number).Par, Instruction.CountBlocks(Get(number).Solution()));
    }

    [Theory, MemberData(nameof(LevelNumbers))]
    public void Reference_solution_only_uses_blocks_the_level_offers(int number)
    {
        var offered = Get(number).Blocks.ToHashSet();
        foreach (var kind in Flatten(Get(number).Solution()).Select(i => i.Kind))
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

    [Theory, MemberData(nameof(LevelNumbers))]
    public void Every_level_has_two_distinct_hints(int number)
    {
        var hints = Get(number).Hints;
        Assert.Equal(2, hints.Count);
        Assert.All(hints, h => Assert.False(string.IsNullOrWhiteSpace(h)));
        Assert.NotEqual(hints[0], hints[1]);
    }

    [Theory, MemberData(nameof(LevelNumbers))]
    public void Answer_trail_reaches_the_flag_on_every_board(int number)
    {
        var level = Get(number);
        foreach (var board in level.Boards)
            Assert.Contains(board.Flag, SolutionPath.Trace(board, level.Solution()));
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
        Assert.Contains(level11.Boards, b => Play.Run(b, Get(9).Solution()) != RunResult.ReachedFlag);
    }

    private static IEnumerable<Instruction> Flatten(IEnumerable<Instruction> program) =>
        program.SelectMany(i => new[] { i }.Concat(i.ChildLists.SelectMany(Flatten)));
}

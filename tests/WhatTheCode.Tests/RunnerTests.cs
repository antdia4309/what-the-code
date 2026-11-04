using WhatTheCode.Models;
using WhatTheCode.Services;
using static WhatTheCode.Services.Build;

namespace WhatTheCode.Tests;

public class RunnerTests
{
    private static readonly Board OpenRoom = Board.FromMap(
        ">...",
        "....",
        "...F");

    [Fact]
    public void Driving_off_the_board_is_a_crash()
    {
        Assert.Equal(RunResult.Crashed, Play.Run(OpenRoom, new[] { Repeat(5, Move()) }));
    }

    [Fact]
    public void Repeat_until_flag_with_nothing_inside_still_ends()
    {
        Assert.Equal(RunResult.RanOutOfSteps, Play.Run(OpenRoom, new[] { Until() }));
    }

    [Fact]
    public void Spinning_forever_ends_instead_of_hanging()
    {
        Assert.Equal(RunResult.RanOutOfSteps, Play.Run(OpenRoom, new[] { Until(Left()) }));
    }

    [Fact]
    public void Ending_somewhere_else_is_stopped_short()
    {
        Assert.Equal(RunResult.StoppedShort, Play.Run(OpenRoom, new[] { Move() }));
    }

    [Theory]
    [InlineData(Condition.PathAhead, true)]   // facing right into open floor
    [InlineData(Condition.PathLeft, false)]   // left of right-facing is up: off the board
    [InlineData(Condition.PathRight, true)]   // right of right-facing is down: open
    public void If_checks_are_relative_to_where_the_bot_faces(Condition condition, bool expected)
    {
        var session = new GameSession(OpenRoom);
        Assert.Equal(expected, session.Check(condition));
    }

    [Fact]
    public void If_takes_the_otherwise_branch_when_blocked()
    {
        var board = Board.FromMap(">#", ".F");
        var session = new GameSession(board);
        var steps = new ProgramRunner(session)
            .Run(new[] { If(Condition.PathAhead, Do(Move()), Right()) })
            .ToList();

        Assert.Equal(2, steps.Count);
        Assert.Equal(StepKind.Check, steps[0].Kind);
        Assert.False(steps[0].Answer);
        Assert.Equal(BlockKind.TurnRight, steps[1].Action);
    }

    [Fact]
    public void Repeat_until_reports_each_check_including_the_last()
    {
        var board = Board.FromMap(">.F");
        var session = new GameSession(board);
        var loop = Until(Move());
        var checks = new List<bool>();

        foreach (var step in new ProgramRunner(session).Run(new[] { loop }))
        {
            if (step.IsAction) session.Apply(step.Action);
            else checks.Add(step.Answer);
        }

        Assert.Equal(new[] { false, false, true }, checks);
    }

    [Fact]
    public void Loop_passes_count_up_and_clear_when_the_loop_ends()
    {
        var board = Board.FromMap(">..F");
        var session = new GameSession(board);
        var loop = Repeat(3, Move());
        var runner = new ProgramRunner(session);
        var seen = new List<int>();

        foreach (var step in runner.Run(new[] { loop }))
        {
            seen.Add(runner.Passes[loop.Id]);
            session.Apply(step.Action);
        }

        Assert.Equal(new[] { 1, 2, 3 }, seen);
        Assert.False(runner.Passes.ContainsKey(loop.Id));
    }

    [Fact]
    public void Inner_loop_restarts_its_count_on_each_outer_pass()
    {
        var board = Board.FromMap(">...", "....", "....", "...F");
        var session = new GameSession(board);
        var inner = Repeat(2, Left());
        var outer = Repeat(2, inner);
        var runner = new ProgramRunner(session);
        var innerPasses = new List<int>();

        foreach (var step in runner.Run(new[] { outer }))
        {
            innerPasses.Add(runner.Passes[inner.Id]);
            session.Apply(step.Action);
        }

        Assert.Equal(new[] { 1, 2, 1, 2 }, innerPasses);
    }
}

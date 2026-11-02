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

        Assert.Single(steps);
        Assert.Equal(BlockKind.TurnRight, steps[0].Action);
    }
}

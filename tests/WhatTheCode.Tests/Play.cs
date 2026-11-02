using WhatTheCode.Models;
using WhatTheCode.Services;

namespace WhatTheCode.Tests;

internal enum RunResult { ReachedFlag, StoppedShort, Crashed, RanOutOfSteps }

internal static class Play
{
    /// <summary>Runs a program on one board exactly the way the game does.</summary>
    public static RunResult Run(Board board, IReadOnlyList<Instruction> program)
    {
        var session = new GameSession(board);
        var runner = new ProgramRunner(session);

        foreach (var step in runner.Run(program))
        {
            var outcome = session.Apply(step.Action);
            if (outcome is StepOutcome.HitWall or StepOutcome.HitEdge)
                return RunResult.Crashed;
        }

        if (runner.RanOutOfSteps) return RunResult.RanOutOfSteps;
        return session.OnFlag ? RunResult.ReachedFlag : RunResult.StoppedShort;
    }
}

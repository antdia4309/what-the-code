using WhatTheCode.Models;

namespace WhatTheCode.Services
{
    /// <summary>Works out which cells a program drives through on a board, for the "show the answer" trail.</summary>
    public static class SolutionPath
    {
        public static HashSet<Cell> Trace(Board board, IReadOnlyList<Instruction> program)
        {
            var session = new GameSession(board);
            var cells = new HashSet<Cell> { board.Start };

            foreach (var step in new ProgramRunner(session).Run(program))
            {
                var outcome = session.Apply(step.Action);
                if (outcome is StepOutcome.HitWall or StepOutcome.HitEdge) break;
                cells.Add(new Cell(session.Bot.X, session.Bot.Y));
            }

            return cells;
        }
    }
}

using WhatTheCode.Models;

namespace WhatTheCode.Services
{
    /// <summary>One action the bot will take, plus the block that caused it (for highlighting).</summary>
    public record ProgramStep(Instruction Source, BlockKind Action);

    /// <summary>
    /// Interprets a program one action at a time. Because If and Repeat-until look at the board,
    /// the caller must apply each yielded step to the session before asking for the next one.
    /// </summary>
    public class ProgramRunner
    {
        /// <summary>Most actions (moves and turns) one run may take.</summary>
        public const int MaxActions = 300;

        /// <summary>Most loop passes and checks one run may take, so an empty or stuck loop still ends.</summary>
        public const int MaxTicks = 2000;

        private readonly GameSession _session;
        private int _actions;
        private int _ticks;

        public ProgramRunner(GameSession session) => _session = session;

        /// <summary>True if the run stopped because it hit <see cref="MaxActions"/> or <see cref="MaxTicks"/>.</summary>
        public bool RanOutOfSteps { get; private set; }

        public IEnumerable<ProgramStep> Run(IReadOnlyList<Instruction> program)
        {
            _actions = 0;
            _ticks = 0;
            RanOutOfSteps = false;
            return Execute(program);
        }

        private IEnumerable<ProgramStep> Execute(IEnumerable<Instruction> instructions)
        {
            foreach (var instruction in instructions)
            {
                if (RanOutOfSteps) yield break;

                switch (instruction.Kind)
                {
                    case BlockKind.Repeat:
                        for (int i = 0; i < instruction.Count && Tick(); i++)
                            foreach (var step in Execute(instruction.Body))
                                yield return step;
                        break;

                    case BlockKind.RepeatUntilFlag:
                        while (!_session.OnFlag && Tick())
                            foreach (var step in Execute(instruction.Body))
                                yield return step;
                        break;

                    case BlockKind.If:
                        if (!Tick()) yield break;
                        var branch = _session.Check(instruction.Condition) ? instruction.Body : instruction.ElseBody;
                        foreach (var step in Execute(branch))
                            yield return step;
                        break;

                    default:
                        if (++_actions > MaxActions)
                        {
                            RanOutOfSteps = true;
                            yield break;
                        }
                        yield return new ProgramStep(instruction, instruction.Kind);
                        break;
                }
            }
        }

        private bool Tick()
        {
            if (++_ticks > MaxTicks) RanOutOfSteps = true;
            return !RanOutOfSteps;
        }
    }
}

using WhatTheCode.Models;

namespace WhatTheCode.Services
{
    public enum StepKind
    {
        /// <summary>The bot moves or turns.</summary>
        Action,

        /// <summary>An If or Repeat-until looked at the board and got an answer. Nothing moves.</summary>
        Check
    }

    /// <summary>
    /// One thing that happens while a program runs, tied to the block that caused it.
    /// Actions change the board; checks are shown so players can see why a branch was taken.
    /// </summary>
    public record ProgramStep(Instruction Source, StepKind Kind, BlockKind Action = default, bool Answer = false)
    {
        public bool IsAction => Kind == StepKind.Action;

        public static ProgramStep Act(Instruction source) => new(source, StepKind.Action, source.Kind);
        public static ProgramStep Checked(Instruction source, bool answer) => new(source, StepKind.Check, Answer: answer);
    }

    /// <summary>
    /// Interprets a program one step at a time. Because If and Repeat-until look at the board,
    /// the caller must apply each action to the session before asking for the next step.
    /// </summary>
    public class ProgramRunner
    {
        /// <summary>Most actions (moves and turns) one run may take.</summary>
        public const int MaxActions = 300;

        /// <summary>Most loop passes and checks one run may take, so an empty or stuck loop still ends.</summary>
        public const int MaxTicks = 2000;

        private readonly GameSession _session;
        private readonly Dictionary<Guid, int> _passes = new();
        private int _actions;
        private int _ticks;

        public ProgramRunner(GameSession session) => _session = session;

        /// <summary>True if the run stopped because it hit <see cref="MaxActions"/> or <see cref="MaxTicks"/>.</summary>
        public bool RanOutOfSteps { get; private set; }

        /// <summary>
        /// Which pass each running loop is on (1 = first time through). A loop is only listed while it's running.
        /// </summary>
        public IReadOnlyDictionary<Guid, int> Passes => _passes;

        public IEnumerable<ProgramStep> Run(IReadOnlyList<Instruction> program)
        {
            _actions = 0;
            _ticks = 0;
            _passes.Clear();
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
                        for (int pass = 1; pass <= instruction.Count && Tick(); pass++)
                        {
                            _passes[instruction.Id] = pass;
                            foreach (var step in Execute(instruction.Body))
                                yield return step;
                        }
                        _passes.Remove(instruction.Id);
                        break;

                    case BlockKind.RepeatUntilFlag:
                        for (int pass = 1; Tick(); pass++)
                        {
                            var atFlag = _session.OnFlag;
                            yield return ProgramStep.Checked(instruction, atFlag);
                            if (atFlag) break;

                            _passes[instruction.Id] = pass;
                            foreach (var step in Execute(instruction.Body))
                                yield return step;
                        }
                        _passes.Remove(instruction.Id);
                        break;

                    case BlockKind.If:
                        if (!Tick()) yield break;
                        var answer = _session.Check(instruction.Condition);
                        yield return ProgramStep.Checked(instruction, answer);
                        foreach (var step in Execute(answer ? instruction.Body : instruction.ElseBody))
                            yield return step;
                        break;

                    default:
                        if (++_actions > MaxActions)
                        {
                            RanOutOfSteps = true;
                            yield break;
                        }
                        yield return ProgramStep.Act(instruction);
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

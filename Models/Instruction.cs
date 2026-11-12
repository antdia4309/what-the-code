namespace WhatTheCode.Models
{
    public enum BlockKind { MoveForward, TurnLeft, TurnRight, Repeat, RepeatUntilFlag, If }

    /// <summary>What an If block checks, relative to the way the bot is facing.</summary>
    public enum Condition { PathAhead, PathLeft, PathRight }

    /// <summary>
    /// One block in the player's program. Loops hold child blocks in <see cref="Body"/>;
    /// If blocks also use <see cref="ElseBody"/> for the "otherwise" branch.
    /// </summary>
    public class Instruction
    {
        public const int MinRepeat = 2;
        public const int MaxRepeat = 9;

        public Guid Id { get; } = Guid.NewGuid();
        public BlockKind Kind { get; }
        public int Count { get; private set; } = 3;
        public Condition Condition { get; private set; } = Condition.PathAhead;
        public List<Instruction> Body { get; } = new();
        public List<Instruction> ElseBody { get; } = new();

        public Instruction(BlockKind kind) => Kind = kind;

        public bool IsContainer => Kind is BlockKind.Repeat or BlockKind.RepeatUntilFlag or BlockKind.If;

        /// <summary>The child lists this block owns (empty for simple blocks).</summary>
        public IEnumerable<List<Instruction>> ChildLists
        {
            get
            {
                if (IsContainer) yield return Body;
                if (Kind == BlockKind.If) yield return ElseBody;
            }
        }

        /// <summary>Finds a block by id anywhere in a program, including inside loops and ifs.</summary>
        public static Instruction? Find(IEnumerable<Instruction> program, Guid id)
        {
            foreach (var block in program)
            {
                if (block.Id == id) return block;
                foreach (var child in block.ChildLists)
                    if (Find(child, id) is { } found) return found;
            }
            return null;
        }

        public void ChangeCount(int delta) =>
            Count = Math.Clamp(Count + delta, MinRepeat, MaxRepeat);

        public void CycleCondition() =>
            Condition = Condition switch
            {
                Condition.PathAhead => Condition.PathLeft,
                Condition.PathLeft  => Condition.PathRight,
                _                   => Condition.PathAhead
            };

        public string Label => LabelFor(Kind);

        public static string LabelFor(BlockKind kind) => kind switch
        {
            BlockKind.MoveForward     => "Move forward",
            BlockKind.TurnLeft        => "Turn left",
            BlockKind.TurnRight       => "Turn right",
            BlockKind.Repeat          => "Repeat",
            BlockKind.RepeatUntilFlag => "Repeat until flag",
            BlockKind.If              => "If",
            _                         => kind.ToString()
        };

        public string ConditionLabel => Condition switch
        {
            Condition.PathAhead => "path ahead",
            Condition.PathLeft  => "path to the left",
            _                   => "path to the right"
        };

        /// <summary>Total blocks in a program, counting containers and everything inside them.</summary>
        public static int CountBlocks(IEnumerable<Instruction> program) =>
            program.Sum(i => 1 + i.ChildLists.Sum(CountBlocks));
    }
}

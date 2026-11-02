namespace WhatTheCode.Models
{
    public enum BlockKind { MoveForward, TurnLeft, TurnRight, Repeat }

    /// <summary>
    /// One block in the player's program. Repeat blocks hold a body of child blocks.
    /// </summary>
    public class Instruction
    {
        public const int MinRepeat = 2;
        public const int MaxRepeat = 9;

        public Guid Id { get; } = Guid.NewGuid();
        public BlockKind Kind { get; }
        public int Count { get; private set; } = 3;
        public List<Instruction> Body { get; } = new();

        public bool IsLoop => Kind == BlockKind.Repeat;

        public Instruction(BlockKind kind) => Kind = kind;

        public void ChangeCount(int delta) =>
            Count = Math.Clamp(Count + delta, MinRepeat, MaxRepeat);

        public string Label => Kind switch
        {
            BlockKind.MoveForward => "Move forward",
            BlockKind.TurnLeft    => "Turn left",
            BlockKind.TurnRight   => "Turn right",
            BlockKind.Repeat      => "Repeat",
            _                     => Kind.ToString()
        };

        /// <summary>Total blocks in a program, counting loops and everything inside them.</summary>
        public static int CountBlocks(IEnumerable<Instruction> program) =>
            program.Sum(i => 1 + CountBlocks(i.Body));
    }
}

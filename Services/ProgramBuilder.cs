using WhatTheCode.Models;

namespace WhatTheCode.Services
{
    /// <summary>
    /// Shorthand for building programs in code, so reference solutions read like the blocks a player builds.
    /// </summary>
    public static class Build
    {
        public static Instruction Move() => new(BlockKind.MoveForward);
        public static Instruction Left() => new(BlockKind.TurnLeft);
        public static Instruction Right() => new(BlockKind.TurnRight);

        public static Instruction Repeat(int times, params Instruction[] body)
        {
            var loop = new Instruction(BlockKind.Repeat);
            loop.ChangeCount(times - loop.Count);
            loop.Body.AddRange(body);
            return loop;
        }

        public static Instruction Until(params Instruction[] body)
        {
            var loop = new Instruction(BlockKind.RepeatUntilFlag);
            loop.Body.AddRange(body);
            return loop;
        }

        public static Instruction If(Condition condition, Instruction[] then, params Instruction[] otherwise)
        {
            var block = new Instruction(BlockKind.If);
            while (block.Condition != condition) block.CycleCondition();
            block.Body.AddRange(then);
            block.ElseBody.AddRange(otherwise);
            return block;
        }

        /// <summary>Groups blocks for an If's "yes" part.</summary>
        public static Instruction[] Do(params Instruction[] blocks) => blocks;

        public static List<Instruction> Steps(params Instruction[] blocks) => blocks.ToList();
    }
}

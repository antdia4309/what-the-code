using WhatTheCode.Models;

namespace WhatTheCode.Tests;

/// <summary>Tiny helpers so test programs read like the blocks a player would build.</summary>
internal static class Blocks
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

    public static Instruction[] Do(params Instruction[] blocks) => blocks;
}

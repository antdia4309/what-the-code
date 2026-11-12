using WhatTheCode.Models;
using WhatTheCode.Services;
using static WhatTheCode.Services.Build;

namespace WhatTheCode.Tests;

public class SnippetTests
{
    public static TheoryData<BlockKind> AllKinds()
    {
        var data = new TheoryData<BlockKind>();
        foreach (var kind in Enum.GetValues<BlockKind>()) data.Add(kind);
        return data;
    }

    [Theory, MemberData(nameof(AllKinds))]
    public void Every_block_has_a_snippet_with_a_cursor_and_a_label(BlockKind kind)
    {
        Assert.Contains(CodeSnippets.Cursor, CodeSnippets.Insert(kind));
        Assert.False(string.IsNullOrWhiteSpace(CodeSnippets.Label(kind)));
    }

    [Theory, MemberData(nameof(AllKinds))]
    public void Snippet_parses_into_that_block(BlockKind kind)
    {
        // Loops and ifs need something inside to be valid, so fill the cursor with a move.
        var text = CodeSnippets.Insert(kind).Replace(CodeSnippets.Cursor, kind is BlockKind.MoveForward or BlockKind.TurnLeft or BlockKind.TurnRight ? "" : "bot.MoveForward();");
        var result = CodeParser.Parse(text);
        Assert.True(result.Ok, result.Error?.Message);
        Assert.Equal(kind, Assert.Single(result.Program!).Kind);
    }

    [Fact]
    public void ToText_is_empty_for_an_empty_program()
    {
        Assert.Equal("", CodeGenerator.ToText(new List<Instruction>()));
    }

    [Fact]
    public void ToText_round_trips_through_the_parser()
    {
        var program = new List<Instruction> { Until(If(Condition.PathRight, Do(Right(), Move()), Do(Move()))) };
        var parsed = CodeParser.Parse(CodeGenerator.ToText(program));
        Assert.True(parsed.Ok, parsed.Error?.Message);
        Assert.Equal(CodeGenerator.ToText(program), CodeGenerator.ToText(parsed.Program!));
    }

    [Fact]
    public void Find_reaches_blocks_nested_in_loops_and_otherwise_parts()
    {
        var deep = Move();
        var program = new List<Instruction> { Move(), Repeat(2, If(Condition.PathAhead, Do(Left()), Do(deep))) };
        Assert.Same(deep, Instruction.Find(program, deep.Id));
        Assert.Null(Instruction.Find(program, Guid.NewGuid()));
    }
}

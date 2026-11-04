using WhatTheCode.Models;
using WhatTheCode.Services;
using static WhatTheCode.Services.Build;

namespace WhatTheCode.Tests;

public class CodeAndMapTests
{
    private static string Code(params Instruction[] program) =>
        string.Join("\n", CodeGenerator.Generate(program).Select(l => l.Text));

    [Fact]
    public void Generates_while_if_else()
    {
        var code = Code(Until(If(Condition.PathAhead, Do(Move()), Left())));

        Assert.Equal(
            """
            while (!bot.AtFlag())
            {
                if (bot.PathAhead())
                {
                    bot.MoveForward();
                }
                else
                {
                    bot.TurnLeft();
                }
            }
            """.Replace("\r\n", "\n"), code);
    }

    [Fact]
    public void Only_statement_lines_are_marked_for_highlighting()
    {
        var lines = CodeGenerator.Generate(new[] { Until(If(Condition.PathAhead, Do(Move()), Left())) });
        var statements = lines.Where(l => l.IsStatement).Select(l => l.Text.Trim());

        Assert.Equal(new[] { "while (!bot.AtFlag())", "if (bot.PathAhead())", "bot.MoveForward();", "bot.TurnLeft();" }, statements);
    }

    [Fact]
    public void Leaves_out_empty_else()
    {
        Assert.DoesNotContain("else", Code(If(Condition.PathLeft, Do(Left()))));
    }

    [Fact]
    public void Nested_counted_loops_use_different_variables()
    {
        var code = Code(Repeat(2, Repeat(3, Move())));
        Assert.Contains("for (int i = 0; i < 2; i++)", code);
        Assert.Contains("for (int j = 0; j < 3; j++)", code);
    }

    [Fact]
    public void Map_reads_start_facing_flag_and_walls()
    {
        var board = Board.FromMap(
            "v#",
            ".F");

        Assert.Equal(new Cell(0, 0), board.Start);
        Assert.Equal(Direction.Down, board.StartFacing);
        Assert.Equal(new Cell(1, 1), board.Flag);
        Assert.Equal(new[] { new Cell(1, 0) }, board.Walls);
    }

    [Fact]
    public void Map_rejects_ragged_rows()
    {
        Assert.Throws<ArgumentException>(() => Board.FromMap(">..", ".F"));
    }

    [Fact]
    public void Block_count_includes_both_if_branches()
    {
        Assert.Equal(4, Instruction.CountBlocks(new[] { If(Condition.PathAhead, Do(Move()), Left(), Right()) }));
    }
}

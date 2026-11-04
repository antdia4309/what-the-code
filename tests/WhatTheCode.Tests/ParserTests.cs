using WhatTheCode.Models;
using WhatTheCode.Services;
using static WhatTheCode.Services.Build;

namespace WhatTheCode.Tests;

public class ParserTests
{
    private static string Code(IReadOnlyList<Instruction> program) =>
        string.Join("\n", CodeGenerator.Generate(program).Select(l => l.Text));

    private static List<Instruction> ParseOk(string source)
    {
        var result = CodeParser.Parse(source);
        Assert.True(result.Ok, result.Error?.Message);
        return result.Program!;
    }

    private static CodeError ParseFails(string source)
    {
        var result = CodeParser.Parse(source);
        Assert.False(result.Ok, "Expected a parse error.");
        return result.Error!;
    }

    public static TheoryData<int> LevelNumbers()
    {
        var data = new TheoryData<int>();
        foreach (var level in LevelCatalog.All) data.Add(level.Number);
        return data;
    }

    // ---------- Round trips: blocks → C# → blocks must be lossless ----------

    [Theory, MemberData(nameof(LevelNumbers))]
    public void Every_level_answer_survives_a_round_trip_through_code(int number)
    {
        var level = LevelCatalog.All.Single(l => l.Number == number);
        var original = level.Solution();
        var reparsed = ParseOk(Code(original));

        Assert.Equal(Code(original), Code(reparsed));
        Assert.Equal(Instruction.CountBlocks(original), Instruction.CountBlocks(reparsed));
    }

    [Theory, MemberData(nameof(LevelNumbers))]
    public void Typed_answers_still_solve_every_board(int number)
    {
        var level = LevelCatalog.All.Single(l => l.Number == number);
        var typed = ParseOk(Code(level.Solution()));

        foreach (var board in level.Boards)
            Assert.Equal(RunResult.ReachedFlag, Play.Run(board, typed));
    }

    // ---------- Things real C# allows ----------

    [Fact]
    public void Understands_else_if_as_an_if_inside_otherwise()
    {
        var program = ParseOk("""
            while (!bot.AtFlag())
            {
                if (bot.PathRight()) { bot.TurnRight(); bot.MoveForward(); }
                else if (bot.PathAhead()) bot.MoveForward();
                else bot.TurnLeft();
            }
            """);

        var outer = program[0].Body[0];
        Assert.Equal(BlockKind.If, outer.Kind);
        Assert.Equal(Condition.PathRight, outer.Condition);
        Assert.Equal(Condition.PathAhead, outer.ElseBody[0].Condition);
        Assert.Equal(7, Instruction.CountBlocks(program));
    }

    [Fact]
    public void Allows_comments_same_line_braces_and_prefix_increment()
    {
        var program = ParseOk("""
            // go to the flag
            for (int step = 0; step < 4; ++step) { /* one square */ bot.MoveForward(); }
            """);

        Assert.Equal(BlockKind.Repeat, program[0].Kind);
        Assert.Equal(4, program[0].Count);
    }

    [Fact]
    public void Records_which_line_each_block_is_on()
    {
        var result = CodeParser.Parse("bot.MoveForward();\n\nbot.TurnLeft();");
        Assert.Equal(1, result.Lines[result.Program![0].Id]);
        Assert.Equal(3, result.Lines[result.Program[1].Id]);
    }

    [Fact]
    public void Empty_code_is_an_empty_program()
    {
        Assert.Empty(ParseOk("  // nothing yet\n"));
    }

    // ---------- Beginner mistakes get clear, located messages ----------

    [Theory]
    [InlineData("bot.MoveForward()", 1, "Add a ;")]
    [InlineData("bot.moveForward();", 1, "case-sensitive: write MoveForward")]
    [InlineData("bot.MoveFoward();", 1, "Did you mean MoveForward?")]
    [InlineData("MoveForward();", 1, "Start with bot.")]
    [InlineData("bot.MoveForward;", 1, "Add () after MoveForward")]
    [InlineData("For (int i = 0; i < 3; i++) { bot.MoveForward(); }", 1, "case-sensitive: write for")]
    [InlineData("for (int i = 0; i <= 3; i++) { bot.MoveForward(); }", 1, "Use < instead of <=")]
    [InlineData("for (int i = 0; i < 12; i++) { bot.MoveForward(); }", 1, "from 2 to 9")]
    [InlineData("for (int i = 0; i < 3; i++);\n{ bot.MoveForward(); }", 1, "Remove the ;")]
    [InlineData("while (bot.AtFlag()) { bot.MoveForward(); }", 1, "Add ! before bot.AtFlag()")]
    [InlineData("bot.PathAhead();", 1, "is a question, not a move")]
    [InlineData("if (bot.MoveForward()) { }", 1, "is a move, not a question")]
    [InlineData("if (!bot.PathAhead()) { }", 1, "can't use ! yet")]
    [InlineData("else { bot.TurnLeft(); }", 1, "no if right before it")]
    [InlineData("bot.MoveForward();\n}", 2, "extra }")]
    [InlineData("while (!bot.AtFlag())\n{\n    bot.MoveForward();\n", 2, "never closed")]
    [InlineData("bot.MoveForward();\nbot.Jump();", 2, "doesn't know Jump")]
    [InlineData("bot.MoveForward(); \"hi\"", 1, "quote marks")]
    public void Explains_common_mistakes(string source, int line, string expected)
    {
        var error = ParseFails(source);
        Assert.Equal(line, error.Line);
        Assert.Contains(expected, error.Message);
    }

    [Fact]
    public void Too_deep_nesting_is_reported_on_the_offending_line()
    {
        var error = ParseFails("""
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    for (int k = 0; k < 2; k++)
                    {
                        for (int m = 0; m < 2; m++) { bot.MoveForward(); }
                    }
                }
            }
            """);
        Assert.Equal(7, error.Line);
        Assert.Contains("too many", error.Message);
    }

    [Fact]
    public void Level_rules_reject_blocks_the_level_does_not_offer()
    {
        var result = CodeParser.Parse("bot.MoveForward();\nif (bot.PathAhead()) { bot.MoveForward(); }");
        var level1 = LevelCatalog.All[0];
        var error = CodeParser.CheckAllowed(result.Program!, level1.Blocks.ToHashSet(), result.Lines, level1.Number);

        Assert.NotNull(error);
        Assert.Equal(2, error!.Line);
        Assert.Contains("if checks", error.Message);
    }
}

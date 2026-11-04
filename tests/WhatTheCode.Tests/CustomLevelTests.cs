using WhatTheCode.Models;
using WhatTheCode.Services;

namespace WhatTheCode.Tests;

public class CustomLevelTests
{
    private static readonly BlockKind[] Moves = { BlockKind.MoveForward, BlockKind.TurnLeft, BlockKind.TurnRight };

    private static CustomLevelSpec Spec(int? par = 3, string title = "Zig", params string[] map) =>
        new(title, map.Length > 0 ? map : new[] { ">..F", "....", "...." }, Moves, par);

    [Fact]
    public void Share_codes_round_trip()
    {
        var spec = Spec(par: 4, title: "Über zig 🚀");
        var back = CustomLevels.Decode(CustomLevels.Encode(spec), out var problem);

        Assert.Null(problem);
        Assert.NotNull(back);
        Assert.Equal(spec.Title, back!.Title);
        Assert.Equal(spec.Map, back.Map);
        Assert.Equal(spec.Par, back.Par);
        Assert.Equal(spec.Blocks.OrderBy(b => b), back.Blocks.OrderBy(b => b));
    }

    [Fact]
    public void Share_codes_are_url_safe()
    {
        var code = CustomLevels.Encode(Spec());
        Assert.DoesNotContain('+', code);
        Assert.DoesNotContain('/', code);
        Assert.DoesNotContain('=', code);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not-a-level")]
    [InlineData("eyJ2IjoxfQ")] // {"v":1} — valid JSON, missing everything
    public void Broken_codes_are_rejected_with_a_reason(string? code)
    {
        Assert.Null(CustomLevels.Decode(code, out var problem));
        Assert.False(string.IsNullOrEmpty(problem));
    }

    [Fact]
    public void Unsolved_levels_cannot_be_shared()
    {
        Assert.Throws<InvalidOperationException>(() => CustomLevels.Encode(Spec(par: null)));
    }

    [Fact]
    public void Walled_off_flags_are_caught()
    {
        var walled = new[] { ">#F", "##.", "..." };
        Assert.Contains("can't reach", CustomLevels.Validate(Spec(map: walled), requirePar: true));
    }

    [Fact]
    public void Missing_bot_flag_or_move_block_are_caught()
    {
        Assert.Contains("Place the bot", CustomLevels.Validate(Spec(map: new[] { "...F", "....", "...." }), true));
        Assert.Contains("Place the flag", CustomLevels.Validate(Spec(map: new[] { ">...", "....", "...." }), true));
        var noMove = new CustomLevelSpec("x", new[] { ">..F", "....", "...." }, new[] { BlockKind.TurnLeft }, 3);
        Assert.Contains("Move forward", CustomLevels.Validate(noMove, true));
    }

    [Fact]
    public void Draft_levels_are_playable_before_they_have_a_par()
    {
        var level = CustomLevels.ToLevel(Spec(par: null));
        Assert.True(level.IsCustom);
        Assert.Single(level.Boards);
        Assert.Equal(RunResult.ReachedFlag, Play.Run(level.Boards[0], Build.Steps(Build.Move(), Build.Move(), Build.Move())));
    }

    [Fact]
    public void Badge_ids_are_unique()
    {
        Assert.Equal(BadgeCatalog.All.Count, BadgeCatalog.All.Select(b => b.Id).Distinct().Count());
    }
}

using WhatTheCode.Models;
using WhatTheCode.Services;

namespace WhatTheCode.Tests;

public class ConceptTests
{
    public static TheoryData<int> LevelsWithConcepts()
    {
        var data = new TheoryData<int>();
        foreach (var level in LevelCatalog.All.Where(l => l.Concept is not null)) data.Add(level.Number);
        return data;
    }

    private static Concept ConceptFor(int number) => LevelCatalog.All.Single(l => l.Number == number).Concept!;

    [Fact]
    public void Every_level_that_introduces_a_block_explains_it()
    {
        var seen = new HashSet<BlockKind>();
        foreach (var level in LevelCatalog.All)
        {
            var fresh = level.Blocks.Where(seen.Add).ToList();
            if (fresh.Count > 0)
                Assert.True(level.Concept is not null, $"Level {level.Number} introduces {string.Join(", ", fresh)} but has no concept card.");
        }
    }

    [Theory, MemberData(nameof(LevelsWithConcepts))]
    public void Concept_has_every_part_filled_in(int number)
    {
        var c = ConceptFor(number);
        Assert.False(string.IsNullOrWhiteSpace(c.Name));
        Assert.False(string.IsNullOrWhiteSpace(c.Line));
        Assert.False(string.IsNullOrWhiteSpace(c.More));
        Assert.False(string.IsNullOrWhiteSpace(c.RealWorld));
        Assert.False(string.IsNullOrWhiteSpace(c.Code));
    }

    [Theory, MemberData(nameof(LevelsWithConcepts))]
    public void Concept_stays_short(int number)
    {
        // The line is read before every attempt; the rest is opt-in but should still fit on a phone.
        var c = ConceptFor(number);
        Assert.True(c.Name.Length <= 30, $"Name is {c.Name.Length} characters.");
        Assert.True(c.Line.Length <= 200, $"Line is {c.Line.Length} characters.");
        Assert.True(c.More.Length <= 420, $"More is {c.More.Length} characters.");
        Assert.True(c.RealWorld.Length <= 200, $"Real-world example is {c.RealWorld.Length} characters.");
    }

    [Theory, MemberData(nameof(LevelsWithConcepts))]
    public void Concept_code_is_code_the_game_understands(int number)
    {
        var result = CodeParser.Parse(ConceptFor(number).Code);
        Assert.True(result.Error is null, $"Level {number}: line {result.Error?.Line}: {result.Error?.Message}");
    }

    [Theory, MemberData(nameof(LevelsWithConcepts))]
    public void Concept_code_only_uses_ideas_the_player_has_met(int number)
    {
        // An example that uses a block the player hasn't unlocked yet would confuse more than it helps.
        var level = LevelCatalog.All.Single(l => l.Number == number);
        var program = CodeParser.Parse(level.Concept!.Code).Program!;
        var known = LevelCatalog.All.Where(l => l.Number <= number).SelectMany(l => l.Blocks).ToHashSet();
        Assert.Null(CodeParser.CheckAllowed(program, known, new Dictionary<Guid, int>(), number));
    }
}

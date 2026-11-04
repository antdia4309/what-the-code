using Microsoft.JSInterop;
using System.Text.Json;

namespace WhatTheCode.Services
{
    public record Badge(string Id, string Emoji, string Name, string HowToEarn);

    /// <summary>
    /// Badges reward good habits as much as progress: solving without help, debugging, typing code,
    /// sticking with a hard level, and making levels for others.
    /// </summary>
    public static class BadgeCatalog
    {
        public const string FirstFlag = "first-flag";
        public const string PartOne = "part-one";
        public const string PartTwo = "part-two";
        public const string NoHints = "no-hints";
        public const string BugHunter = "bug-hunter";
        public const string ProCoder = "pro-coder";
        public const string NeverGiveUp = "never-give-up";
        public const string MazeMaster = "maze-master";
        public const string LevelMaker = "level-maker";
        public const string GuestStar = "guest-star";

        public static IReadOnlyList<Badge> All { get; } = new[]
        {
            new Badge(FirstFlag,   "🚩", "First flag",       "Reach the flag on any level."),
            new Badge(PartOne,     "⭐", "Loop legend",      "Get 3 stars on every level from 1 to 6."),
            new Badge(PartTwo,     "🌟", "Decision maker",   "Get 3 stars on every level from 7 to 12."),
            new Badge(NoHints,     "🧠", "Figured it out",   "Solve one of levels 7 to 12 without a hint."),
            new Badge(BugHunter,   "🐞", "Bug hunter",       "Use Step to look at your program, then solve the level."),
            new Badge(ProCoder,    "⌨️", "Pro coder",        "Solve a level by typing your program in Code mode."),
            new Badge(NeverGiveUp, "💪", "Never give up",    "Solve a level after 5 or more runs that didn't make it."),
            new Badge(MazeMaster,  "🧭", "Maze master",      "Beat The gauntlet, level 12."),
            new Badge(LevelMaker,  "🛠️", "Level maker",      "Make a level and solve it so it's ready to share."),
            new Badge(GuestStar,   "🎟️", "Guest star",       "Beat a level someone else made."),
        };
    }

    /// <summary>Remembers earned badges in the browser, next to level progress.</summary>
    public class BadgeStore
    {
        private const string StorageKey = "whatthecode.badges.v1";
        private readonly IJSRuntime _js;
        private HashSet<string> _earned = new();

        public BadgeStore(IJSRuntime js) => _js = js;

        public bool Has(string id) => _earned.Contains(id);
        public int Count => _earned.Count(id => BadgeCatalog.All.Any(b => b.Id == id));

        public async Task LoadAsync()
        {
            try
            {
                var json = await _js.InvokeAsync<string?>("localStorage.getItem", StorageKey);
                if (!string.IsNullOrWhiteSpace(json))
                    _earned = JsonSerializer.Deserialize<HashSet<string>>(json) ?? new();
            }
            catch (Exception ex) when (ex is JSException or JsonException)
            {
                _earned = new();
            }
        }

        /// <summary>Awards a badge. Returns it if it's new, so the game can celebrate; null if already earned.</summary>
        public async Task<Badge?> AwardAsync(string id)
        {
            var badge = BadgeCatalog.All.FirstOrDefault(b => b.Id == id);
            if (badge is null || !_earned.Add(id)) return null;

            try { await _js.InvokeVoidAsync("localStorage.setItem", StorageKey, JsonSerializer.Serialize(_earned)); }
            catch (JSException) { /* still counts for this visit */ }
            return badge;
        }
    }

    /// <summary>The level being made in the editor, kept while the maker tests it.</summary>
    public class DraftLevel
    {
        public CustomLevelSpec? Spec { get; set; }
    }
}

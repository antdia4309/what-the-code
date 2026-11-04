using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using WhatTheCode.Models;

namespace WhatTheCode.Services
{
    /// <summary>
    /// A player-made level as it's stored in a share link: a title, the map, which blocks are allowed,
    /// and par (set when the maker solves it). <see cref="Par"/> is null until the level has been beaten once.
    /// </summary>
    public record CustomLevelSpec(string Title, string[] Map, BlockKind[] Blocks, int? Par);

    /// <summary>Turns custom levels into short, URL-safe codes and back, checking everything on the way in.</summary>
    public static class CustomLevels
    {
        public const int MinSize = 3;
        public const int MaxSize = 8;
        public const int MaxTitle = 40;

        // Short property names keep share links short.
        private sealed record Wire(
            [property: JsonPropertyName("v")] int Version,
            [property: JsonPropertyName("t")] string Title,
            [property: JsonPropertyName("m")] string[] Map,
            [property: JsonPropertyName("b")] string Blocks,
            [property: JsonPropertyName("p")] int Par);

        private static readonly (BlockKind Kind, char Code)[] BlockCodes =
        {
            (BlockKind.MoveForward, 'M'), (BlockKind.TurnLeft, 'L'), (BlockKind.TurnRight, 'R'),
            (BlockKind.Repeat, 'P'), (BlockKind.RepeatUntilFlag, 'U'), (BlockKind.If, 'I'),
        };

        /// <summary>Encodes a level that has been solved (it needs a par to be shared).</summary>
        public static string Encode(CustomLevelSpec spec)
        {
            if (spec.Par is not { } par) throw new InvalidOperationException("Solve the level first so it has a par.");
            var blocks = new string(BlockCodes.Where(b => spec.Blocks.Contains(b.Kind)).Select(b => b.Code).ToArray());
            var json = JsonSerializer.Serialize(new Wire(1, spec.Title, spec.Map, blocks, par));
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(json)).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        }

        /// <summary>Reads a share code. Returns null (and a reason) if it isn't a valid level.</summary>
        public static CustomLevelSpec? Decode(string? code, out string? problem)
        {
            problem = "That level link looks broken or incomplete.";
            if (string.IsNullOrWhiteSpace(code) || code.Length > 2000) return null;

            try
            {
                var b64 = code.Replace('-', '+').Replace('_', '/');
                b64 = b64.PadRight(b64.Length + (4 - b64.Length % 4) % 4, '=');
                var wire = JsonSerializer.Deserialize<Wire>(Encoding.UTF8.GetString(Convert.FromBase64String(b64)));
                if (wire is null || wire.Version != 1 || wire.Map is null || wire.Blocks is null) return null;

                var blocks = BlockCodes.Where(b => wire.Blocks.Contains(b.Code)).Select(b => b.Kind).ToArray();
                var spec = new CustomLevelSpec((wire.Title ?? "").Trim(), wire.Map, blocks, wire.Par);

                problem = Validate(spec, requirePar: true);
                return problem is null ? spec : null;
            }
            catch (Exception ex) when (ex is FormatException or JsonException or ArgumentException or DecoderFallbackException)
            {
                return null;
            }
        }

        /// <summary>Everything that has to be true for a level to be playable. Returns null when it's fine.</summary>
        public static string? Validate(CustomLevelSpec spec, bool requirePar)
        {
            if (spec.Title.Length > MaxTitle) return $"Keep the name to {MaxTitle} characters.";
            if (spec.Map.Length is < MinSize or > MaxSize) return $"Boards are {MinSize} to {MaxSize} rows tall.";
            if (spec.Map.Any(r => r.Length != spec.Map[0].Length)) return "Every row has to be the same width.";
            if (spec.Map[0].Length is < MinSize or > MaxSize) return $"Boards are {MinSize} to {MaxSize} squares wide.";

            var all = string.Concat(spec.Map);
            if (all.Any(c => ".#F><^v".IndexOf(c) < 0)) return "The board has squares the game doesn't understand.";
            if (all.Count(c => "><^v".IndexOf(c) >= 0) != 1) return "Place the bot on the board (exactly one).";
            if (all.Count(c => c == 'F') != 1) return "Place the flag on the board (exactly one).";

            if (!spec.Blocks.Contains(BlockKind.MoveForward)) return "Allow Move forward, or the bot can't go anywhere.";
            if (requirePar && spec.Par is not (>= 1 and <= 99)) return "This level doesn't have a par yet.";

            if (!BoardCheck.FlagReachable(Board.FromMap(spec.Map)))
                return "The bot can't reach the flag. Clear a path through the walls.";

            return null;
        }

        /// <summary>Builds a playable level. Unsolved drafts get no par yet.</summary>
        public static Level ToLevel(CustomLevelSpec spec) => new()
        {
            Number = 0,
            Title = string.IsNullOrWhiteSpace(spec.Title) ? "A level with no name" : spec.Title,
            Tip = spec.Par is null
                ? "Solve your own level to prove it works. Your block count becomes its par."
                : "Someone made this level. Can you beat their block count?",
            Boards = new[] { Board.FromMap(spec.Map) },
            Par = spec.Par ?? 99,
            Blocks = spec.Blocks.Distinct().OrderBy(b => b).ToArray(),
            Hints = Array.Empty<string>(),
            Solution = () => new List<Instruction>()
        };
    }
}

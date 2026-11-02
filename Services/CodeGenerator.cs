using WhatTheCode.Models;

namespace WhatTheCode.Services
{
    /// <summary>A line of generated C#, tied back to the block that produced it.</summary>
    public record CodeLine(string Text, Guid? SourceId = null);

    /// <summary>Turns the player's blocks into the equivalent real C# — the "code bridge".</summary>
    public static class CodeGenerator
    {
        private static readonly string[] LoopVariables = { "i", "j", "k" };

        public static List<CodeLine> Generate(IReadOnlyList<Instruction> program)
        {
            var lines = new List<CodeLine>();

            if (program.Count == 0)
            {
                lines.Add(new("// Add blocks and your C# shows up here."));
                return lines;
            }

            Write(program, lines, depth: 0);
            return lines;
        }

        private static void Write(IEnumerable<Instruction> instructions, List<CodeLine> lines, int depth)
        {
            var indent = new string(' ', depth * 4);

            foreach (var instruction in instructions)
            {
                switch (instruction.Kind)
                {
                    case BlockKind.Repeat:
                        var v = LoopVariables[Math.Min(depth, LoopVariables.Length - 1)];
                        lines.Add(new($"{indent}for (int {v} = 0; {v} < {instruction.Count}; {v}++)", instruction.Id));
                        lines.Add(new($"{indent}{{", instruction.Id));
                        if (instruction.Body.Count == 0)
                            lines.Add(new($"{indent}    // nothing to repeat yet", instruction.Id));
                        else
                            Write(instruction.Body, lines, depth + 1);
                        lines.Add(new($"{indent}}}", instruction.Id));
                        break;

                    default:
                        lines.Add(new($"{indent}bot.{instruction.Kind}();", instruction.Id));
                        break;
                }
            }
        }
    }
}

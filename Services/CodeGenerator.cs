using WhatTheCode.Models;

namespace WhatTheCode.Services
{
    /// <summary>
    /// A line of generated C#, tied back to the block that produced it.
    /// Braces, <c>else</c> and comments are not <see cref="IsStatement"/>, so they don't light up while running.
    /// </summary>
    public record CodeLine(string Text, Guid? SourceId = null, bool IsStatement = true);

    /// <summary>Turns the player's blocks into the equivalent real C# — the "code bridge".</summary>
    public static class CodeGenerator
    {
        private static readonly string[] LoopVariables = { "i", "j", "k", "m" };

        public static List<CodeLine> Generate(IReadOnlyList<Instruction> program)
        {
            var lines = new List<CodeLine>();

            if (program.Count == 0)
            {
                lines.Add(new("// Add blocks and your C# shows up here."));
                return lines;
            }

            Write(program, lines, depth: 0, countedLoops: 0);
            return lines;
        }

        private static void Write(IEnumerable<Instruction> instructions, List<CodeLine> lines, int depth, int countedLoops)
        {
            var pad = new string(' ', depth * 4);

            foreach (var ins in instructions)
            {
                switch (ins.Kind)
                {
                    case BlockKind.Repeat:
                        var v = LoopVariables[Math.Min(countedLoops, LoopVariables.Length - 1)];
                        lines.Add(new($"{pad}for (int {v} = 0; {v} < {ins.Count}; {v}++)", ins.Id));
                        WriteBlock(ins.Body, ins.Id, lines, depth, countedLoops + 1, "nothing to repeat yet");
                        break;

                    case BlockKind.RepeatUntilFlag:
                        lines.Add(new($"{pad}while (!bot.AtFlag())", ins.Id));
                        WriteBlock(ins.Body, ins.Id, lines, depth, countedLoops, "nothing to repeat yet");
                        break;

                    case BlockKind.If:
                        lines.Add(new($"{pad}if (bot.{ConditionMethod(ins.Condition)}())", ins.Id));
                        WriteBlock(ins.Body, ins.Id, lines, depth, countedLoops, "nothing to do yet");
                        if (ins.ElseBody.Count > 0)
                        {
                            lines.Add(new($"{pad}else", ins.Id, IsStatement: false));
                            WriteBlock(ins.ElseBody, ins.Id, lines, depth, countedLoops, null);
                        }
                        break;

                    default:
                        lines.Add(new($"{pad}bot.{ins.Kind}();", ins.Id));
                        break;
                }
            }
        }

        private static void WriteBlock(List<Instruction> body, Guid owner, List<CodeLine> lines, int depth, int countedLoops, string? emptyComment)
        {
            var pad = new string(' ', depth * 4);
            lines.Add(new($"{pad}{{", owner, IsStatement: false));
            if (body.Count == 0 && emptyComment is not null)
                lines.Add(new($"{pad}    // {emptyComment}", owner, IsStatement: false));
            else
                Write(body, lines, depth + 1, countedLoops);
            lines.Add(new($"{pad}}}", owner, IsStatement: false));
        }

        private static string ConditionMethod(Condition c) => c switch
        {
            Condition.PathLeft  => "PathLeft",
            Condition.PathRight => "PathRight",
            _                   => "PathAhead"
        };
    }
}

using WhatTheCode.Models;

namespace WhatTheCode.Services
{
    /// <summary>One action the bot will take, plus the block that caused it (for highlighting).</summary>
    public record ProgramStep(Instruction Source, BlockKind Action);

    public static class ProgramRunner
    {
        /// <summary>Safety cap so a big nested loop can't run forever.</summary>
        public const int MaxSteps = 300;

        /// <summary>Unrolls loops into the exact sequence of actions the bot will perform.</summary>
        public static List<ProgramStep> Flatten(IEnumerable<Instruction> program)
        {
            var steps = new List<ProgramStep>();
            Expand(program, steps);
            return steps;
        }

        private static void Expand(IEnumerable<Instruction> instructions, List<ProgramStep> steps)
        {
            foreach (var instruction in instructions)
            {
                if (steps.Count >= MaxSteps) return;

                if (instruction.IsLoop)
                {
                    for (int i = 0; i < instruction.Count; i++)
                        Expand(instruction.Body, steps);
                }
                else
                {
                    steps.Add(new ProgramStep(instruction, instruction.Kind));
                }
            }
        }
    }
}

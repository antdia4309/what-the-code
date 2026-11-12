using WhatTheCode.Models;

namespace WhatTheCode.Services
{
    /// <summary>The C# each palette button drops into the editor in Code mode.</summary>
    public static class CodeSnippets
    {
        /// <summary>Where the cursor lands after inserting.</summary>
        public const string Cursor = "$0";

        /// <summary>The text to insert. <see cref="Cursor"/> marks where the cursor goes.</summary>
        public static string Insert(BlockKind kind) => kind switch
        {
            BlockKind.MoveForward     => "bot.MoveForward();$0",
            BlockKind.TurnLeft        => "bot.TurnLeft();$0",
            BlockKind.TurnRight       => "bot.TurnRight();$0",
            BlockKind.Repeat          => "for (int i = 0; i < 3; i++)\n{\n    $0\n}",
            BlockKind.RepeatUntilFlag => "while (!bot.AtFlag())\n{\n    $0\n}",
            BlockKind.If              => "if (bot.PathAhead())\n{\n    $0\n}\nelse\n{\n    \n}",
            _                         => ""
        };

        /// <summary>What the palette button says.</summary>
        public static string Label(BlockKind kind) => kind switch
        {
            BlockKind.MoveForward     => "MoveForward();",
            BlockKind.TurnLeft        => "TurnLeft();",
            BlockKind.TurnRight       => "TurnRight();",
            BlockKind.Repeat          => "for loop",
            BlockKind.RepeatUntilFlag => "while loop",
            BlockKind.If              => "if / else",
            _                         => kind.ToString()
        };
    }
}

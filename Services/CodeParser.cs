using WhatTheCode.Models;

namespace WhatTheCode.Services
{
    /// <summary>A problem in typed code, with the line it's on (1-based).</summary>
    public record CodeError(int Line, string Message);

    /// <summary>
    /// The result of reading typed C#. Either <see cref="Program"/> is set or <see cref="Error"/> is.
    /// <see cref="Lines"/> maps each block back to the line it was typed on, for highlighting while it runs.
    /// </summary>
    public record ParseResult(List<Instruction>? Program, CodeError? Error, IReadOnlyDictionary<Guid, int> Lines)
    {
        public bool Ok => Program is not null;
    }

    /// <summary>
    /// Reads the small slice of C# the game understands and turns it into the same blocks the editor builds:
    /// <c>bot.MoveForward();</c>, <c>bot.TurnLeft();</c>, <c>bot.TurnRight();</c>,
    /// <c>for (int i = 0; i &lt; N; i++)</c>, <c>while (!bot.AtFlag())</c>, and
    /// <c>if (bot.PathAhead()) … else …</c> (including <c>else if</c>).
    /// Error messages are written for beginners: say what's wrong and how to fix it.
    /// </summary>
    public static class CodeParser
    {
        public const int MaxNesting = 3;

        private static readonly Dictionary<string, BlockKind> Actions = new()
        {
            ["MoveForward"] = BlockKind.MoveForward,
            ["TurnLeft"] = BlockKind.TurnLeft,
            ["TurnRight"] = BlockKind.TurnRight,
        };

        private static readonly Dictionary<string, Condition> Questions = new()
        {
            ["PathAhead"] = Condition.PathAhead,
            ["PathLeft"] = Condition.PathLeft,
            ["PathRight"] = Condition.PathRight,
        };

        private static readonly string[] AllMethods = { "MoveForward", "TurnLeft", "TurnRight", "PathAhead", "PathLeft", "PathRight", "AtFlag" };
        private static readonly string[] Keywords = { "for", "while", "if", "else", "int", "bot" };

        public static ParseResult Parse(string source)
        {
            var lines = new Dictionary<Guid, int>();
            try
            {
                var tokens = Tokenizer.Read(source);
                var parser = new Parser(tokens, lines);
                var program = parser.ParseProgram();
                return new ParseResult(program, null, lines);
            }
            catch (CodeException ex)
            {
                return new ParseResult(null, new CodeError(ex.Line, ex.Message), lines);
            }
        }

        /// <summary>
        /// Checks a parsed program only uses blocks this level offers. Returns null when it's fine.
        /// </summary>
        public static CodeError? CheckAllowed(IEnumerable<Instruction> program, IReadOnlyCollection<BlockKind> allowed,
                                              IReadOnlyDictionary<Guid, int> lines, int levelNumber)
        {
            foreach (var block in Flatten(program))
            {
                if (allowed.Contains(block.Kind)) continue;

                var what = block.Kind switch
                {
                    BlockKind.Repeat          => "for loops",
                    BlockKind.RepeatUntilFlag => "while loops",
                    BlockKind.If              => "if checks",
                    _                         => $"bot.{block.Kind}()"
                };
                var which = levelNumber > 0 ? $"Level {levelNumber}" : "This level";
                return new CodeError(lines.GetValueOrDefault(block.Id, 1), $"{which} doesn't use {what}. Solve it with what the snippets offer.");
            }
            return null;
        }

        private static IEnumerable<Instruction> Flatten(IEnumerable<Instruction> program) =>
            program.SelectMany(i => new[] { i }.Concat(i.ChildLists.SelectMany(Flatten)));

        // ------------------------------------------------------------------ tokens

        private enum T { Word, Number, Symbol, End }

        private readonly record struct Token(T Type, string Text, int Line);

        private sealed class CodeException(int line, string message) : Exception(message)
        {
            public int Line { get; } = line;
        }

        private static class Tokenizer
        {
            public static List<Token> Read(string src)
            {
                var tokens = new List<Token>();
                int i = 0, line = 1;

                while (i < src.Length)
                {
                    char c = src[i];

                    if (c == '\n') { line++; i++; continue; }
                    if (char.IsWhiteSpace(c)) { i++; continue; }

                    // Comments
                    if (c == '/' && i + 1 < src.Length && src[i + 1] == '/')
                    {
                        while (i < src.Length && src[i] != '\n') i++;
                        continue;
                    }
                    if (c == '/' && i + 1 < src.Length && src[i + 1] == '*')
                    {
                        int startLine = line;
                        i += 2;
                        while (i + 1 < src.Length && !(src[i] == '*' && src[i + 1] == '/'))
                        {
                            if (src[i] == '\n') line++;
                            i++;
                        }
                        if (i + 1 >= src.Length) throw new CodeException(startLine, "This /* comment never ends. Close it with */.");
                        i += 2;
                        continue;
                    }

                    if (char.IsLetter(c) || c == '_')
                    {
                        int start = i;
                        while (i < src.Length && (char.IsLetterOrDigit(src[i]) || src[i] == '_')) i++;
                        tokens.Add(new Token(T.Word, src[start..i], line));
                        continue;
                    }

                    if (char.IsDigit(c))
                    {
                        int start = i;
                        while (i < src.Length && char.IsDigit(src[i])) i++;
                        tokens.Add(new Token(T.Number, src[start..i], line));
                        continue;
                    }

                    // Two-character symbols first
                    if (i + 1 < src.Length)
                    {
                        var pair = src.Substring(i, 2);
                        if (pair is "++" or "<=" or "+=" or "==" or "!=" or "&&" or "||")
                        {
                            tokens.Add(new Token(T.Symbol, pair, line));
                            i += 2;
                            continue;
                        }
                    }

                    if ("(){};.!<>=+-,".IndexOf(c) >= 0)
                    {
                        tokens.Add(new Token(T.Symbol, c.ToString(), line));
                        i++;
                        continue;
                    }

                    var shown = c switch { '“' or '”' or '"' => "quote marks", '\'' => "an apostrophe", _ => $"'{c}'" };
                    throw new CodeException(line, $"The game doesn't understand {shown} here.");
                }

                tokens.Add(new Token(T.End, "", line));
                return tokens;
            }
        }

        // ------------------------------------------------------------------ parser

        private sealed class Parser(List<Token> tokens, Dictionary<Guid, int> lines)
        {
            private int _pos;

            private Token Peek => tokens[_pos];
            private Token PeekAt(int ahead) => tokens[Math.Min(_pos + ahead, tokens.Count - 1)];
            private Token Previous => tokens[Math.Max(_pos - 1, 0)];
            private Token Next() => tokens[_pos++];

            private bool Is(string text) => Peek.Type != T.End && Peek.Text == text;

            private bool Accept(string text)
            {
                if (!Is(text)) return false;
                _pos++;
                return true;
            }

            /// <summary>Requires a symbol; if it's missing, reports it on the line of the code it should follow.</summary>
            private void Expect(string text, string message)
            {
                if (Accept(text)) return;
                throw new CodeException(Previous.Line, message);
            }

            public List<Instruction> ParseProgram()
            {
                var program = new List<Instruction>();
                while (Peek.Type != T.End)
                {
                    if (Is("}"))
                        throw new CodeException(Peek.Line, "There's an extra } here with no { to match it.");
                    program.Add(ParseStatement(depth: 0));
                }
                return program;
            }

            private Instruction ParseStatement(int depth)
            {
                var token = Peek;

                if (token.Type == T.Word)
                {
                    switch (token.Text)
                    {
                        case "bot": return ParseAction();
                        case "for": return ParseFor(depth);
                        case "while": return ParseWhile(depth);
                        case "if": return ParseIf(depth);
                        case "else": throw new CodeException(token.Line, "This else has no if right before it.");
                    }

                    if (Actions.ContainsKey(token.Text) || Questions.ContainsKey(token.Text))
                        throw new CodeException(token.Line, $"Start with bot. so the bot knows it's for it: bot.{token.Text}().");

                    throw new CodeException(token.Line, UnknownWord(token.Text));
                }

                if (token.Text == "{")
                    throw new CodeException(token.Line, "Curly braces need an if, for or while in front of them.");
                if (token.Text == ";")
                    throw new CodeException(token.Line, "There's a ; here with nothing before it.");
                if (token.Type == T.End)
                    throw new CodeException(token.Line, "The code ends too soon. Is something missing?");

                throw new CodeException(token.Line, $"The game didn't expect '{token.Text}' here.");
            }

            private string ReadBotMethod()
            {
                Next(); // bot
                Expect(".", "Put a dot between bot and what it should do, like bot.MoveForward().");

                if (Peek.Type != T.Word)
                    throw new CodeException(Previous.Line, "Something should come after bot. — try bot.MoveForward().");

                var name = Next();
                if (!AllMethods.Contains(name.Text))
                    throw new CodeException(name.Line, UnknownMethod(name.Text));

                Expect("(", $"Add () after {name.Text}, like bot.{name.Text}().");
                Expect(")", $"Close the brackets: bot.{name.Text}().");
                return name.Text;
            }

            private Instruction ParseAction()
            {
                int line = Peek.Line;
                var method = ReadBotMethod();

                if (Questions.ContainsKey(method) || method == "AtFlag")
                    throw new CodeException(line, $"bot.{method}() is a question, not a move. Use it inside if ( … ){(method == "AtFlag" ? " or while ( … )" : "")}.");

                if (!Is(";"))
                    throw new CodeException(line, $"Add a ; after bot.{method}() to end the line.");
                Next();

                var block = new Instruction(Actions[method]);
                lines[block.Id] = line;
                return block;
            }

            private Instruction ParseFor(int depth)
            {
                int line = Next().Line; // for
                CheckDepth(line, depth);

                Expect("(", "A for loop needs ( right after for.");
                if (!Accept("int"))
                    throw new CodeException(line, "Write the counter as int i = 0, like: for (int i = 0; i < 3; i++).");

                if (Peek.Type != T.Word)
                    throw new CodeException(line, "Give the counter a name after int, like i.");
                var name = Next().Text;

                Expect("=", $"Start the counter with = 0, like int {name} = 0.");
                if (Peek.Type != T.Number || Peek.Text != "0")
                    throw new CodeException(line, $"Start counting from 0: int {name} = 0.");
                Next();
                Expect(";", $"Put a ; after int {name} = 0.");

                if (!Accept(name))
                    throw new CodeException(line, $"Use the same counter in the middle part: {name} < 3.");
                if (Is("<="))
                    throw new CodeException(line, $"Use < instead of <= here. With {name} starting at 0, {name} < 3 runs 3 times.");
                Expect("<", $"Compare with <, like {name} < 3.");

                if (Peek.Type != T.Number)
                    throw new CodeException(line, $"Put how many times to repeat after <, like {name} < 3.");
                int count = int.Parse(Next().Text);
                if (count < Instruction.MinRepeat || count > Instruction.MaxRepeat)
                    throw new CodeException(line, $"Repeat counts in this game go from {Instruction.MinRepeat} to {Instruction.MaxRepeat}.");

                Expect(";", $"Put a ; after {name} < {count}.");

                bool counted = (Accept(name) && Accept("++")) || (Accept("++") && Accept(name));
                if (!counted)
                    throw new CodeException(line, $"End the loop header with {name}++ so the counter goes up each time.");
                Expect(")", "Close the for loop's brackets with ).");

                var loop = new Instruction(BlockKind.Repeat);
                loop.ChangeCount(count - loop.Count);
                lines[loop.Id] = line;
                ParseBody(loop.Body, depth + 1, line, "for loop");
                return loop;
            }

            private Instruction ParseWhile(int depth)
            {
                int line = Next().Line; // while
                CheckDepth(line, depth);

                Expect("(", "A while loop needs ( right after while.");
                if (!Accept("!"))
                {
                    if (Is("bot") && PeekAt(2).Text == "AtFlag")
                        throw new CodeException(line, "Add ! before bot.AtFlag(). The loop runs while the bot is NOT at the flag.");
                    throw new CodeException(line, "In this game, while loops run until the flag: while (!bot.AtFlag()).");
                }
                if (!Is("bot"))
                    throw new CodeException(line, "In this game, while loops run until the flag: while (!bot.AtFlag()).");

                var method = ReadBotMethod();
                if (method != "AtFlag")
                    throw new CodeException(line, "In this game, while loops run until the flag: while (!bot.AtFlag()).");
                Expect(")", "Close the while loop's brackets with ).");

                var loop = new Instruction(BlockKind.RepeatUntilFlag);
                lines[loop.Id] = line;
                ParseBody(loop.Body, depth + 1, line, "while loop");
                return loop;
            }

            private Instruction ParseIf(int depth)
            {
                int line = Next().Line; // if
                CheckDepth(line, depth);

                Expect("(", "An if needs ( right after if.");
                if (Is("!"))
                    throw new CodeException(line, "This game's if checks can't use ! yet. Swap what's in the two parts instead.");
                if (!Is("bot"))
                    throw new CodeException(line, "An if checks the board, like if (bot.PathAhead()).");

                var method = ReadBotMethod();
                if (!Questions.TryGetValue(method, out var condition))
                    throw new CodeException(line, method == "AtFlag"
                        ? "Checking for the flag goes in a while loop: while (!bot.AtFlag())."
                        : $"bot.{method}() is a move, not a question. An if checks something, like bot.PathAhead().");

                if (Is("&&") || Is("||"))
                    throw new CodeException(line, "This game's if checks one thing at a time. Put one if inside another instead.");
                Expect(")", "Close the if's brackets with ).");

                var block = new Instruction(BlockKind.If);
                while (block.Condition != condition) block.CycleCondition();
                lines[block.Id] = line;
                ParseBody(block.Body, depth + 1, line, "if");

                if (Accept("else"))
                {
                    if (Is("if"))
                        block.ElseBody.Add(ParseIf(depth + 1)); // else if = an If inside Otherwise
                    else
                        ParseBody(block.ElseBody, depth + 1, Previous.Line, "else");
                }

                return block;
            }

            /// <summary>Either { statements } or a single statement, like C# allows.</summary>
            private void ParseBody(List<Instruction> body, int depth, int ownerLine, string owner)
            {
                if (Peek.Type == T.End)
                    throw new CodeException(ownerLine, $"This {owner} needs something to run. Add {{ }} with code inside.");

                if (Is(";"))
                    throw new CodeException(Peek.Line, $"Remove the ; after the {owner}'s ( ). It ends the {owner} before it starts.");

                if (!Accept("{"))
                {
                    body.Add(ParseStatement(depth));
                    return;
                }

                int open = Previous.Line;
                while (!Is("}"))
                {
                    if (Peek.Type == T.End)
                        throw new CodeException(open, $"The {{ on line {open} is never closed. Add a }} at the end of the {owner}.");
                    body.Add(ParseStatement(depth));
                }
                Next(); // }
            }

            private static void CheckDepth(int line, int depth)
            {
                if (depth >= MaxNesting)
                    throw new CodeException(line, $"That's too many loops and ifs inside each other. The limit is {MaxNesting} deep.");
            }

            private static string UnknownWord(string word)
            {
                var keyword = Keywords.FirstOrDefault(k => string.Equals(k, word, StringComparison.OrdinalIgnoreCase));
                if (keyword is not null)
                    return $"C# is case-sensitive: write {keyword} in lowercase.";

                var near = Closest(word, Keywords);
                return near is not null
                    ? $"The game doesn't know '{word}'. Did you mean {near}?"
                    : $"The game doesn't know '{word}'. Lines start with bot., for, while or if.";
            }

            private static string UnknownMethod(string name)
            {
                var exact = AllMethods.FirstOrDefault(m => string.Equals(m, name, StringComparison.OrdinalIgnoreCase));
                if (exact is not null)
                    return $"C# is case-sensitive: write {exact}.";

                var near = Closest(name, AllMethods);
                return near is not null
                    ? $"The bot doesn't know {name}. Did you mean {near}?"
                    : $"The bot doesn't know {name}. It can MoveForward, TurnLeft and TurnRight.";
            }

            private static string? Closest(string word, IEnumerable<string> options)
            {
                var best = options
                    .Select(o => (o, d: Distance(word.ToLowerInvariant(), o.ToLowerInvariant())))
                    .OrderBy(x => x.d)
                    .First();
                return best.d <= Math.Max(2, best.o.Length / 3) ? best.o : null;
            }

            /// <summary>Edit distance, for "did you mean" suggestions.</summary>
            private static int Distance(string a, string b)
            {
                var d = new int[a.Length + 1, b.Length + 1];
                for (int i = 0; i <= a.Length; i++) d[i, 0] = i;
                for (int j = 0; j <= b.Length; j++) d[0, j] = j;
                for (int i = 1; i <= a.Length; i++)
                    for (int j = 1; j <= b.Length; j++)
                        d[i, j] = Math.Min(Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1), d[i - 1, j - 1] + (a[i - 1] == b[j - 1] ? 0 : 1));
                return d[a.Length, b.Length];
            }
        }
    }
}

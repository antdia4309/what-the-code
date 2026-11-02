using WhatTheCode.Models;
using static WhatTheCode.Services.Build;

namespace WhatTheCode.Services
{
    /// <summary>
    /// Hand-built levels. Each introduces at most one new idea:
    /// sequence → turning → walls → loops → patterns → nested loops →
    /// if/else → repeat-until → nested ifs → minimal programs → wall-following.
    ///
    /// Map key: <c>.</c> open, <c>#</c> wall, <c>F</c> flag, <c>&gt; &lt; ^ v</c> bot start and facing.
    /// Every level has a reference solution in the test project that must hit par on every board.
    /// </summary>
    public static class LevelCatalog
    {
        private static List<Instruction> WallFollower() => Steps(
            Until(
                If(Condition.PathRight,
                    Do(Right(), Move()),
                    If(Condition.PathAhead, Do(Move()), Left()))));

        private static readonly BlockKind[] MoveOnly   = { BlockKind.MoveForward };
        private static readonly BlockKind[] WithTurns  = { BlockKind.MoveForward, BlockKind.TurnLeft, BlockKind.TurnRight };
        private static readonly BlockKind[] MoveLoop   = { BlockKind.MoveForward, BlockKind.Repeat };
        private static readonly BlockKind[] Loops      = { BlockKind.MoveForward, BlockKind.TurnLeft, BlockKind.TurnRight, BlockKind.Repeat };
        private static readonly BlockKind[] WithIf     = { BlockKind.MoveForward, BlockKind.TurnLeft, BlockKind.TurnRight, BlockKind.Repeat, BlockKind.If };
        private static readonly BlockKind[] Everything = { BlockKind.MoveForward, BlockKind.TurnLeft, BlockKind.TurnRight, BlockKind.Repeat, BlockKind.RepeatUntilFlag, BlockKind.If };

        public static IReadOnlyList<Level> All { get; } = new List<Level>
        {
            // ---------- Part 1: sequences and loops ----------

            new()
            {
                Number = 1, Title = "First steps",
                NewIdea = "A program is a list of steps. Your bot runs them in order, top to bottom.",
                Tip = "Add Move forward blocks until the bot would land on the flag, then press Run.",
                Par = 3, Blocks = MoveOnly,
                Hints = new[]
                {
                    "Count the squares between the bot and the flag.",
                    "Each Move forward goes exactly one square. Nothing else is needed."
                },
                Solution = () => Steps(Move(), Move(), Move()),
                Boards = new[]
                {
                    Board.FromMap(
                        ".....",
                        ".....",
                        ">..F.",
                        ".....",
                        ".....")
                }
            },
            new()
            {
                Number = 2, Title = "Turn the corner",
                NewIdea = "Turning doesn't move the bot. It only changes which way the bot faces.",
                Tip = "Watch the pointer on the bot. Turns are from the bot's point of view, not yours.",
                Par = 7, Blocks = WithTurns,
                Hints = new[]
                {
                    "Go along the bottom first, then go up.",
                    "Three squares across, one turn, three squares up. Which way does the bot have to turn to face up?"
                },
                Solution = () => Steps(Move(), Move(), Move(), Left(), Move(), Move(), Move()),
                Boards = new[]
                {
                    Board.FromMap(
                        ".....",
                        "...F.",
                        ".....",
                        ".....",
                        ">....")
                }
            },
            new()
            {
                Number = 3, Title = "Around the wall",
                Tip = "Walls stop the bot cold. Trace your path with a finger before you build.",
                Par = 10, Blocks = WithTurns,
                Hints = new[]
                {
                    "Go over the wall, not through it.",
                    "Take one step, then climb up a row and drive along it. Drop back down once you're past the wall."
                },
                Solution = () => Steps(Move(), Left(), Move(), Right(), Move(), Move(), Right(), Move(), Left(), Move()),
                Boards = new[]
                {
                    Board.FromMap(
                        "......",
                        "......",
                        ">.#.F.",
                        "..#...",
                        "..#...")
                }
            },
            new()
            {
                Number = 4, Title = "Long hallway",
                NewIdea = "New block: Repeat. Blocks inside it run again and again, so you don't have to copy them.",
                Tip = "Seven moves in a row? Put one Move inside a Repeat and set the count.",
                Par = 2, Blocks = MoveLoop,
                Hints = new[]
                {
                    "Tap + Repeat first, then + Move forward. The move lands inside the loop.",
                    "The Repeat's number should match the squares between the bot and the flag. Use its + and − buttons."
                },
                Solution = () => Steps(Repeat(7, Move())),
                Boards = new[]
                {
                    Board.FromMap(
                        "########",
                        ">......F",
                        "########")
                }
            },
            new()
            {
                Number = 5, Title = "Staircase",
                Tip = "Spot the pattern: move, turn, move, turn. One stair's worth of blocks goes inside the Repeat.",
                Par = 5, Blocks = Loops,
                Hints = new[]
                {
                    "Build one stair by hand, then look at the blocks you used.",
                    "One stair is four blocks. Count the stairs to know how many times to repeat them."
                },
                Solution = () => Steps(Repeat(5, Move(), Left(), Move(), Right())),
                Boards = new[]
                {
                    Board.FromMap(
                        "....#F",
                        "...#..",
                        "..#..#",
                        ".#..#.",
                        "#..#..",
                        ">.#...")
                }
            },
            new()
            {
                Number = 6, Title = "Big steps",
                NewIdea = "Loops can go inside loops. Tap a Repeat's inner area to add blocks there.",
                Tip = "Each stair is two moves across and two moves up. Can a Repeat handle each pair?",
                Par = 7, Blocks = Loops,
                Hints = new[]
                {
                    "Build it the long way first, then look for blocks that come in pairs.",
                    "A Repeat can sit inside another Repeat. The outer one runs once for each stair."
                },
                Solution = () => Steps(Repeat(3, Repeat(2, Move()), Left(), Repeat(2, Move()), Right())),
                Boards = new[]
                {
                    Board.FromMap(
                        ".....#F",
                        ".....#.",
                        "...#...",
                        "...#.#.",
                        ".#...#.",
                        ".#.#...",
                        ">..#...")
                }
            },

            // ---------- Part 2: decisions ----------

            new()
            {
                Number = 7, Title = "Look before you leap",
                NewIdea = "New block: If. While the program runs, it checks the board. If the answer is yes, the top part runs. If not, the Otherwise part runs.",
                Tip = "The corner is in a different spot on each board, so you can't count your way there. Let an If decide when to turn.",
                Par = 4, Blocks = WithIf,
                Hints = new[]
                {
                    "Picture the bot standing at the corner. What does it see straight ahead that it didn't see a moment ago?",
                    "Tap the Otherwise area under an If to put blocks there. Then pick a Repeat count big enough for the longer board."
                },
                Solution = () => Steps(Repeat(9, If(Condition.PathAhead, Do(Move()), Left()))),
                Boards = new[]
                {
                    Board.FromMap(
                        "##F###",
                        "##.###",
                        "##.###",
                        "##.###",
                        "##.###",
                        ">..###"),
                    Board.FromMap(
                        "######",
                        "######",
                        "####F#",
                        "####.#",
                        "####.#",
                        ">....#")
                }
            },
            new()
            {
                Number = 8, Title = "Until you get there",
                NewIdea = "New block: Repeat until flag. It keeps going until the bot reaches the flag, so you never have to count.",
                Tip = "Three boards, three lengths. Build one program that doesn't care how far the flag is.",
                Par = 4, Blocks = Everything,
                Hints = new[]
                {
                    "This is the same idea as Level 7, minus the counting.",
                    "Build the If from Level 7, then put it inside Repeat until flag instead of Repeat."
                },
                Solution = () => Steps(Until(If(Condition.PathAhead, Do(Move()), Left()))),
                Boards = new[]
                {
                    Board.FromMap(
                        "########",
                        ">......F",
                        "########"),
                    Board.FromMap(
                        "#####",
                        "#F###",
                        "#.###",
                        "#.###",
                        ">.###"),
                    Board.FromMap(
                        "######",
                        "#F...#",
                        "####.#",
                        "####.#",
                        ">....#")
                }
            },
            new()
            {
                Number = 9, Title = "Which way?",
                NewIdea = "An If can go inside another If. Check one direction first, and if it's blocked, check another.",
                Tip = "Some corners turn left and some turn right. Tap the If's question to change what it checks.",
                Par = 6, Blocks = Everything,
                Hints = new[]
                {
                    "When the way ahead is blocked, there are still two choices.",
                    "Put a second If inside the first one's Otherwise part, and set it to check one side."
                },
                Solution = () => Steps(Until(If(Condition.PathAhead, Do(Move()), If(Condition.PathLeft, Do(Left()), Right())))),
                Boards = new[]
                {
                    Board.FromMap(
                        ">..###",
                        "##.###",
                        "##....",
                        "#####.",
                        "#####.",
                        "#####F"),
                    Board.FromMap(
                        ">...##",
                        "###.##",
                        "###.##",
                        "###.##",
                        "###...",
                        "#####F")
                }
            },
            new()
            {
                Number = 10, Title = "Spiral",
                Tip = "Every turn is a right turn. What's the smallest program that gets to the middle?",
                Par = 4, Blocks = Everything,
                Hints = new[]
                {
                    "You only ever need one kind of turn on this board.",
                    "It's very close to your Level 8 program. Look at which way the corners go."
                },
                Solution = () => Steps(Until(If(Condition.PathAhead, Do(Move()), Right()))),
                Boards = new[]
                {
                    Board.FromMap(
                        ">.....",
                        "#####.",
                        "....#.",
                        ".##F#.",
                        ".####.",
                        "......")
                }
            },
            new()
            {
                Number = 11, Title = "Hug the wall",
                NewIdea = "A real maze trick: keep your right hand on the wall. Turn right whenever you can, go straight if you can't, and turn left as a last resort.",
                Tip = "Dead ends are fine. Following the right wall leads you back out of them.",
                Par = 7, Blocks = Everything,
                Hints = new[]
                {
                    "Order matters. Check right first, then straight ahead, and only turn left if both are blocked.",
                    "When there's a path to the right, the bot needs two blocks: one to face it and one to go."
                },
                Solution = () => WallFollower(),
                Boards = new[]
                {
                    Board.FromMap(
                        ">..#...",
                        "##.#.#.",
                        "...#.#.",
                        ".###.#.",
                        "..##.#.",
                        "#.....#",
                        "#.###.F"),
                    Board.FromMap(
                        "v#.....",
                        ".#.###.",
                        ".#...#.",
                        "...#.#.",
                        "##.#.#.",
                        "#..#.##",
                        "#.##..F")
                }
            },
            new()
            {
                Number = 12, Title = "The gauntlet",
                Tip = "Three mazes you've never seen. If your program really follows the wall, it won't need any changes.",
                Par = 7, Blocks = Everything,
                Hints = new[]
                {
                    "Did your Level 11 program really follow the wall, or did it get lucky?",
                    "A true wall-follower works on any maze like these, without changes."
                },
                Solution = () => WallFollower(),
                Boards = new[]
                {
                    Board.FromMap(
                        ">....#F",
                        "###.#..",
                        "#...#.#",
                        "#.###.#",
                        "#.....#",
                        "#.#.###",
                        "#.#...#"),
                    Board.FromMap(
                        "^.....#",
                        ".####.#",
                        ".#....#",
                        ".#.####",
                        ".#....F",
                        ".######",
                        "......#"),
                    Board.FromMap(
                        "#######",
                        "#F..#.#",
                        "##.##.#",
                        "#..#..#",
                        "#.##.##",
                        "#.....#",
                        ">.#####")
                }
            }
        };
    }
}

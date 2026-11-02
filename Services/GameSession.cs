using WhatTheCode.Models;

namespace WhatTheCode.Services
{
    public enum StepOutcome { Moved, Turned, HitWall, HitEdge }

    /// <summary>Live state for one board: the grid, the bot, and how a single action changes them.</summary>
    public class GameSession
    {
        public Board Board { get; }
        public Grid Grid { get; }
        public Character Bot { get; }

        public GameSession(Board board)
        {
            Board = board;
            Grid = new Grid(board.Columns, board.Rows, board.Walls);
            Bot = new Character(board.Start.X, board.Start.Y, board.StartFacing);
            Reset();
        }

        public bool OnFlag => Bot.X == Board.Flag.X && Bot.Y == Board.Flag.Y;

        public void Reset()
        {
            Bot.Reset(Board.Start.X, Board.Start.Y, Board.StartFacing);
            Grid.Reset();
            Grid.MarkVisited(Bot.X, Bot.Y);
        }

        /// <summary>Answers an If block's question about the bot's surroundings right now.</summary>
        public bool Check(Condition condition)
        {
            var direction = condition switch
            {
                Condition.PathLeft  => Character.LeftOf(Bot.Facing),
                Condition.PathRight => Character.RightOf(Bot.Facing),
                _                   => Bot.Facing
            };
            return Grid.IsOpen(Bot.CellToward(direction));
        }

        public StepOutcome Apply(BlockKind action)
        {
            switch (action)
            {
                case BlockKind.TurnLeft:
                    Bot.TurnLeft();
                    return StepOutcome.Turned;

                case BlockKind.TurnRight:
                    Bot.TurnRight();
                    return StepOutcome.Turned;

                case BlockKind.MoveForward:
                    var result = Bot.MoveForward(Grid);
                    if (result == MoveResult.Moved)
                        Grid.MarkVisited(Bot.X, Bot.Y);
                    return result switch
                    {
                        MoveResult.HitWall => StepOutcome.HitWall,
                        MoveResult.HitEdge => StepOutcome.HitEdge,
                        _                  => StepOutcome.Moved
                    };

                default:
                    throw new ArgumentOutOfRangeException(nameof(action), "Only actions reach the board; the runner handles loops and ifs.");
            }
        }
    }
}

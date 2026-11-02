using WhatTheCode.Models;

namespace WhatTheCode.Services
{
    public enum StepOutcome { Moved, Turned, HitWall, HitEdge }

    /// <summary>Board state for one level: the grid, the bot, and how a single block changes them.</summary>
    public class GameSession
    {
        public Level Level { get; }
        public Grid Grid { get; }
        public Character Bot { get; }

        public GameSession(Level level)
        {
            Level = level;
            Grid = new Grid(level.Columns, level.Rows, level.Walls);
            Bot = new Character(level.Start.X, level.Start.Y, level.StartFacing);
            Reset();
        }

        public bool OnFlag => Bot.X == Level.Flag.X && Bot.Y == Level.Flag.Y;

        public void Reset()
        {
            Bot.Reset(Level.Start.X, Level.Start.Y, Level.StartFacing);
            Grid.Reset();
            Grid.MarkVisited(Bot.X, Bot.Y);
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
                    throw new ArgumentOutOfRangeException(nameof(action), "Repeat is expanded before it reaches the board.");
            }
        }
    }
}

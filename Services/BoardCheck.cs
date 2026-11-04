using WhatTheCode.Models;

namespace WhatTheCode.Services
{
    public static class BoardCheck
    {
        /// <summary>True if the bot could drive from its start to the flag, ignoring which way it faces.</summary>
        public static bool FlagReachable(Board board)
        {
            var walls = board.Walls.ToHashSet();
            bool Open(Cell c) => c.X >= 0 && c.X < board.Columns && c.Y >= 0 && c.Y < board.Rows && !walls.Contains(c);

            var seen = new HashSet<Cell> { board.Start };
            var queue = new Queue<Cell>(seen);
            while (queue.Count > 0)
            {
                var c = queue.Dequeue();
                if (c == board.Flag) return true;
                foreach (var n in new[] { c with { X = c.X + 1 }, c with { X = c.X - 1 }, c with { Y = c.Y + 1 }, c with { Y = c.Y - 1 } })
                    if (Open(n) && seen.Add(n)) queue.Enqueue(n);
            }
            return false;
        }
    }
}

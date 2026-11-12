namespace WhatTheCode.Models
{
    /// <summary>
    /// A programming idea a level introduces. The short line is always shown;
    /// the rest sits behind "Tell me more" so nobody has to read a wall of text to play.
    /// </summary>
    /// <param name="Name">What the idea is called, like "Loops".</param>
    /// <param name="Line">One plain sentence, plus how to use it in the game.</param>
    /// <param name="More">A short paragraph on why the idea exists.</param>
    /// <param name="RealWorld">Where the same idea shows up outside the game.</param>
    /// <param name="Code">A small C# example. The tests check that the game's parser accepts it.</param>
    public sealed record Concept(string Name, string Line, string More, string RealWorld, string Code);
}

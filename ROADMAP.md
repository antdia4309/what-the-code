# What the CODE? — Roadmap

_Last updated: November 2026 · Sessions 1–7 shipped_

## Who it's for

**Curious beginners, roughly 16–30, who have never written code.** High school and college students meeting programming for the first time, and early-career people wondering whether tech is for them.

Not young kids (Scratch and Code.org own that space, and it pulls the design toward cartoons). Not working developers (there's nothing here for them to learn).

Public data backs this up: people learning to code skew 18–24, online-course learners skew mid-to-late 20s, a large share are thinking about switching careers, and Gen Z learners are the most likely to use coding challenges. Python is the language most learners want next.

## Principles

These hold for every session:

1. **Beginner-first, never babyish.** Playful and encouraging, like Duolingo. No mascot story, no talking down.
2. **One new idea at a time.** Each level introduces at most one concept, and the game says what it is in one sentence.
3. **Short explanations, more on request.** One plain sentence up front, with "Tell me more" for anyone who wants it.
4. **Connect to the real world.** "A robot vacuum uses this exact trick" matters more to this audience than a cartoon.
5. **Phones are first-class.** Every feature has to work one-handed on a phone.
6. **No accounts, no server, for as long as possible.** Everything lives in the browser and in links. That keeps it free to run and instant to try.
7. **Tests guard the game.** Every level has an answer the tests prove works, and nothing deploys if a test fails.

## Where it stands (Sessions 1–7)

12 levels across two parts (sequences and loops; decisions). Blocks and real C#, with switching between them both ways. Debugging tools: step, pause, speed, visible decisions. Hints and answer reveal. Phone layout. A level editor with share links. 10 badges. 151 automated tests. Deployed to GitHub Pages on every push.

---

## Phase 1 — Make the first ten minutes great

The goal: a complete beginner gets from "what is this?" to solving Level 3 without help, and we can *see* whether that's happening.

### Session 8 · First-run experience and dark mode

- A short, skippable welcome on first visit: three steps (this is the bot, these are blocks, press Run), each one shown on the real screen rather than on a separate page.
- **Concept cards.** When a level introduces something new, a card explains it in one sentence, with "Tell me more" for a short paragraph and a real-world example (loops → assembly lines; if/else → a thermostat; wall-following → robot vacuums and maze-solving).
- A tone pass on all game text, to make sure nothing is cute for its own sake or assumes coding knowledge.
- **Dark mode.** It follows the phone/computer setting by default, with a manual toggle.

**Done when**
- [ ] A first-time visitor sees the welcome once; returning players never do; it can be skipped and replayed from a menu.
- [ ] Every level that introduces a concept has a card with a "Tell me more" and a real-world example.
- [ ] Dark mode passes contrast checks (WCAG AA) on every screen, including the editor and code view.
- [ ] Tested on phone and desktop, in both light and dark.

### Session 9 · Measure and share

- **Anonymous play stats.** These would be privacy-friendly (no cookies, no personal data), using a service like GoatCounter or Plausible. Events: level started, solved, abandoned; hints used; Blocks vs Code; Step used; links shared. A short privacy note in the footer.
- **"Share my solution."** A link that opens the level with *your* program loaded. It's useful for showing off, asking a friend for help, or a teacher checking work.
- **Install as an app.** Add to home screen, works offline.
- A "share" button after beating a level, with a ready-made message ("I beat Level 9 of What the CODE? in 6 blocks").

**Done when**
- [ ] A dashboard shows, per level: how many started, how many solved, and the average number of runs.
- [ ] No personal data or cookies are collected, and the privacy note says so plainly.
- [ ] A solution link opens on another device with the program loaded and ready to run.
- [ ] The game installs from Chrome (Android) and Safari (iPhone) and plays with no connection.

### ⏸ Checkpoint: read the data

Run Session 9's stats for 2–3 weeks and share the game in at least two places (LinkedIn, a student community).

Then answer these:
- Where do people quit? A big drop at one level means fix that level before adding more.
- Do people try Code mode? If few do, Python can wait. If many do, it moves up.
- Do people come back the next day or week? If not, the daily puzzle moves up.

**Phase 2 and 3 can swap order based on what this shows.**

---

## Phase 2 — Go deeper

The goal: a player who finishes the game has met every core idea of a first programming class.

### Session 10 · Functions

- A new **Define / Use** block: write a group of blocks once, give it a name, and use it as often as you like. In code: `void Stair() { … }` and `Stair();`.
- Part 3 starts with levels 13–16, which teach functions. Each has a "you'd need 30 blocks without this" moment.
- The code parser, generator, step-through and editor all support functions.

**Done when**
- [ ] Functions work the same way in Blocks and Code mode, and convert between them without loss.
- [ ] Step-through shows when the program jumps into a function and back out.
- [ ] Levels 13–16 have tested par answers, and at least one is unsolvable within par *without* a function.

### Session 11 · Part 3 levels and the daily puzzle

- Levels 17–20 combine functions with loops and decisions.
- **A daily puzzle.** The same generated level for everyone each day, built so that functions or a clever loop beat brute force. It comes with a streak counter.
- **A second score: fewest steps.** It sits beside fewest blocks. Sometimes the two fight each other, which is a real programming trade-off.

**Done when**
- [ ] Every generated daily puzzle is checked solvable before it's shown, and the same date gives everyone the same puzzle.
- [ ] Streaks survive a reload and reset after a missed day.
- [ ] Levels 17–20 have tested par answers.

---

## Phase 3 — More languages

The goal: the skills carry straight into the language a player will meet next.

### Session 12 · Python

- A **C# / Python** switch for the code panel and Code mode.
- A Python parser with the same beginner-friendly errors, rewritten for Python's common slip-ups (missing colons, indentation, `True` vs `true`).
- Concept cards mention how the idea looks in both languages.

**Done when**
- [ ] Every level's answer converts C# → Python → blocks without loss (round-trip tests, like Session 6).
- [ ] At least 15 common Python mistakes each produce a clear, located message.
- [ ] The language choice is remembered.

### Session 13 · JavaScript

- The same approach for JavaScript.
- A short "same idea, three languages" view on each concept card.

**Done when**
- [ ] The same round-trip and error-message checks as Python.

---

## Phase 4 — Variety and community

The goal: there's always something new to play, much of it made by players.

### Session 14 · Gems, keys and variables

- Collectibles and switches: pick up every gem, find the key before the door.
- **Variables**, introduced through counting ("collect 3 gems, then head to the flag").
- New question blocks: `bot.OnGem()`, `bot.HasKey()`.
- Levels 21–26, and the editor gets these pieces too.

### Session 15 · Level packs and featured levels

- A **level pack** link: several custom levels bundled into one link, for a teacher, a club, or a friend.
- Multi-board custom levels in the editor.
- A **featured levels** list, curated by you, stored as a file in the repo. Players can submit a level link for consideration.

---

## Backlog (not scheduled)

Worth doing eventually. Each one needs a reason from the data before it gets a session.

| Idea | Why it waits |
|---|---|
| Themes (unlocked by badges) | Cosmetic; lower value for this audience than for kids |
| Sound and haptics | Nice polish; optional and off by default |
| Spanish (and other languages) | Big reach, but every level, hint and error message needs translating |
| Accessibility audit with screen reader users | Should happen before any big public push |
| Accounts and syncing progress across devices | Needs a server (e.g. Supabase) and a privacy policy |
| Classroom mode (teacher sees student progress) | Needs accounts first |
| AI hints ("why didn't this work?") | Needs a server to keep API keys safe, and has a running cost |
| Leaderboards | Needs a server, and needs cheat-proofing |

## How we'll know it's working

| Measure | Starting goal |
|---|---|
| New players who solve Level 3 | 70% |
| Players who reach Part 2 (Level 7) | 35% |
| Players who come back within a week | 20% |
| Players who try Code mode at least once | 25% |
| Custom levels shared per week | Growing month over month |

These are starting guesses. Session 9 gives the real baseline, and the targets get reset from there.

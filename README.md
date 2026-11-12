# 🎮 What the CODE?

**▶ Play it: [antdia4309.github.io/what-the-code](https://antdia4309.github.io/what-the-code/)**

A dual-layer, age-adaptive coding education game built with **C# + Blazor**. Designed to teach programming logic to anyone — from a 6-year-old dragging blocks to a 40-year-old reading real syntax.

---

## 🧠 The Concept

Most coding education tools pick a lane — either too simple for adults or too intimidating for kids. **What the CODE?** bridges that gap with a progressive interface that evolves as the user's brain levels up.

| Junior Mode | Pro Mode |
|---|---|
| Large buttons, visual blocks | Full code editor, dark mode |
| Audio cues, mascot guidance | Syntax highlighting, debugging |
| Block-based logic | Real C# / text-based code |

---

## 🚀 Sessions (Build Roadmap)

| Session | Focus | Status |
|---|---|---|
| 1 | Core feedback loop — code → visual output | ✅ Complete |
| 2 | Program builder + levels — build a program, run it, reach the flag | ✅ Complete |
| 3 | Decisions + deployment — If/Otherwise, Repeat until, multi-board levels, WebAssembly on GitHub Pages | ✅ Complete |
| 4 | Phone-friendly — pinned board, thumb dock for blocks and Run, tap-the-board guidance | ✅ Complete |
| 5 | Debugging tools — pause, step, speed, visible decisions, loop counters | ✅ Complete |
| 6 | Pro mode — type real C# instead of blocks, with beginner-friendly errors | ✅ Complete |
| 7 | Level editor with share links, and badges | ✅ Complete |
| 8 | First-run welcome, concept cards, dark mode, accessible colors | ✅ Complete |
| 9 | Anonymous play stats, "share my solution", install as an app | 🔲 Next |
| 10–15 | Functions, Part 3 levels, daily puzzle, Python, JavaScript, gems & variables, level packs | 🔲 Planned |

See **[ROADMAP.md](ROADMAP.md)** for who the game is for, the principles behind it, and what "done" means for each session.

---

## 🎯 What's playable now

**12 levels in two parts.** Each one introduces at most one new idea.

| Part | Levels | Ideas |
|---|---|---|
| Sequences & loops | 1–6 | Step order, turning, planning around walls, Repeat, patterns, nested loops |
| Decisions | 7–12 | If/Otherwise, Repeat until flag, nested ifs, minimal programs, wall-following mazes |

- **A quick welcome.** First-time players get three skippable steps that point at the real board, blocks and Run button. It can be replayed from ⚙️ Settings.
- **Concept cards.** When a level introduces an idea, a card names it in one sentence. "Tell me more" adds why it matters, where it shows up in real life (recipes, assembly lines, thermostats, robot vacuums), and a small C# example.
- **Build, then run.** Stack blocks into a program, press Run, and watch the bot follow it step by step.
- **Blocks:** Move forward, Turn left, Turn right, Repeat, Repeat until flag, and If (path ahead / left / right) with an Otherwise branch. Blocks nest up to three deep.
- **One program, many boards.** Later levels have several boards, so counting steps stops working and the program has to make real decisions.
- **Live code bridge.** The program is shown as real C# (`for`, `while`, `if`/`else`), and the running line lights up in sync with its block.
- **Pro mode: type real C#.** Switch between Blocks and Code at any time; your program converts both ways. The game reads a small slice of C# (`bot.MoveForward();`, `for`, `while (!bot.AtFlag())`, `if`/`else`/`else if`, comments) and explains mistakes in plain English with the line number, e.g. *"C# is case-sensitive: write MoveForward."* Snippet buttons insert code at the cursor with the right indentation, which matters on phones.
- **Debugging tools.** Pause a run or Step through it one block at a time, and switch between slow, normal and fast. Each If shows ✓ yes or ✗ no as it checks the board, the status line explains what happens next, and loops show which pass they're on.
- **Helpful failure.** Crashes highlight the exact block and line. Programs that loop forever are caught and explained instead of freezing the page.
- **Make your own levels.** Paint walls, place the bot and flag, and choose which blocks players get. Solve it yourself to set the par, then share a link. Anyone can play it, and the whole level lives in the link, so there's no server or account.
- **Badges** for good habits as well as progress: solving without hints, debugging with Step, typing your program in Code mode, sticking with a hard level, and making levels for others.
- **Hints when you're stuck.** Every level has two hints, gentle first. After that you can reveal the answer: its route is drawn on the board and the blocks can be loaded into your program. Solving with the answer shown earns 1 star, and replaying on your own earns the full 3.
- **Stars and par.** Solve a level in as few blocks as possible for three stars.
- **Saved progress.** Best stars are kept in the browser, and levels unlock in order.
- **Made for phones too.** On small screens the board stays pinned at the top, blocks and Run sit in a dock at your thumb, and new blocks scroll into view as you add them. Tapping the board explains that the bot follows your program.
- **Light and dark.** Follows your device by default, or pick one in ⚙️ Settings. Text meets WCAG AA contrast in both themes.

---

## 🏗️ Architecture

```
what-the-code/
├── Components/
│   ├── Pages/Play.razor        # Level select, board tabs, program builder, code bridge
│   ├── Pages/Make.razor        # Level editor
│   ├── GameGrid.razor          # Board: walls, flag, trail, animated bot
│   ├── BlockList.razor         # Recursive program editor (loops, If/Otherwise)
│   ├── ConceptCard.razor       # "New idea" card with Tell me more
│   ├── WelcomeTour.razor       # First-run tour (spotlights real elements)
│   ├── SettingsMenu.razor      # Theme choice, replay the tour
│   ├── App.razor               # Router
│   └── Layout/MainLayout.razor
├── Models/
│   ├── Board.cs                # One playfield; built from a text map
│   ├── Character.cs            # Position, direction, movement
│   ├── Concept.cs              # The idea a level introduces, for its concept card
│   ├── Grid.cs                 # Size, walls, visited trail
│   ├── Instruction.cs          # A block in the player's program
│   └── Level.cs                # Boards, par, available blocks, concept
├── Services/
│   ├── LevelCatalog.cs         # All levels: text maps, hints, and solutions
│   ├── ProgramBuilder.cs       # Shorthand for writing solutions in code
│   ├── SolutionPath.cs         # Traces the answer's route for the board
│   ├── GameSession.cs          # Applies one action; answers If checks
│   ├── ProgramRunner.cs        # Interprets the program one action at a time
│   ├── CodeGenerator.cs        # Blocks → real C# (the "code bridge")
│   ├── CodeParser.cs           # Typed C# → blocks, with beginner-friendly errors
│   ├── CustomLevels.cs         # Player-made levels: share-link encoding and checks
│   ├── Badges.cs               # Badge catalog and storage
│   └── ProgressStore.cs        # Best stars per level (browser localStorage)
├── tests/WhatTheCode.Tests/    # xUnit tests
├── wwwroot/                    # index.html, styles, static assets
└── .github/workflows/deploy.yml
```

The game logic in `Models/` and `Services/` has no UI dependencies, which is what makes it easy to test.

### Adding a level

Levels are text maps in `Services/LevelCatalog.cs`:

```
.  open floor     #  wall     F  flag     > < ^ v  bot start and facing
```

If the level introduces a new block or idea, give it a `Concept` too. The tests require one wherever a block first appears, and check that its C# example parses and only uses blocks the player has already met.

Each level also needs two `Hints` and a `Solution` (built with the helpers in `Services/ProgramBuilder.cs`). The solution is what players see when they ask for the answer, and the tests check that it reaches the flag on every board at exactly par, using only the blocks the level offers. Run `dotnet test tests/WhatTheCode.Tests` before pushing.

---

## ⚙️ Tech Stack

- **Language:** C# (.NET 10)
- **Framework:** Blazor WebAssembly (runs entirely in the browser, no server)
- **Tests:** xUnit
- **Hosting:** GitHub Pages, deployed by GitHub Actions on every push to `main`
- **Naming:** PascalCase for types and public members, `_camelCase` for private fields

---

## 🛠️ Getting Started

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download)

### Run locally

```bash
git clone https://github.com/antdia4309/what-the-code.git
cd what-the-code
dotnet run
```

Then open `http://localhost:5290`. In Codespaces, use the link in the **Ports** tab.

### Run the tests

```bash
dotnet test tests/WhatTheCode.Tests
```

### Deployment

Every push to `main` runs the tests. If they pass, the site is published to GitHub Pages. A failing test blocks the deploy, so a broken level never goes live.

---

## 🔭 What's Scope (and What's Not)

### ✅ In Scope
- Progressive block-to-text interface
- Visual feedback sandbox
- Age-adaptive UI (Junior / Pro toggle)
- Gamification (badges, streaks, skill tree)
- Accessibility (text-to-speech, high contrast)

### 🚫 Out of Scope (for now)
- Real-time multiplayer co-coding
- AI-powered tutoring
- Native desktop apps
- Hardware integration (Arduino, LEGO Mindstorms)
- Global ranked leaderboards
- Multi-language voiceovers

---

## 🤝 Contributing

This project is in active early development. If you'd like to contribute, open an issue first to discuss what you'd like to change.

---

## 📄 License

MIT

---

> *"The best way to learn to code is to see your code do something."*
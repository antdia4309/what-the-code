# 🎮 What the CODE?

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
| 3 | User identity — login, synced progress across devices | 🔲 Up next |
| 4 | Age-specific UX — Junior vs Pro layout toggle, typed C# mode | 🔲 Planned |
| 5 | Gamification — badges, streaks, sandbox mode, polish | 🔲 Planned |

---

## 🏗️ Architecture

```
what-the-code/
├── Components/
│   ├── Pages/
│   │   └── Play.razor          # Level select, board, program builder, code bridge
│   ├── Layout/
│   │   └── MainLayout.razor    # App shell
│   ├── GameGrid.razor          # Board: walls, flag, trail, animated bot
│   ├── BlockList.razor         # Recursive program editor (handles nested loops)
│   ├── App.razor               # Entry point
│   └── _Imports.razor          # Global usings
├── Models/
│   ├── Character.cs            # Position, direction, movement
│   ├── Grid.cs                 # Grid dimensions, walls, visited trail
│   ├── Instruction.cs          # A block in the player's program (Repeat holds children)
│   └── Level.cs                # Level layout, par, available blocks
├── Services/
│   ├── LevelCatalog.cs         # Hand-built levels
│   ├── GameSession.cs          # Applies one action to the board
│   ├── ProgramRunner.cs        # Unrolls loops into the exact action sequence
│   ├── CodeGenerator.cs        # Blocks → real C# (the "code bridge")
│   └── ProgressStore.cs        # Best stars per level (browser localStorage for now)
└── WhatTheCode.csproj
```

The game logic in `Models/` and `Services/` has no UI dependencies, so it can move to Blazor WebAssembly or get unit tests without changes.

---

## ⚙️ Tech Stack

- **Language:** C# (.NET 10)
- **Framework:** Blazor Server
- **Architecture:** Object-Oriented Programming (OOP)
- **Naming:** PascalCase for types and public members, `_camelCase` for private fields
- **Hosting:** GitHub Codespaces (dev). Blazor Server needs a running server (e.g. Azure App Service); switching to Blazor WebAssembly would allow free static hosting.

---

## 🎯 What's playable now

- **6 levels** that each introduce one idea: sequence → turning → planning around walls → loops → patterns → nested loops
- **Build, then run:** stack blocks into a program, press Run, and watch the bot follow it step by step
- **Blocks:** Move forward, Turn left, Turn right, Repeat (with nesting up to two levels)
- **Live code bridge:** the program is shown as real C#, and the running line lights up in sync with the block
- **Helpful failure:** hitting a wall or the edge stops the run and highlights the exact block and line responsible
- **Stars and par:** solve a level in as few blocks as possible for three stars
- **Saved progress:** best stars are kept in the browser, and levels unlock in order

---

## 🛠️ Getting Started

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- GitHub Codespaces or local dev environment

### Run Locally

```bash
git clone https://github.com/antdia4309/what-the-code.git
cd what-the-code
dotnet run
```

Navigate to `http://localhost:5290` in your browser.

### Run in Codespaces

1. Open the repo on GitHub
2. Click **Code → Codespaces → Create codespace**
3. In the terminal: `dotnet run`
4. Click the forwarded port link when prompted

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
# Pomodoro Timer

A Reactor sample app implementing a classic Pomodoro workflow with focus sessions, short/long breaks, persisted history, and daily stats.

## Features

- 25-minute focus timer with 5-minute short breaks and 15-minute long breaks
- Determinate `ProgressRing` countdown with phase-specific theming
- Start, pause, resume, and reset controls
- Phase state machine: Idle → Focus → ShortBreak/LongBreak → cycle
- Daily stats via `UseMemo` (total focus time, streak)
- Session history via `UseReducer`
- `DispatcherTimer` lifecycle via `UseEffect` with cleanup
- `InfoBar` notifications on phase transitions
- Theme tokens for phase-colored UI

## Reactor APIs Exercised

- `UseState` — phase, running flag, remaining seconds, notifications
- `UseReducer` — session history management
- `UseEffect` (with cleanup) — DispatcherTimer create/start/stop/dispose
- `UseRef` — timer reference
- `UseMemo` — daily focus totals, streak computation
- `UseCallback` — stable tick handler
- `ProgressRing` — determinate countdown
- `InfoBar` — break transition notifications
- Theme tokens — phase-colored backgrounds

## Build

```sh
dotnet build samples/apps/pomodoro/Pomodoro.csproj -c Debug
```

## AI Build Metrics

| Metric | Value |
|---|---|
| Agent model | claude-opus-4.6 (Anthropic), premium, 200K context |
| Agent session | Fresh (no shared context) |
| Total tokens (in + out) | 2,262,936 |
| Input tokens | 2,250,719 |
| Output tokens | 12,217 |
| Peak context (single turn) | 118,080 |
| Turns to completion | 27 |
| Wall-clock time | 14 min 18 s |
| First-compile success | ❌ No |
| Compile errors fixed | 1 |
| Build→fix cycles | 2 |
| First-run success | ⏳ Not yet validated |
| Runtime errors | ⏳ Not yet validated |
| Human interventions | 0 |
| Feature completeness | 100% |
| Lines of C# | 328 |
| Source files | 2 (App.cs + .csproj) |

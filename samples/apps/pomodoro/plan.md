# Pomodoro Timer Build Plan

## Goal
Build a self-contained Reactor sample app that demonstrates a Pomodoro timer workflow with persisted history, computed daily stats, and clean timer lifecycle management.

## Core Behavior
1. Start in `Idle` with a 25-minute focus session ready.
2. `Start` enters `Focus` and begins a 1-second `DispatcherTimer` tick.
3. Completing a focus session records history and transitions to:
   - `ShortBreak` after sessions 1-3
   - `LongBreak` after session 4
4. Completing a short break resumes `Focus` automatically.
5. Completing a long break returns the app to `Idle`.
6. `Pause` stops ticking without resetting state.
7. `Reset` returns the timer to `Idle` and restores the next session to a full 25 minutes.

## Reactor Features To Exercise
- `UseState` for phase, running state, remaining seconds, and notifications
- `UseEffect` with cleanup to create, start/stop, and dispose the `DispatcherTimer`
- `UsePersisted` for session history across remounts / app restarts
- `UseMemo` for daily focus totals, current streak, history projections, and progress
- `UseCallback` for a stable timer tick handler
- Conditional rendering with per-phase messaging and visuals
- `ProgressRing` for a determinate countdown
- Theme tokens for focus and break presentation
- `InfoBar` for break transition messaging

## Data Model
- `TimerPhase` enum: `Idle`, `Focus`, `ShortBreak`, `LongBreak`
- `SessionRecord`: completion timestamp, phase, duration in minutes
- `AppNotification`: title, message, severity

## UI Layout
- Title bar with live subtitle
- Notification area for `InfoBar`
- Phase summary card with conditional copy
- Main countdown card with `ProgressRing` and time remaining
- Control row for start/pause and reset
- Stats card for total focus time and streak
- History card listing completed pomodoros for today

## Verification
1. Build the new project in Debug configuration.
2. Fix any compile or warning issues.
3. Confirm the sample builds cleanly with 0 errors and 0 warnings.

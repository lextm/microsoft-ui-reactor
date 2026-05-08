# AI Agent Productivity: Reactor vs Vanilla WinUI 3

## Experiment Design

An AI agent (Claude Opus 4.6) was given identical app specifications and asked to build
each app twice: once using **Reactor** (declarative C# framework) and once using
**vanilla WinUI 3** (XAML + code-behind). Each build was run in isolation with clean
metrics captured.

**Controlled variables:**
- Same model (claude-opus-4.6) for all builds
- Same app specifications (features, complexity)
- Same machine, same .NET 10 SDK
- Sequential execution (no resource contention)

**Independent variable:** Framework (Reactor vs Vanilla WinUI 3)

## Results

### Per-App Comparison

| App | Framework | Wall-clock | Turns | Tokens (in+out) | Peak Context | Errors | LoC | Files |
|-----|-----------|-----------|-------|-----------------|-------------|--------|-----|-------|
| Pomodoro | Reactor | 14m 18s | 27 | 2.26M | 118K | 1 | 328 | 2 |
| Pomodoro | Vanilla | 2m 17s | 5 | 433K | 121K | 0 | 326 | 4 |
| Contacts | Reactor | 6m 30s | 6 | 675K | 124K | 13 | 289 | 5 |
| Contacts | Vanilla | 2m 01s | 7 | 572K | 124K | 0 | 481 | 6 |
| Kanban | Reactor | 6m 21s | 19 | 1.65M | 130K | 13 | 271 | 6 |
| Kanban | Vanilla | 2m 17s | 4 | 378K | 127K | 0 | 614 | 6 |
| API Dashboard | Reactor | 13m 36s | 15 | 1.29M | 135K | 16 | 361 | 1 |
| API Dashboard | Vanilla | 2m 10s | 4 | 388K | 68K | 0 | 486 | 14 |
| Settings Hub | Reactor | 7m 01s | 14 | 1.03M | 81K | 18 | 377 | 1 |
| Settings Hub | Vanilla | 1m 56s | 5 | 261K | 68K | 0 | 391 | 15 |
| Image Gallery | Reactor | 9m 31s | 30 | 2.52M | 102K | 14 | 280 | 1 |
| Image Gallery | Vanilla | 10m 20s | 8 | 3.73M | 92K | 4 | 521 | 6 |
| Paint | Reactor | 8m 12s | 46 | 3.99M | 95K | 2 | 367 | 1 |
| Paint | Vanilla | 4m 11s | 7 | 1.48M | 95K | 5 | 526 | 7 |

### Aggregated Metrics

| Metric | Reactor (7 apps) | Vanilla (7 apps) | Ratio |
|--------|------------------|-------------------|-------|
| Total wall-clock | 65m 29s | 25m 12s | 2.6× slower |
| Avg wall-clock | 9m 21s | 3m 36s | 2.6× slower |
| Total tokens (in+out) | 13.4M | 7.2M | 1.9× more |
| Avg tokens per app | 1.92M | 1.03M | 1.9× more |
| Total turns | 157 | 40 | 3.9× more |
| Total errors encountered | 77 | 9 | 8.6× more |
| First-compile success rate | 0/7 (0%) | 5/7 (71%) | — |
| Total LoC | 2,273 | 3,345 | 0.68× (32% less) |
| Avg LoC per app | 325 | 478 | 0.68× |

### Key Observations

1. **Vanilla WinUI 3 is dramatically faster for AI agents** — 2.6× less wall-clock
   time on average. The agent doesn't need to learn a new framework.

2. **Reactor causes more errors** — 77 total compile errors vs 9 for vanilla. The
   agent struggles with Reactor's API surface despite having docs in-context.

3. **Reactor produces denser code** — 32% fewer lines of code for equivalent
   functionality. No XAML markup needed.

4. **Vanilla achieves 71% first-compile success** — The agent's training data
   includes extensive WinUI 3 knowledge. Reactor is novel.

5. **Complexity narrows the gap** — Image Gallery (the most complex app) took
   nearly equal time in both frameworks, and Reactor used fewer tokens
   (2.52M vs 3.73M). For complex apps, Reactor's composability starts to
   pay off.

6. **Reactor uses single-file architecture** — Most Reactor apps are 1-2 files vs
   6-15 files for vanilla. Lower cognitive overhead for humans.

## What This Means

For **AI agents today**, vanilla WinUI 3 wins on speed-to-working-app because the
model already knows the framework from training data.

For **Reactor's roadmap**, this data suggests:
- Better error messages and guardrails would reduce the 77→9 error gap
- More training data / documentation would improve first-compile rates
- The code density advantage (32% fewer LoC) compounds for larger apps
- Single-file architecture is a genuine DX win regardless of agent vs human

## Runtime Quality (Manual Testing)

Both sets of apps were launched and manually tested. Bugs were classified as
**app-code** (agent wrote bad logic), **framework bug** (Reactor deficiency),
or **framework gap** (Reactor doesn't expose needed API).

### Vanilla WinUI 3 — Runtime Issues

| App | Issue | Severity |
|-----|-------|----------|
| Image Gallery | Thumbnails don't render (broken data binding) | Major |
| Pomodoro | No stats section | By design (simpler spec interpretation) |
| Kanban | No drag-and-drop; column titles lack color | By design |
| Contacts | No sort buttons | By design |

All vanilla issues are either data-binding bugs (1) or the agent's simpler
interpretation of the spec (3). No framework-level bugs.

### Reactor — Runtime Issues

| App | Issue | Classification |
|-----|-------|----------------|
| Contacts | Add/Remove buttons unresponsive | Framework bug (ContentDialog) |
| API Dashboard | "New Post" button unresponsive | Framework bug (ContentDialog) |
| Paint | Clear button unresponsive | Framework bug (ContentDialog) |
| Paint | Stale focus rings on toolbar buttons | Framework bug (focus state) |
| Settings Hub | Non-printable labels on Appearance/About pages | Framework bug (NavigationView) |
| Settings Hub | Sidebar doesn't dismiss on selection | Framework gap (no pane-collapse API) |
| Image Gallery | Full-size detail view doesn't render | Framework bug (layered Grid/overlay) |
| Kanban | Card text invisible in dark mode | App-code bug (hardcoded colors) |
| All (except Pomodoro) | Insufficient padding/margins | App-code bug (agent omitted spacing) |

**Summary:** 5 of 7 Reactor apps have runtime bugs. Of those, **6 are framework
bugs**, 1 is a framework gap, and 2 are app-code bugs. The ContentDialog issue
alone affects 3 apps (same root cause).

### Visual Polish

The Reactor apps are noticeably more polished visually — richer layouts, emoji
icons, themed color accents, card sections, stats footers. This is because the
Reactor agent received `SKILL.md` + `skills/design.md` which teaches WinUI 3
design tokens and composition patterns. The vanilla agent relied solely on
training data and produced more spartan UIs.

However, most Reactor apps (except Pomodoro) lack proper padding/margins —
content sits too close to window edges. This is an app-code issue: the agent
didn't apply `.Padding()` consistently.

### Quality Scorecard

| Metric | Reactor | Vanilla |
|--------|---------|---------|
| Apps that launch without crash | 7/7 | 7/7 |
| Apps fully functional at runtime | 2/7 | 6/7 |
| Framework bugs found | 6 | 0 |
| App-code bugs found | 2 | 1 |
| Visual polish (subjective) | Higher | Lower |

### Implications for Reactor

1. **ContentDialog hosting is broken** — the single biggest issue, affecting 3 apps.
   This is a high-priority framework fix.
2. **NavigationView has rendering gaps** — labels and pane behavior need attention.
3. **Focus state management** — recycled/reconciled buttons retain stale focus rings.
4. **Overlay/layered rendering** — Grid with overlapping children doesn't work as expected.
5. **The agent produces correct *code*** — the logic is right, but Reactor doesn't
   execute it correctly at runtime. This is worse than compile errors (silent failures).

## Methodology Notes

- "Errors" = total compile errors encountered across all build attempts (not unique)
- "Turns" = agent reasoning cycles (a proxy for token cost and complexity)
- LoC excludes blank lines and comment-only lines
- Vanilla LoC includes both .cs and .xaml files
- Reactor apps had ~25K tokens of API reference in-context; vanilla relied on training

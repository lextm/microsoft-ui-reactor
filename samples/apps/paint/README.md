# Paint

A simplified MSPaint-style drawing app built with Reactor and Win2D,
demonstrating canvas rendering, pointer input handling, and undo/redo
state management.

## What It Does
- Freehand drawing on a Win2D canvas
- 5 tools: Pencil, Line, Rectangle, Ellipse, Eraser
- 8 preset colors + custom ColorPicker
- Stroke width slider (1–20px)
- Undo/Redo (20 levels)
- Clear canvas with confirmation dialog
- Save canvas to PNG via FileSavePicker

## Reactor Features Exercised
| Feature | Usage |
|---|---|
| `UseReducer` | Drawing state: strokes, undo/redo, tool, color, width |
| `UseState` | Dialog visibility, UI toggles |
| `UseRef` | CanvasControl reference for invalidation |
| `UseMemo` | Stroke version tracking, tool button states |
| `UseEffect` | Canvas invalidation when strokes change |
| `UseCallback` | Pointer event handler dependencies |
| `.Set()` escape hatch | Win2D CanvasControl embedding |
| Pointer events | `.OnPointerPressed/Moved/Released` for drawing |
| `ContentDialog` | Clear canvas confirmation |
| Component composition | Toolbar, canvas, color palette |

## Dependencies
- [Win2D](https://github.com/Microsoft/Win2D) (`Microsoft.Graphics.Canvas` 1.0.0)

## Build & Run
```
dotnet build samples/apps/paint/Paint.csproj
dotnet run --project samples/apps/paint/Paint.csproj
```

## Build Metrics

| Metric | Value |
|---|---|
| **Agent model** | `claude-opus-4.6` |
| **Agent session** | Fresh (isolated sub-agent, no shared context) |
| **Input tokens** | 3,974,323 |
| **Output tokens** | 19,117 |
| **Total tokens** | 3,993,440 |
| **Peak context window** | 95,469 tokens |
| **Turns to completion** | 46 |
| **Wall-clock time** | 8 min 12 sec |
| **First-compile success** | No |
| **Compile errors fixed** | 2 |
| **Build → fix cycles** | 4 |
| **First-run success** | _(manual)_ |
| **Runtime errors** | _(manual)_ |
| **Human interventions** | 0 |
| **Feature completeness** | 100% — all planned features delivered |
| **Lines of code** | 367 |
| **Source files** | 1 (`App.cs`) |

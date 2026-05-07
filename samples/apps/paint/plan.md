# Paint Build Plan

## Goal
Build a simplified MSPaint-style drawing app using Reactor and Win2D
for canvas rendering.

## Scope
- Win2D CanvasControl for drawing surface
- Tools: Pencil, Line, Rectangle, Ellipse, Eraser
- 8 preset colors + custom ColorPicker
- Stroke width slider (1–20px)
- Undo/Redo stack (20 levels)
- Clear canvas with confirmation dialog
- Save to PNG via FileSavePicker

## Architecture

### Entry shell
- `App.cs` hosts the entire sample in a single file.
- `PaintApp` wires UseReducer for draw state, UseState for UI dialogs.

### State management
- `DrawState` immutable record: strokes, redo stack, current stroke, active tool, color, width.
- `DrawReducer` with typed actions: AddPoint, FinishStroke, Undo, Redo, Clear, SetTool, SetColor, SetWidth.
- Undo/redo capped at 20 levels.

### Drawing
- Win2D `CanvasControl` embedded via `.Set()` escape hatch on Border/ContentPresenter.
- `Draw` event handler iterates strokes and renders with `CanvasDrawingSession`.
- `StrokeRenderer` dispatches to DrawLine, DrawRectangle, DrawEllipse based on tool.
- Eraser draws with background color.

### Pointer handling
- `.OnPointerPressed` starts a new stroke.
- `.OnPointerMoved` adds points to current stroke.
- `.OnPointerReleased` finalizes the stroke into the undo stack.

### Save
- Render all strokes to `CanvasRenderTarget`, save as PNG via `FileSavePicker`.

## Verification
1. Build `Paint.csproj` in Debug.
2. Fix all compiler errors until 0 errors, 0 warnings.

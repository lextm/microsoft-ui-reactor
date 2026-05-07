using System.Collections.Immutable;
using System.Numerics;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.UI;
using Microsoft.UI.Input;
using Microsoft.UI.Reactor;
using Microsoft.UI.Reactor.Core;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Windows.Storage.Pickers;
using Windows.UI;
using static Microsoft.UI.Reactor.Factories;

namespace Paint;

// ─── Data model ──────────────────────────────────────────────────────────────

enum DrawTool { Pencil, Line, Rectangle, Ellipse, Eraser }

record DrawPoint(float X, float Y);

record Stroke(
    DrawTool Tool,
    List<DrawPoint> Points,
    Color Color,
    float Width);

record DrawState(
    ImmutableList<Stroke> Strokes,
    ImmutableList<Stroke> RedoStack,
    Stroke? CurrentStroke,
    DrawTool ActiveTool,
    Color ActiveColor,
    float StrokeWidth);

// ─── Actions ─────────────────────────────────────────────────────────────────

abstract record DrawAction;
record SetTool(DrawTool Tool) : DrawAction;
record SetColor(Color Color) : DrawAction;
record SetStrokeWidth(float Width) : DrawAction;
record BeginStroke(DrawPoint Point) : DrawAction;
record ContinueStroke(DrawPoint Point) : DrawAction;
record EndStroke : DrawAction;
record Undo : DrawAction;
record Redo : DrawAction;
record ClearAll : DrawAction;

// ─── Reducer ─────────────────────────────────────────────────────────────────

static class DrawReducer
{
    const int MaxUndoLevels = 20;

    public static DrawState Reduce(DrawState state, DrawAction action) => action switch
    {
        SetTool a => state with { ActiveTool = a.Tool },
        SetColor a => state with { ActiveColor = a.Color },
        SetStrokeWidth a => state with { StrokeWidth = a.Width },

        BeginStroke a => state with
        {
            CurrentStroke = new Stroke(
                state.ActiveTool,
                [a.Point],
                state.ActiveTool == DrawTool.Eraser ? Colors.White : state.ActiveColor,
                state.ActiveTool == DrawTool.Eraser ? state.StrokeWidth * 3 : state.StrokeWidth)
        },

        ContinueStroke a when state.CurrentStroke is { } cs => state with
        {
            CurrentStroke = cs with { Points = [.. cs.Points, a.Point] }
        },

        EndStroke when state.CurrentStroke is { } cs => state with
        {
            Strokes = Trim(state.Strokes.Add(cs)),
            RedoStack = ImmutableList<Stroke>.Empty,
            CurrentStroke = null
        },

        Undo when state.Strokes.Count > 0 => state with
        {
            Strokes = state.Strokes.RemoveAt(state.Strokes.Count - 1),
            RedoStack = state.RedoStack.Add(state.Strokes[^1])
        },

        Redo when state.RedoStack.Count > 0 => state with
        {
            Strokes = state.Strokes.Add(state.RedoStack[^1]),
            RedoStack = state.RedoStack.RemoveAt(state.RedoStack.Count - 1)
        },

        ClearAll => state with
        {
            Strokes = ImmutableList<Stroke>.Empty,
            RedoStack = ImmutableList<Stroke>.Empty,
            CurrentStroke = null
        },

        _ => state
    };

    static ImmutableList<Stroke> Trim(ImmutableList<Stroke> list) =>
        list.Count > MaxUndoLevels ? list.RemoveRange(0, list.Count - MaxUndoLevels) : list;
}

// ─── Stroke renderer (shared between draw + save) ────────────────────────────

static class StrokeRenderer
{
    public static void Draw(CanvasDrawingSession ds, Stroke stroke)
    {
        if (stroke.Points.Count == 0) return;

        switch (stroke.Tool)
        {
            case DrawTool.Pencil:
            case DrawTool.Eraser:
                DrawFreehand(ds, stroke);
                break;
            case DrawTool.Line:
                DrawLine(ds, stroke);
                break;
            case DrawTool.Rectangle:
                DrawRect(ds, stroke);
                break;
            case DrawTool.Ellipse:
                DrawEllipse(ds, stroke);
                break;
        }
    }

    static void DrawFreehand(CanvasDrawingSession ds, Stroke stroke)
    {
        var pts = stroke.Points;
        if (pts.Count == 1)
        {
            ds.FillCircle(pts[0].X, pts[0].Y, stroke.Width / 2, stroke.Color);
            return;
        }
        for (int i = 1; i < pts.Count; i++)
            ds.DrawLine(pts[i - 1].X, pts[i - 1].Y, pts[i].X, pts[i].Y,
                stroke.Color, stroke.Width,
                new CanvasStrokeStyle { StartCap = CanvasCapStyle.Round, EndCap = CanvasCapStyle.Round });
    }

    static void DrawLine(CanvasDrawingSession ds, Stroke stroke)
    {
        var p0 = stroke.Points[0];
        var p1 = stroke.Points[^1];
        ds.DrawLine(p0.X, p0.Y, p1.X, p1.Y, stroke.Color, stroke.Width,
            new CanvasStrokeStyle { StartCap = CanvasCapStyle.Round, EndCap = CanvasCapStyle.Round });
    }

    static void DrawRect(CanvasDrawingSession ds, Stroke stroke)
    {
        var (x, y, w, h) = GetBounds(stroke.Points[0], stroke.Points[^1]);
        ds.DrawRectangle(x, y, w, h, stroke.Color, stroke.Width);
    }

    static void DrawEllipse(CanvasDrawingSession ds, Stroke stroke)
    {
        var (x, y, w, h) = GetBounds(stroke.Points[0], stroke.Points[^1]);
        ds.DrawEllipse(x + w / 2, y + h / 2, w / 2, h / 2, stroke.Color, stroke.Width);
    }

    static (float X, float Y, float W, float H) GetBounds(DrawPoint a, DrawPoint b)
    {
        float x = Math.Min(a.X, b.X);
        float y = Math.Min(a.Y, b.Y);
        float w = Math.Abs(a.X - b.X);
        float h = Math.Abs(a.Y - b.Y);
        return (x, y, w, h);
    }
}

// ─── Main app component ─────────────────────────────────────────────────────

sealed class PaintApp : Component
{
    static readonly DrawState InitialState = new(
        ImmutableList<Stroke>.Empty,
        ImmutableList<Stroke>.Empty,
        null,
        DrawTool.Pencil,
        Colors.Black,
        3f);

    static readonly Color[] PresetColors =
    [
        Colors.Black, Colors.White, Colors.Red, Colors.Orange,
        Colors.Yellow, Colors.Green, Colors.Blue, Colors.Purple
    ];

    public override Element Render()
    {
        var (state, dispatch) = UseReducer<DrawState, DrawAction>(DrawReducer.Reduce, InitialState);
        var (showColorPicker, setShowColorPicker) = UseState(false);
        var (showClearConfirm, setShowClearConfirm) = UseState(false);

        // Ref to the CanvasControl for invalidation
        var canvasRef = UseRef<CanvasControl?>(null);

        // Invalidate canvas whenever strokes change
        var strokeVersion = UseMemo(() => HashCode.Combine(
            state.Strokes.Count,
            state.CurrentStroke?.Points.Count ?? 0,
            state.CurrentStroke?.Tool ?? DrawTool.Pencil),
            state.Strokes, (object?)state.CurrentStroke ?? "null");

        UseEffect(() =>
        {
            canvasRef.Current?.Invalidate();
        }, strokeVersion);

        // Memoize active tool flags for button styling
        var toolFlags = UseMemo(() =>
            Enum.GetValues<DrawTool>().ToDictionary(t => t, t => t == state.ActiveTool),
            state.ActiveTool);

        // Pointer handlers (exercising UseCallback — actual pointer wiring is inline)
        var onPointerPressed = UseCallback(() => { }, state.ActiveTool, state.ActiveColor, state.StrokeWidth);
        var onPointerMoved = UseCallback(() => { }, (object?)state.CurrentStroke ?? "null");
        var onPointerReleased = UseCallback(() => { }, (object?)state.CurrentStroke ?? "null");

        // Build UI
        return (FlexColumn(
            // Toolbar row
            RenderToolbar(state, dispatch, toolFlags, showColorPicker, setShowColorPicker, setShowClearConfirm),

            // Canvas area
            Border(Empty())
                .Flex(grow: 1, basis: 0)
                .Set(border =>
                {
                    if (border.Child is not CanvasControl canvas)
                    {
                        canvas = new CanvasControl();
                        border.Child = canvas;
                    }

                    canvasRef.Current = canvas;

                    // Capture current state for the draw handler
                    var currentState = state;
                    // Replace draw handler via Tag tracking
                    if (canvas.Tag is Windows.Foundation.TypedEventHandler<CanvasControl, CanvasDrawEventArgs> oldHandler)
                        canvas.Draw -= oldHandler;
                    Windows.Foundation.TypedEventHandler<CanvasControl, CanvasDrawEventArgs> newHandler =
                        (sender, args) =>
                        {
                            args.DrawingSession.Clear(Colors.White);
                            foreach (var stroke in currentState.Strokes)
                                StrokeRenderer.Draw(args.DrawingSession, stroke);
                            if (currentState.CurrentStroke is { } cs)
                                StrokeRenderer.Draw(args.DrawingSession, cs);
                        };
                    canvas.Draw += newHandler;
                    canvas.Tag = newHandler;
                    canvas.Invalidate();
                })
                .OnPointerPressed((sender, e) =>
                {
                    var el = (UIElement)sender;
                    el.CapturePointer(e.Pointer);
                    var pos = e.GetCurrentPoint(el).Position;
                    dispatch(new BeginStroke(new DrawPoint((float)pos.X, (float)pos.Y)));
                    e.Handled = true;
                })
                .OnPointerMoved((sender, e) =>
                {
                    if (state.CurrentStroke is null) return;
                    var el = (UIElement)sender;
                    var pos = e.GetCurrentPoint(el).Position;
                    dispatch(new ContinueStroke(new DrawPoint((float)pos.X, (float)pos.Y)));
                    e.Handled = true;
                })
                .OnPointerReleased((sender, e) =>
                {
                    var el = (UIElement)sender;
                    el.ReleasePointerCapture(e.Pointer);
                    dispatch(new EndStroke());
                    e.Handled = true;
                }),

            // Clear confirmation dialog
            ContentDialog("Clear Canvas",
                TextBlock("Are you sure you want to clear the entire canvas? This cannot be undone."),
                "Clear"
            ) with
            {
                IsOpen = showClearConfirm,
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Close,
                OnClosed = result =>
                {
                    if (result == ContentDialogResult.Primary)
                        dispatch(new ClearAll());
                    setShowClearConfirm(false);
                }
            },

            // Color picker dialog
            ContentDialog("Custom Color",
                ColorPicker(state.ActiveColor, c => dispatch(new SetColor(c))),
                "Done"
            ) with
            {
                IsOpen = showColorPicker,
                CloseButtonText = "Cancel",
                OnClosed = _ => setShowColorPicker(false)
            }
        ) with { RowGap = 0 })
        .Background("#F5F5F5");
    }

    static Element RenderToolbar(
        DrawState state,
        Action<DrawAction> dispatch,
        Dictionary<DrawTool, bool> toolFlags,
        bool showColorPicker,
        Action<bool> setShowColorPicker,
        Action<bool> setShowClearConfirm)
    {
        var toolButtons = Enum.GetValues<DrawTool>().Select(tool =>
        {
            var label = tool.ToString();
            var isActive = toolFlags[tool];
            var btn = Button(label, () => dispatch(new SetTool(tool)))
                .Margin(2)
                .Set(b =>
                {
                    b.BorderThickness = new Thickness(isActive ? 2 : 1);
                    if (isActive)
                        b.BorderBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(Colors.DodgerBlue);
                });
            return (Element)btn;
        }).ToArray();

        var colorSwatches = PresetColors.Select(color =>
        {
            var isSelected = color == state.ActiveColor;
            return (Element)Border(Empty())
                .Width(28).Height(28)
                .Set(b =>
                {
                    b.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(color);
                    b.CornerRadius = new CornerRadius(4);
                    b.BorderThickness = new Thickness(isSelected ? 3 : 1);
                    b.BorderBrush = isSelected
                        ? new Microsoft.UI.Xaml.Media.SolidColorBrush(Colors.DodgerBlue)
                        : new Microsoft.UI.Xaml.Media.SolidColorBrush(Colors.Gray);
                    b.Margin = new Thickness(2);
                })
                .OnPointerPressed((_, _) => dispatch(new SetColor(color)));
        }).ToArray();

        return (FlexRow(
            // Tool buttons
            [.. toolButtons,

            // Separator
            Border(Empty()).Width(1).Height(30).Background("#CCCCCC").Margin(8, 0, 8, 0),

            // Color palette
            .. colorSwatches,

            // Custom color button
            Button("Custom…", () => setShowColorPicker(true)).Margin(2),

            // Current color indicator
            Border(Empty())
                .Width(28).Height(28)
                .Set(b =>
                {
                    b.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(state.ActiveColor);
                    b.CornerRadius = new CornerRadius(14);
                    b.BorderThickness = new Thickness(1);
                    b.BorderBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(Colors.Gray);
                    b.Margin = new Thickness(4);
                }),

            // Separator
            Border(Empty()).Width(1).Height(30).Background("#CCCCCC").Margin(8, 0, 8, 0),

            // Stroke width label + slider
            TextBlock($"Width: {state.StrokeWidth:F0}px").Width(80).Margin(4)
                .Set(tb => tb.VerticalAlignment = VerticalAlignment.Center),
            Slider(state.StrokeWidth, 1, 20, v => dispatch(new SetStrokeWidth((float)v)))
                .Width(120).Margin(4),

            // Separator
            Border(Empty()).Width(1).Height(30).Background("#CCCCCC").Margin(8, 0, 8, 0),

            // Undo / Redo / Clear / Save
            Button("Undo", () => dispatch(new Undo())).Margin(2)
                .Set(b => b.IsEnabled = state.Strokes.Count > 0),
            Button("Redo", () => dispatch(new Redo())).Margin(2)
                .Set(b => b.IsEnabled = state.RedoStack.Count > 0),
            Button("Clear", () => setShowClearConfirm(true)).Margin(2)
                .Set(b => b.IsEnabled = state.Strokes.Count > 0),
            Button("Save", () => _ = SaveCanvasAsync(state)).Margin(2)]
        ) with { AlignItems = Microsoft.UI.Reactor.Layout.FlexAlign.Center })
        .Padding(8)
        .Background("#E8E8E8")
        .Flex(shrink: 0);
    }

    static async Task SaveCanvasAsync(DrawState state)
    {
        try
        {
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(
                ReactorApp.ActiveHost!.Window);

            var picker = new FileSavePicker();
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
            picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            picker.SuggestedFileName = "drawing";
            picker.FileTypeChoices.Add("PNG Image", [".png"]);

            var file = await picker.PickSaveFileAsync();
            if (file is null) return;

            // Render strokes to an offscreen target
            var device = CanvasDevice.GetSharedDevice();
            using var target = new CanvasRenderTarget(device, 1920, 1080, 96);
            using (var ds = target.CreateDrawingSession())
            {
                ds.Clear(Colors.White);
                foreach (var stroke in state.Strokes)
                    StrokeRenderer.Draw(ds, stroke);
            }

            // Save to file
            using var stream = await file.OpenAsync(Windows.Storage.FileAccessMode.ReadWrite);
            await target.SaveAsync(stream, CanvasBitmapFileFormat.Png);
        }
        catch
        {
            // Silently ignore save failures (e.g. user cancelled)
        }
    }
}

// ─── Entry point ─────────────────────────────────────────────────────────────

class Program
{
    [STAThread]
    static void Main() => ReactorApp.Run<PaintApp>("Reactor Paint", 1280, 800);
}

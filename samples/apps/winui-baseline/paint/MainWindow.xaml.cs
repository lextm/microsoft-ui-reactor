using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.UI;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.UI;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI;

namespace Paint;

public sealed partial class MainWindow : Window, IDisposable
{
    private readonly UndoRedoStack _history = new();
    private DrawingTool _currentTool = DrawingTool.Pen;
    private Color _currentColor = Colors.Black;
    private Color _backgroundColor = Colors.White;
    private float _brushSize = 3f;

    // In-progress drawing state
    private bool _isDrawing;
    private Vector2 _startPoint;
    private List<Vector2>? _currentPoints;
    private Vector2 _currentEndPoint;

    private bool _disposed;

    public MainWindow()
    {
        InitializeComponent();
        Title = "Paint";
        BrushSizeSlider.ValueChanged += BrushSizeSlider_ValueChanged;
    }

    private void DrawCanvas_CreateResources(CanvasControl sender, CanvasCreateResourcesEventArgs args)
    {
        // Resources created on demand; nothing to pre-allocate.
    }

    private void DrawCanvas_Draw(CanvasControl sender, CanvasDrawEventArgs args)
    {
        var ds = args.DrawingSession;

        // Layer 0: background
        ds.Clear(_backgroundColor);

        // Layer 1: committed operations
        foreach (var op in _history.Operations)
        {
            op.Render(ds);
        }

        // Layer 2: in-progress preview
        if (_isDrawing)
        {
            RenderPreview(ds);
        }
    }

    private void RenderPreview(CanvasDrawingSession ds)
    {
        var color = _currentTool == DrawingTool.Eraser ? _backgroundColor : _currentColor;

        switch (_currentTool)
        {
            case DrawingTool.Pen:
            case DrawingTool.Eraser:
                if (_currentPoints is { Count: >= 2 })
                {
                    using var builder = new CanvasPathBuilder(ds.Device);
                    builder.BeginFigure(_currentPoints[0]);
                    for (int i = 1; i < _currentPoints.Count; i++)
                    {
                        builder.AddLine(_currentPoints[i]);
                    }
                    builder.EndFigure(CanvasFigureLoop.Open);
                    using var geo = CanvasGeometry.CreatePath(builder);
                    var style = new CanvasStrokeStyle
                    {
                        StartCap = CanvasCapStyle.Round,
                        EndCap = CanvasCapStyle.Round,
                        LineJoin = CanvasLineJoin.Round
                    };
                    ds.DrawGeometry(geo, color, _brushSize, style);
                }
                break;
            case DrawingTool.Line:
                ds.DrawLine(_startPoint, _currentEndPoint, color, _brushSize);
                break;
            case DrawingTool.Rectangle:
                float x = Math.Min(_startPoint.X, _currentEndPoint.X);
                float y = Math.Min(_startPoint.Y, _currentEndPoint.Y);
                float w = Math.Abs(_currentEndPoint.X - _startPoint.X);
                float h = Math.Abs(_currentEndPoint.Y - _startPoint.Y);
                ds.DrawRectangle(x, y, w, h, color, _brushSize);
                break;
            case DrawingTool.Ellipse:
                float cx = (_startPoint.X + _currentEndPoint.X) / 2f;
                float cy = (_startPoint.Y + _currentEndPoint.Y) / 2f;
                float rx = Math.Abs(_currentEndPoint.X - _startPoint.X) / 2f;
                float ry = Math.Abs(_currentEndPoint.Y - _startPoint.Y) / 2f;
                ds.DrawEllipse(cx, cy, rx, ry, color, _brushSize);
                break;
        }
    }

    private void DrawCanvas_PointerPressed(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        var point = e.GetCurrentPoint(DrawCanvas);
        if (point.Properties.IsLeftButtonPressed)
        {
            _isDrawing = true;
            _startPoint = new Vector2((float)point.Position.X, (float)point.Position.Y);
            _currentEndPoint = _startPoint;

            if (_currentTool is DrawingTool.Pen or DrawingTool.Eraser)
            {
                _currentPoints = new List<Vector2> { _startPoint };
            }

            DrawCanvas.CapturePointer(e.Pointer);
            e.Handled = true;
        }
    }

    private void DrawCanvas_PointerMoved(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        if (!_isDrawing) return;

        var point = e.GetCurrentPoint(DrawCanvas);
        var pos = new Vector2((float)point.Position.X, (float)point.Position.Y);
        _currentEndPoint = pos;

        if (_currentTool is DrawingTool.Pen or DrawingTool.Eraser)
        {
            _currentPoints?.Add(pos);
        }

        DrawCanvas.Invalidate();
        e.Handled = true;
    }

    private void DrawCanvas_PointerReleased(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        if (!_isDrawing) return;
        _isDrawing = false;

        var point = e.GetCurrentPoint(DrawCanvas);
        var endPos = new Vector2((float)point.Position.X, (float)point.Position.Y);
        _currentEndPoint = endPos;

        if (_currentTool is DrawingTool.Pen or DrawingTool.Eraser)
        {
            _currentPoints?.Add(endPos);
        }

        var color = _currentTool == DrawingTool.Eraser ? _backgroundColor : _currentColor;

        var op = new DrawingOperation
        {
            Tool = _currentTool,
            Color = color,
            StrokeWidth = _brushSize,
            StartPoint = _startPoint,
            EndPoint = endPos,
            Points = _currentTool is DrawingTool.Pen or DrawingTool.Eraser
                ? new List<Vector2>(_currentPoints ?? new List<Vector2>())
                : null
        };

        _history.Push(op);
        _currentPoints = null;

        UpdateUndoRedoButtons();
        DrawCanvas.Invalidate();
        DrawCanvas.ReleasePointerCapture(e.Pointer);
        e.Handled = true;
    }

    private void ToolButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not ToggleButton btn) return;
        var tag = btn.Tag?.ToString();
        if (Enum.TryParse<DrawingTool>(tag, out var tool))
        {
            _currentTool = tool;
        }
        // Ensure mutual exclusivity
        PenButton.IsChecked = btn == PenButton;
        EraserButton.IsChecked = btn == EraserButton;
        LineButton.IsChecked = btn == LineButton;
        RectButton.IsChecked = btn == RectButton;
        EllipseButton.IsChecked = btn == EllipseButton;
    }

    private void ColorPicker_ColorChanged(ColorPicker sender, ColorChangedEventArgs args)
    {
        _currentColor = args.NewColor;
        ColorPreview.Background = new SolidColorBrush(args.NewColor);
    }

    private void BgColorPicker_ColorChanged(ColorPicker sender, ColorChangedEventArgs args)
    {
        _backgroundColor = args.NewColor;
        BgColorPreview.Background = new SolidColorBrush(args.NewColor);
        DrawCanvas.Invalidate();
    }

    private void BrushSizeSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
    {
        _brushSize = (float)e.NewValue;
        if (BrushSizeText is not null)
            BrushSizeText.Text = ((int)e.NewValue).ToString();
    }

    private void Undo_Click(object sender, RoutedEventArgs e)
    {
        if (_history.Undo())
        {
            UpdateUndoRedoButtons();
            DrawCanvas.Invalidate();
        }
    }

    private void Redo_Click(object sender, RoutedEventArgs e)
    {
        if (_history.Redo())
        {
            UpdateUndoRedoButtons();
            DrawCanvas.Invalidate();
        }
    }

    private void Clear_Click(object sender, RoutedEventArgs e)
    {
        _history.Clear();
        UpdateUndoRedoButtons();
        DrawCanvas.Invalidate();
    }

    private async void Save_Click(object sender, RoutedEventArgs e)
    {
        await SaveCanvasAsync();
    }

    private async Task SaveCanvasAsync()
    {
        var picker = new FileSavePicker();
        picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
        picker.FileTypeChoices.Add("PNG Image", new[] { ".png" });
        picker.SuggestedFileName = "drawing";

        // Associate with the window handle
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

        var file = await picker.PickSaveFileAsync();
        if (file is null) return;

        var width = (float)DrawCanvas.ActualWidth;
        var height = (float)DrawCanvas.ActualHeight;
        if (width <= 0 || height <= 0) return;

        var device = CanvasDevice.GetSharedDevice();
        using var renderTarget = new CanvasRenderTarget(device, width, height, 96f);
        using (var ds = renderTarget.CreateDrawingSession())
        {
            ds.Clear(_backgroundColor);
            foreach (var op in _history.Operations)
            {
                op.Render(ds);
            }
        }

        using var stream = await file.OpenAsync(FileAccessMode.ReadWrite);
        await renderTarget.SaveAsync(stream, CanvasBitmapFileFormat.Png);
    }

    private void UpdateUndoRedoButtons()
    {
        UndoButton.IsEnabled = _history.UndoCount > 0;
        RedoButton.IsEnabled = _history.RedoCount > 0;
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _history.Dispose();
            _disposed = true;
        }
    }
}

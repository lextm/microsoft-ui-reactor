using System;
using System.Collections.Generic;
using System.Numerics;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Geometry;
using Windows.UI;

namespace Paint;

/// <summary>
/// Represents a single drawing operation that can be rendered onto a canvas.
/// </summary>
public sealed class DrawingOperation : IDisposable
{
    public DrawingTool Tool { get; init; }
    public Color Color { get; init; }
    public float StrokeWidth { get; init; }
    public List<Vector2>? Points { get; init; }
    public Vector2 StartPoint { get; init; }
    public Vector2 EndPoint { get; init; }

    private CanvasGeometry? _cachedGeometry;
    private bool _disposed;

    public void Render(CanvasDrawingSession ds)
    {
        if (_disposed) return;

        switch (Tool)
        {
            case DrawingTool.Pen:
            case DrawingTool.Eraser:
                RenderFreehand(ds);
                break;
            case DrawingTool.Line:
                ds.DrawLine(StartPoint, EndPoint, Color, StrokeWidth);
                break;
            case DrawingTool.Rectangle:
                RenderRectangle(ds);
                break;
            case DrawingTool.Ellipse:
                RenderEllipse(ds);
                break;
        }
    }

    private void RenderFreehand(CanvasDrawingSession ds)
    {
        if (Points is null || Points.Count < 2) return;

        if (_cachedGeometry is null)
        {
            var device = ds.Device;
            using var builder = new CanvasPathBuilder(device);
            builder.BeginFigure(Points[0]);
            for (int i = 1; i < Points.Count; i++)
            {
                builder.AddLine(Points[i]);
            }
            builder.EndFigure(CanvasFigureLoop.Open);
            _cachedGeometry = CanvasGeometry.CreatePath(builder);
        }

        var style = new CanvasStrokeStyle
        {
            StartCap = CanvasCapStyle.Round,
            EndCap = CanvasCapStyle.Round,
            LineJoin = CanvasLineJoin.Round
        };
        ds.DrawGeometry(_cachedGeometry, Color, StrokeWidth, style);
    }

    private void RenderRectangle(CanvasDrawingSession ds)
    {
        float x = Math.Min(StartPoint.X, EndPoint.X);
        float y = Math.Min(StartPoint.Y, EndPoint.Y);
        float w = Math.Abs(EndPoint.X - StartPoint.X);
        float h = Math.Abs(EndPoint.Y - StartPoint.Y);
        ds.DrawRectangle(x, y, w, h, Color, StrokeWidth);
    }

    private void RenderEllipse(CanvasDrawingSession ds)
    {
        float cx = (StartPoint.X + EndPoint.X) / 2f;
        float cy = (StartPoint.Y + EndPoint.Y) / 2f;
        float rx = Math.Abs(EndPoint.X - StartPoint.X) / 2f;
        float ry = Math.Abs(EndPoint.Y - StartPoint.Y) / 2f;
        ds.DrawEllipse(cx, cy, rx, ry, Color, StrokeWidth);
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _cachedGeometry?.Dispose();
            _cachedGeometry = null;
            _disposed = true;
        }
    }
}

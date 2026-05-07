using System;
using System.Collections.Generic;

namespace Paint;

/// <summary>
/// Manages undo/redo stacks for drawing operations.
/// </summary>
public sealed class UndoRedoStack : IDisposable
{
    private const int MaxLevels = 20;
    private readonly List<DrawingOperation> _undoStack = new();
    private readonly List<DrawingOperation> _redoStack = new();

    public int UndoCount => _undoStack.Count;
    public int RedoCount => _redoStack.Count;
    public IReadOnlyList<DrawingOperation> Operations => _undoStack;

    public void Push(DrawingOperation op)
    {
        _undoStack.Add(op);
        // Clear redo stack on new operation
        foreach (var r in _redoStack) r.Dispose();
        _redoStack.Clear();
        // Enforce max levels
        while (_undoStack.Count > MaxLevels)
        {
            _undoStack[0].Dispose();
            _undoStack.RemoveAt(0);
        }
    }

    public bool Undo()
    {
        if (_undoStack.Count == 0) return false;
        var op = _undoStack[^1];
        _undoStack.RemoveAt(_undoStack.Count - 1);
        _redoStack.Add(op);
        return true;
    }

    public bool Redo()
    {
        if (_redoStack.Count == 0) return false;
        var op = _redoStack[^1];
        _redoStack.RemoveAt(_redoStack.Count - 1);
        _undoStack.Add(op);
        return true;
    }

    public void Clear()
    {
        foreach (var op in _undoStack) op.Dispose();
        foreach (var op in _redoStack) op.Dispose();
        _undoStack.Clear();
        _redoStack.Clear();
    }

    public void Dispose() => Clear();
}

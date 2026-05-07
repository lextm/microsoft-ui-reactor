using System;

namespace ImageGallery.Models;

public sealed class ImageItem
{
    public required string FilePath { get; init; }
    public required string FileName { get; init; }
    public long FileSize { get; init; }
    public DateTime DateModified { get; init; }
    public int Width { get; set; }
    public int Height { get; set; }

    public string FileSizeDisplay =>
        FileSize switch
        {
            < 1024 => $"{FileSize} B",
            < 1024 * 1024 => $"{FileSize / 1024.0:F1} KB",
            _ => $"{FileSize / (1024.0 * 1024.0):F1} MB"
        };

    public string DimensionsDisplay =>
        Width > 0 && Height > 0 ? $"{Width} × {Height}" : "Unknown";
}

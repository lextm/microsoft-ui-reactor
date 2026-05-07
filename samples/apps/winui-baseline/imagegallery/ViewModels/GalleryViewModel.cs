using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using ImageGallery.Models;

namespace ImageGallery.ViewModels;

public sealed class GalleryViewModel : INotifyPropertyChanged
{
    private static readonly string[] SupportedExtensions =
        [".jpg", ".jpeg", ".png", ".bmp", ".gif", ".webp", ".tiff", ".tif"];

    public ObservableCollection<ImageItem> Images { get; } = [];

    private ImageItem? _selectedImage;
    public ImageItem? SelectedImage
    {
        get => _selectedImage;
        set
        {
            if (_selectedImage == value) return;
            _selectedImage = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasSelection));
            OnPropertyChanged(nameof(CanGoPrev));
            OnPropertyChanged(nameof(CanGoNext));
        }
    }

    private string _folderPath = string.Empty;
    public string FolderPath
    {
        get => _folderPath;
        private set { _folderPath = value; OnPropertyChanged(); }
    }

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set { _isLoading = value; OnPropertyChanged(); }
    }

    private bool _isViewerOpen;
    public bool IsViewerOpen
    {
        get => _isViewerOpen;
        set { _isViewerOpen = value; OnPropertyChanged(); }
    }

    private double _zoomFactor = 1.0;
    public double ZoomFactor
    {
        get => _zoomFactor;
        set { _zoomFactor = Math.Clamp(value, 0.1, 10.0); OnPropertyChanged(); }
    }

    private bool _isSlideshowActive;
    public bool IsSlideshowActive
    {
        get => _isSlideshowActive;
        set { _isSlideshowActive = value; OnPropertyChanged(); }
    }

    private int _slideshowInterval = 3;
    public int SlideshowInterval
    {
        get => _slideshowInterval;
        set { _slideshowInterval = Math.Clamp(value, 1, 30); OnPropertyChanged(); }
    }

    public bool HasSelection => _selectedImage != null;
    public bool CanGoPrev => _selectedImage != null && Images.IndexOf(_selectedImage) > 0;
    public bool CanGoNext => _selectedImage != null && Images.IndexOf(_selectedImage) < Images.Count - 1;

    public async Task LoadFolderAsync(string folderPath)
    {
        IsLoading = true;
        Images.Clear();
        FolderPath = folderPath;

        try
        {
            var files = await Task.Run(() =>
                Directory.EnumerateFiles(folderPath)
                    .Where(f => SupportedExtensions.Contains(
                        Path.GetExtension(f).ToLowerInvariant()))
                    .OrderBy(f => f)
                    .Select(f =>
                    {
                        var info = new FileInfo(f);
                        return new ImageItem
                        {
                            FilePath = f,
                            FileName = info.Name,
                            FileSize = info.Length,
                            DateModified = info.LastWriteTime
                        };
                    })
                    .ToList());

            foreach (var item in files)
                Images.Add(item);
        }
        finally
        {
            IsLoading = false;
        }
    }

    public void GoToNext()
    {
        if (_selectedImage == null) return;
        var idx = Images.IndexOf(_selectedImage);
        if (idx < Images.Count - 1)
            SelectedImage = Images[idx + 1];
    }

    public void GoToPrev()
    {
        if (_selectedImage == null) return;
        var idx = Images.IndexOf(_selectedImage);
        if (idx > 0)
            SelectedImage = Images[idx - 1];
    }

    public void ZoomIn() => ZoomFactor *= 1.25;
    public void ZoomOut() => ZoomFactor /= 1.25;
    public void ZoomFit() => ZoomFactor = 1.0;

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

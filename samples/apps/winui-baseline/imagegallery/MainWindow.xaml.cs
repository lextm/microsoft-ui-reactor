using System;
using ImageGallery.Models;
using ImageGallery.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Storage.Pickers;

namespace ImageGallery;

public sealed partial class MainWindow : Window
{
    private readonly GalleryViewModel _vm = new();
    private DispatcherTimer? _slideshowTimer;

    public MainWindow()
    {
        InitializeComponent();
        Title = "Image Gallery";
        ImageGridView.ItemsSource = _vm.Images;

        // Set up ItemsPanel for grid layout
        var itemsPanelTemplate = (ItemsPanelTemplate)Microsoft.UI.Xaml.Markup.XamlReader.Load(
            @"<ItemsPanelTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'>
                <ItemsWrapGrid Orientation='Horizontal' ItemWidth='180' ItemHeight='180' MaximumRowsOrColumns='20'/>
              </ItemsPanelTemplate>");
        ImageGridView.ItemsPanel = itemsPanelTemplate;

        // Use ContainerContentChanging for virtualized thumbnail loading
        ImageGridView.ContainerContentChanging += ImageGridView_ContainerContentChanging;
    }

    private async void OpenFolder_Click(object sender, RoutedEventArgs e)
    {
        var picker = new FolderPicker();
        picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
        picker.FileTypeFilter.Add("*");

        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

        var folder = await picker.PickSingleFolderAsync();
        if (folder == null) return;

        EmptyState.Visibility = Visibility.Collapsed;
        LoadingRing.IsActive = true;

        await _vm.LoadFolderAsync(folder.Path);

        LoadingRing.IsActive = false;
        StatusText.Text = $"{_vm.Images.Count} images in {folder.Path}";

        if (_vm.Images.Count == 0)
            EmptyState.Visibility = Visibility.Visible;
    }

    private void ImageGridView_ItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is ImageItem item)
        {
            _vm.SelectedImage = item;
            ShowViewer(item);
        }
    }

    private void ShowViewer(ImageItem item)
    {
        _vm.IsViewerOpen = true;
        ViewerOverlay.Visibility = Visibility.Visible;
        LoadFullImage(item);
        UpdateInfoBar(item);
        UpdateNavButtons();
    }

    private void LoadFullImage(ImageItem item)
    {
        var bitmap = new BitmapImage();
        bitmap.UriSource = new Uri(item.FilePath);
        bitmap.ImageOpened += (s, _) =>
        {
            item.Width = bitmap.PixelWidth;
            item.Height = bitmap.PixelHeight;
            UpdateInfoBar(item);
        };
        FullImage.Source = bitmap;
        ImageScrollViewer.ChangeView(null, null, 1.0f);
    }

    private void UpdateInfoBar(ImageItem item)
    {
        InfoFileName.Text = item.FileName;
        InfoDimensions.Text = item.DimensionsDisplay;
        InfoFileSize.Text = item.FileSizeDisplay;
        InfoDate.Text = item.DateModified.ToString("yyyy-MM-dd HH:mm");
        DispatcherQueue.TryEnqueue(() =>
        {
            InfoZoom.Text = $"{ImageScrollViewer.ZoomFactor * 100:F0}%";
        });
    }

    private void UpdateNavButtons()
    {
        PrevButton.IsEnabled = _vm.CanGoPrev;
        NextButton.IsEnabled = _vm.CanGoNext;
    }

    private void BackToGallery_Click(object sender, RoutedEventArgs e)
    {
        _vm.IsViewerOpen = false;
        ViewerOverlay.Visibility = Visibility.Collapsed;
        StopSlideshow();
    }

    private void PrevImage_Click(object sender, RoutedEventArgs e)
    {
        _vm.GoToPrev();
        if (_vm.SelectedImage != null)
        {
            LoadFullImage(_vm.SelectedImage);
            UpdateInfoBar(_vm.SelectedImage);
            UpdateNavButtons();
        }
    }

    private void NextImage_Click(object sender, RoutedEventArgs e)
    {
        _vm.GoToNext();
        if (_vm.SelectedImage != null)
        {
            LoadFullImage(_vm.SelectedImage);
            UpdateInfoBar(_vm.SelectedImage);
            UpdateNavButtons();
        }
    }

    private void ZoomIn_Click(object sender, RoutedEventArgs e)
    {
        ImageScrollViewer.ChangeView(null, null, ImageScrollViewer.ZoomFactor * 1.25f);
        UpdateZoomDisplay();
    }

    private void ZoomOut_Click(object sender, RoutedEventArgs e)
    {
        ImageScrollViewer.ChangeView(null, null, ImageScrollViewer.ZoomFactor / 1.25f);
        UpdateZoomDisplay();
    }

    private void ZoomFit_Click(object sender, RoutedEventArgs e)
    {
        ImageScrollViewer.ChangeView(null, null, 1.0f);
        UpdateZoomDisplay();
    }

    private void UpdateZoomDisplay()
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            InfoZoom.Text = $"{ImageScrollViewer.ZoomFactor * 100:F0}%";
        });
    }

    private void Slideshow_Click(object sender, RoutedEventArgs e)
    {
        if (_vm.Images.Count == 0) return;

        if (_vm.SelectedImage == null)
            _vm.SelectedImage = _vm.Images[0];

        ShowViewer(_vm.SelectedImage);
        StartSlideshow();
    }

    private void StopSlideshow_Click(object sender, RoutedEventArgs e)
    {
        StopSlideshow();
    }

    private void StartSlideshow()
    {
        _vm.IsSlideshowActive = true;
        SlideshowButton.Visibility = Visibility.Collapsed;
        StopSlideshowButton.Visibility = Visibility.Visible;

        var interval = (int)IntervalBox.Value;
        _slideshowTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(interval)
        };
        _slideshowTimer.Tick += SlideshowTimer_Tick;
        _slideshowTimer.Start();
    }

    private void StopSlideshow()
    {
        _vm.IsSlideshowActive = false;
        SlideshowButton.Visibility = Visibility.Visible;
        StopSlideshowButton.Visibility = Visibility.Collapsed;

        if (_slideshowTimer != null)
        {
            _slideshowTimer.Stop();
            _slideshowTimer.Tick -= SlideshowTimer_Tick;
            _slideshowTimer = null;
        }
    }

    private void SlideshowTimer_Tick(object? sender, object e)
    {
        if (_vm.CanGoNext)
            _vm.GoToNext();
        else
            _vm.SelectedImage = _vm.Images[0];

        if (_vm.SelectedImage != null)
        {
            LoadFullImage(_vm.SelectedImage);
            UpdateInfoBar(_vm.SelectedImage);
            UpdateNavButtons();
        }
    }

    private void ImageGridView_ContainerContentChanging(
        ListViewBase sender,
        ContainerContentChangingEventArgs args)
    {
        if (args.InRecycleQueue)
        {
            // Clear image when recycled to free memory
            var templateRoot = args.ItemContainer.ContentTemplateRoot as Grid;
            if (templateRoot?.Children[0] is Image img)
                img.Source = null;
            return;
        }

        if (args.Phase == 0)
        {
            // Phase 0: set up container structure if needed
            if (args.ItemContainer.ContentTemplateRoot == null)
            {
                var grid = new Grid
                {
                    Width = 160,
                    Height = 160,
                    Margin = new Thickness(4),
                    CornerRadius = new CornerRadius(4)
                };
                grid.Background = (Brush)Application.Current.Resources["CardBackgroundFillColorDefaultBrush"];

                var image = new Image { Stretch = Stretch.UniformToFill };
                grid.Children.Add(image);

                var label = new TextBlock
                {
                    VerticalAlignment = VerticalAlignment.Bottom,
                    Padding = new Thickness(4),
                    MaxLines = 1,
                    TextTrimming = TextTrimming.CharacterEllipsis,
                    FontSize = 11
                };
                grid.Children.Add(label);

                args.ItemContainer.Content = grid;
            }

            args.RegisterUpdateCallback(1, ImageGridView_ContainerContentChanging);
        }
        else if (args.Phase == 1)
        {
            // Phase 1: load thumbnail asynchronously
            if (args.Item is ImageItem item &&
                args.ItemContainer.Content is Grid container)
            {
                if (container.Children[0] is Image img)
                {
                    var bitmap = new BitmapImage { DecodePixelWidth = 160 };
                    bitmap.UriSource = new Uri(item.FilePath);
                    img.Source = bitmap;
                }
                if (container.Children[1] is TextBlock tb)
                {
                    tb.Text = item.FileName;
                }
            }
        }
    }
}

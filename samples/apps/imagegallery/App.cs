using Microsoft.UI.Reactor;
using Microsoft.UI.Reactor.Core;
using Microsoft.UI.Reactor.Layout;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using static Microsoft.UI.Reactor.Factories;
using IOPath = System.IO.Path;

namespace ImageGallery;

// ── Data model ──────────────────────────────────────────────────────────

record ImageInfo(string FilePath, string FileName, long FileSize);

// ── Root app component ──────────────────────────────────────────────────

sealed class App : Component
{
    static readonly string[] ImageExtensions =
        [".jpg", ".jpeg", ".png", ".bmp", ".gif", ".webp"];

    public override Element Render()
    {
        var (folderPath, setFolderPath) = UseState<string?>(null);
        var (images, setImages) = UseState<ImageInfo[]>([]);
        var (selectedIndex, setSelectedIndex) = UseState(-1);
        var (lightboxOpen, setLightboxOpen) = UseState(false);

        // Scan folder when path changes
        UseEffect(() =>
        {
            if (folderPath is null) { setImages([]); return; }

            try
            {
                var files = Directory.EnumerateFiles(folderPath)
                    .Where(f => ImageExtensions.Contains(
                        IOPath.GetExtension(f).ToLowerInvariant()))
                    .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
                    .Select(f => new ImageInfo(f, IOPath.GetFileName(f),
                        new FileInfo(f).Length))
                    .ToArray();
                setImages(files);
            }
            catch
            {
                setImages([]);
            }
        }, folderPath ?? "");

        var imageCount = UseMemo(() => images.Length, images);

        async void PickFolder()
        {
            try
            {
                var picker = new Windows.Storage.Pickers.FolderPicker();
                var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(
                    ReactorApp.ActiveHost!.Window);
                WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
                picker.FileTypeFilter.Add("*");
                var folder = await picker.PickSingleFolderAsync();
                if (folder is not null)
                    setFolderPath(folder.Path);
            }
            catch
            {
                // User cancelled or error
            }
        }

        void OpenLightbox(int index)
        {
            setSelectedIndex(index);
            setLightboxOpen(true);
        }

        void CloseLightbox() => setLightboxOpen(false);

        void NavigatePrev()
        {
            if (selectedIndex > 0) setSelectedIndex(selectedIndex - 1);
        }

        void NavigateNext()
        {
            if (selectedIndex < images.Length - 1)
                setSelectedIndex(selectedIndex + 1);
        }

        // ── Build UI ────────────────────────────────────────────────

        // Toolbar
        var toolbar = (FlexRow(
            Button("\U0001f4c1 Select Folder", PickFolder),
            TextBlock(folderPath ?? "No folder selected")
                .Opacity(folderPath is null ? 0.5 : 1.0)
                .VAlign(VerticalAlignment.Center)
                .Flex(grow: 1, shrink: 1),
            TextBlock($"{imageCount} image{(imageCount == 1 ? "" : "s")}")
                .VAlign(VerticalAlignment.Center)
                .Opacity(0.7)
        ) with { AlignItems = FlexAlign.Center, ColumnGap = 12 })
            .Padding(12, 8, 12, 8)
            .Background(Theme.CardBackground);

        // Main content
        Element mainContent;
        if (folderPath is null)
        {
            mainContent = FlexColumn(
                TextBlock("\U0001f4f7").FontSize(48)
                    .HAlign(HorizontalAlignment.Center),
                TextBlock("Select a folder to browse images").FontSize(16)
                    .Opacity(0.6).HAlign(HorizontalAlignment.Center),
                Button("\U0001f4c1 Select Folder", PickFolder)
                    .HAlign(HorizontalAlignment.Center)
            ) with { AlignItems = FlexAlign.Center,
                     JustifyContent = FlexJustify.Center, RowGap = 12 };
        }
        else if (imageCount == 0)
        {
            mainContent = FlexColumn(
                TextBlock("\U0001f5bc\ufe0f").FontSize(48)
                    .HAlign(HorizontalAlignment.Center),
                TextBlock("No images found").FontSize(18)
                    .HAlign(HorizontalAlignment.Center),
                TextBlock("Supported: .jpg, .jpeg, .png, .bmp, .gif, .webp")
                    .FontSize(12).Opacity(0.5)
                    .HAlign(HorizontalAlignment.Center)
            ) with { AlignItems = FlexAlign.Center,
                     JustifyContent = FlexJustify.Center, RowGap = 8 };
        }
        else
        {
            var thumbnails = images.Select((img, i) =>
                ThumbnailCard(img, i, () => OpenLightbox(i)));

            mainContent = ScrollView(
                (FlexRow([.. thumbnails]) with
                {
                    Wrap = FlexWrap.Wrap,
                    ColumnGap = 8,
                    RowGap = 8,
                    AlignItems = FlexAlign.FlexStart,
                }).Padding(12)
            );
        }

        // Status bar
        var statusBar = (FlexRow(
            TextBlock(folderPath is not null
                ? $"\U0001f4c2 {folderPath}"
                : "Ready")
                .FontSize(11).Opacity(0.6)
                .Flex(grow: 1, shrink: 1)
        ) with { AlignItems = FlexAlign.Center })
            .Padding(8, 4, 8, 4)
            .Background(Theme.CardBackground);

        // Root layout
        var page = FlexColumn(
            toolbar,
            mainContent.Flex(grow: 1, basis: 0),
            statusBar
        );

        // Lightbox overlay (layered on top via Grid)
        if (lightboxOpen && selectedIndex >= 0
            && selectedIndex < images.Length)
        {
            var lightbox = LightboxOverlay(
                images, selectedIndex,
                NavigatePrev, NavigateNext, CloseLightbox);

            return Grid(
                [GridSize.Star()], [GridSize.Star()],
                page, lightbox
            );
        }

        return page;
    }

    // ── Thumbnail card ──────────────────────────────────────────────

    static Element ThumbnailCard(ImageInfo img, int index, Action onClick)
    {
        return Border(
            FlexColumn(
                Image(img.FilePath)
                    .Set(i =>
                    {
                        i.Stretch =
                            Microsoft.UI.Xaml.Media.Stretch.UniformToFill;
                    })
                    .Width(150).Height(130),
                TextBlock(img.FileName).FontSize(11)
                    .Set(tb =>
                    {
                        tb.TextTrimming =
                            Microsoft.UI.Xaml.TextTrimming.CharacterEllipsis;
                        tb.MaxLines = 1;
                    })
                    .MaxWidth(150)
                    .Padding(4, 2, 4, 4)
            ) with { AlignItems = FlexAlign.Stretch }
        )
        .Set(b =>
        {
            b.CornerRadius = new CornerRadius(6);
            b.BorderBrush =
                new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Microsoft.UI.Colors.Transparent);
            b.BorderThickness = new Thickness(1);
        })
        .Background(Theme.CardBackground)
        .OnPointerPressed((_, _) => onClick())
        .WithKey($"thumb-{index}");
    }

    // ── Lightbox overlay ────────────────────────────────────────────

    static Element LightboxOverlay(
        ImageInfo[] images,
        int selectedIndex,
        Action onPrev,
        Action onNext,
        Action onClose)
    {
        var img = images[selectedIndex];
        var hasPrev = selectedIndex > 0;
        var hasNext = selectedIndex < images.Length - 1;

        var sizeText = img.FileSize switch
        {
            < 1024 => $"{img.FileSize} B",
            < 1024 * 1024 => $"{img.FileSize / 1024.0:F1} KB",
            _ => $"{img.FileSize / (1024.0 * 1024.0):F1} MB"
        };

        var prevButton = Button("\u25c0", onPrev)
            .Opacity(hasPrev ? 1.0 : 0.3)
            .VAlign(VerticalAlignment.Center)
            .Margin(8);

        var nextButton = Button("\u25b6", onNext)
            .Opacity(hasNext ? 1.0 : 0.3)
            .VAlign(VerticalAlignment.Center)
            .Margin(8);

        var infoBar = (FlexRow(
            TextBlock(img.FileName).FontSize(13)
                .Foreground(Theme.SecondaryText)
                .Flex(grow: 1, shrink: 1),
            TextBlock(sizeText).FontSize(12)
                .Opacity(0.7)
                .Foreground(Theme.SecondaryText),
            TextBlock($"{selectedIndex + 1} / {images.Length}")
                .FontSize(12).Opacity(0.7)
                .Foreground(Theme.SecondaryText)
        ) with { AlignItems = FlexAlign.Center, ColumnGap = 16 })
            .Padding(16, 8, 16, 8);

        return Border(
            FlexColumn(
                // Close button row
                (FlexRow(
                    TextBlock($"  {img.FileName}").FontSize(14)
                        .Foreground(Theme.SecondaryText)
                        .Flex(grow: 1),
                    Button("\u2715", onClose)
                ) with { AlignItems = FlexAlign.Center })
                    .Padding(8, 4, 8, 4),

                // Image row with prev/next
                (FlexRow(
                    prevButton,
                    Image(img.FilePath)
                        .Set(i =>
                        {
                            i.Stretch =
                                Microsoft.UI.Xaml.Media.Stretch.Uniform;
                        })
                        .Flex(grow: 1, basis: 0)
                        .MaxWidth(1200).MaxHeight(800),
                    nextButton
                ) with { AlignItems = FlexAlign.Center,
                         JustifyContent = FlexJustify.Center })
                    .Flex(grow: 1, basis: 0),

                infoBar
            ) with { AlignItems = FlexAlign.Stretch }
        )
        .Set(b =>
        {
            b.Background =
                new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Microsoft.UI.ColorHelper.FromArgb(220, 0, 0, 0));
        })
        .OnPointerPressed((_, _) => onClose())
        .OnKeyDown((_, e) =>
        {
            switch (e.Key)
            {
                case Windows.System.VirtualKey.Escape:
                    onClose();
                    e.Handled = true;
                    break;
                case Windows.System.VirtualKey.Left:
                    onPrev();
                    e.Handled = true;
                    break;
                case Windows.System.VirtualKey.Right:
                    onNext();
                    e.Handled = true;
                    break;
            }
        })
        .Set(b => b.IsTabStop = true);
    }
}

// ── Entry point ─────────────────────────────────────────────────────────

class Program
{
    [STAThread]
    static void Main() =>
        ReactorApp.Run<App>("Image Gallery", 1100, 750);
}

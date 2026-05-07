# Image Gallery

A photo gallery browser built with Reactor that loads images from a local
folder, displays them in a responsive thumbnail grid, and supports a
lightbox detail view with keyboard navigation.

## What It Does
- Select a folder via native FolderPicker dialog
- Scan for image files (jpg, jpeg, png, bmp, gif, webp)
- Display thumbnails in a responsive wrapping grid
- Click thumbnail → lightbox overlay with full-size image
- Previous/Next navigation in lightbox (buttons + arrow keys)
- Escape to close lightbox
- Image count in toolbar, empty folder handling

## Reactor Features Exercised
| Feature | Usage |
|---|---|
| `UseState` | Folder path, image list, selected index, lightbox visibility |
| `UseEffect` | Scan folder when path changes |
| `UseMemo` | Compute image count |
| `UseCallback` | Keyboard event handlers |
| Conditional rendering | Lightbox visible/hidden, empty states |
| Component composition | ThumbnailCard, LightboxOverlay helpers |
| `.Set()` escape hatch | BitmapImage source, Image.Stretch |
| `.OnKeyDown` | Keyboard navigation in lightbox |
| FlexRow + Wrap | Responsive thumbnail grid |
| Theme tokens | Card backgrounds, overlay colors |

## Build & Run
```
dotnet build samples/apps/imagegallery/ImageGallery.csproj
dotnet run --project samples/apps/imagegallery/ImageGallery.csproj
```

## Build Metrics

| Metric | Value |
|---|---|
| **Agent model** | `claude-opus-4.6` |
| **Agent session** | Fresh (isolated sub-agent, no shared context) |
| **Input tokens** | 2,501,885 |
| **Output tokens** | 20,236 |
| **Total tokens** | 2,522,121 |
| **Peak context window** | 102,033 tokens |
| **Turns to completion** | 30 |
| **Wall-clock time** | 9 min 31 sec |
| **First-compile success** | No |
| **Compile errors fixed** | 14 |
| **Build → fix cycles** | 3 |
| **First-run success** | _(manual)_ |
| **Runtime errors** | _(manual)_ |
| **Human interventions** | 0 |
| **Feature completeness** | 100% — all planned features delivered |
| **Lines of code** | 280 |
| **Source files** | 1 (`App.cs`) |

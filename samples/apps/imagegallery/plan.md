# Image Gallery Build Plan

## Goal
Build a photo gallery browser that loads images from a local folder,
displays them in a responsive thumbnail grid, and supports a lightbox detail view.

## Scope
- Folder picker to select image source directory
- Scan for image files (jpg, jpeg, png, bmp, gif, webp)
- Responsive wrapping grid of thumbnail cards
- Lightbox overlay with prev/next navigation
- Keyboard navigation (arrows, escape)
- Empty state handling
- Image count in status bar

## Architecture

### Entry shell
- `App.cs` hosts the entire sample in a single file.
- `ImageGalleryApp` manages folder path, image list, and lightbox state.

### Image scanning
- `UseEffect` triggers folder scan when selected path changes.
- Scans for common image extensions using `Directory.GetFiles`.
- `UseMemo` computes filtered/sorted image list.

### Thumbnail grid
- FlexRow with Wrap for responsive wrapping layout.
- Each thumbnail is a 150×150 card with filename label.
- BitmapImage loaded from file URI.
- Click opens lightbox at that index.

### Lightbox overlay
- Grid-layered dark semi-transparent background.
- Centered large image with prev/next buttons.
- Close button and click-outside-to-close.
- Keyboard: Left/Right arrows, Escape to close.

### Folder picker
- Windows.Storage.Pickers.FolderPicker with HWND initialization.
- Async void handler with try/catch.

## Verification
1. Build `ImageGallery.csproj` in Debug.
2. Fix all compiler errors until 0 errors, 0 warnings.

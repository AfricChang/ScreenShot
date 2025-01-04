using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Avalonia.Interactivity;
using ScreenShot.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ScreenShot.Controls;

public class ScreenshotCanvas : Canvas
{
    private Point startPoint;
    private IDrawingElement? currentElement;
    private readonly List<IDrawingElement> drawingElements = new();
    private readonly Image drawingImage;

    public Bitmap? OriginalImage { get; set; }

    public ScreenshotCanvas()
    {
        DoubleTapped += OnDoubleTap;
        ClipToBounds = true;

        drawingImage = new Image
        {
            Stretch = Stretch.None,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
        };
        Children.Add(drawingImage);
    }

    public static readonly DirectProperty<ScreenshotCanvas, Color> DrawingColorProperty =
        AvaloniaProperty.RegisterDirect<ScreenshotCanvas, Color>(
            nameof(DrawingColor),
            o => o.DrawingColor,
            (o, v) => o.DrawingColor = v);

    private Color drawingColor = Colors.Red;
    public Color DrawingColor
    {
        get => drawingColor;
        set => SetAndRaise(DrawingColorProperty, ref drawingColor, value);
    }

    public static readonly DirectProperty<ScreenshotCanvas, DrawingTool> CurrentToolProperty =
        AvaloniaProperty.RegisterDirect<ScreenshotCanvas, DrawingTool>(
            nameof(CurrentTool),
            o => o.CurrentTool,
            (o, v) => o.CurrentTool = v);

    private DrawingTool currentTool = DrawingTool.None;
    public DrawingTool CurrentTool
    {
        get => currentTool;
        set => SetAndRaise(CurrentToolProperty, ref currentTool, value);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        startPoint = e.GetPosition(this);

        switch (CurrentTool)
        {
            case DrawingTool.Text:
                var textElement = new TextElement
                {
                    Position = startPoint,
                    Color = DrawingColor,
                    Text = "双击编辑文字"
                };
                currentElement = textElement;
                drawingElements.Add(textElement);
                break;

            case DrawingTool.Arrow:
                var arrowElement = new ArrowElement
                {
                    Start = startPoint,
                    End = startPoint,
                    Color = DrawingColor
                };
                currentElement = arrowElement;
                drawingElements.Add(arrowElement);
                break;

            case DrawingTool.Rectangle:
                var rectangleElement = new RectangleElement
                {
                    Bounds = new Rect(startPoint, startPoint),
                    Color = DrawingColor,
                    StrokeThickness = 2
                };
                currentElement = rectangleElement;
                drawingElements.Add(rectangleElement);
                break;
        }

        RedrawCanvas();
    }

    public async Task SaveToFileAsync(IStorageFile file)
    {
        if (OriginalImage == null) return;

        using var stream = await file.OpenWriteAsync();
        var pixelSize = OriginalImage.PixelSize;
        var dpi = OriginalImage.Dpi;

        using var renderBitmap = new RenderTargetBitmap(pixelSize, dpi);
        using (var context = renderBitmap.CreateDrawingContext())
        {
            var rect = new Rect(0, 0, pixelSize.Width, pixelSize.Height);
            context.DrawImage(OriginalImage, rect, rect);

            foreach (var element in drawingElements)
            {
                element.Draw(context);
            }
        }

        renderBitmap.Save(stream);
    }

    public async Task CopyToClipboardAsync()
    {
        if (OriginalImage == null) return;

        var pixelSize = OriginalImage.PixelSize;
        var dpi = OriginalImage.Dpi;

        using var renderBitmap = new RenderTargetBitmap(pixelSize, dpi);
        using (var context = renderBitmap.CreateDrawingContext())
        {
            var rect = new Rect(0, 0, pixelSize.Width, pixelSize.Height);
            context.DrawImage(OriginalImage, rect, rect);

            foreach (var element in drawingElements)
            {
                element.Draw(context);
            }
        }

        var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
        if (clipboard != null)
        {
            var dataObject = new DataObject();
            dataObject.Set("PNG", renderBitmap);
            await clipboard.SetDataObjectAsync(dataObject);
        }
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        if (currentElement != null)
        {
            var currentPoint = e.GetPosition(this);
            UpdateDrawing(currentPoint);
            RedrawCanvas();
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        currentElement = null;
    }

    private void UpdateDrawing(Point currentPoint)
    {
        switch (currentElement)
        {
            case ArrowElement arrow:
                arrow.End = currentPoint;
                break;
            case RectangleElement rectangle:
                rectangle.Bounds = new Rect(
                    Math.Min(startPoint.X, currentPoint.X),
                    Math.Min(startPoint.Y, currentPoint.Y),
                    Math.Abs(currentPoint.X - startPoint.X),
                    Math.Abs(currentPoint.Y - startPoint.Y));
                break;
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == BoundsProperty)
        {
            RedrawCanvas();
        }
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        RedrawCanvas();
    }

    private void RedrawCanvas()
    {
        if (OriginalImage == null) return;

        var pixelSize = OriginalImage.PixelSize;
        var dpi = OriginalImage.Dpi;

        using var renderBitmap = new RenderTargetBitmap(pixelSize, dpi);
        using (var context = renderBitmap.CreateDrawingContext())
        {
            var rect = new Rect(0, 0, pixelSize.Width, pixelSize.Height);
            context.DrawImage(OriginalImage, rect, rect);

            foreach (var element in drawingElements)
            {
                element.Draw(context);
            }
        }

        drawingImage.Source = renderBitmap;
    }

    protected void OnDoubleTap(object? sender, TappedEventArgs e)
    {
        if (CurrentTool != DrawingTool.Text) return;

        var position = e.GetPosition(this);
        var textElement = drawingElements.Find(el => el is TextElement text &&
            text.Contains(position)) as TextElement;

        if (textElement != null)
        {
            // TODO: 弹出文本编辑对话框
        }
    }

    public void ClearDrawings()
    {
        drawingElements.Clear();
        RedrawCanvas();
    }

    public void SetImage(Bitmap image)
    {
        OriginalImage = image;
        Width = image.PixelSize.Width;
        Height = image.PixelSize.Height;
        RedrawCanvas();
    }
}

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using ScreenShot.Models;
using ScreenShot.Platform;
using System;
using System.Collections.Generic;

namespace ScreenShot.Controls;

public class ScreenshotCanvas : Canvas
{
    private Point startPoint;
    private IDrawingElement? currentElement;
    private readonly List<IDrawingElement> drawingElements = new();
    private readonly Image drawingImage;

    public ScreenshotCanvas()
    {
        DoubleTapped += OnDoubleTap;
        ClipToBounds = true;

        drawingImage = new Image();
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

    public static readonly DirectProperty<ScreenshotCanvas, string> CurrentToolProperty =
        AvaloniaProperty.RegisterDirect<ScreenshotCanvas, string>(
            nameof(CurrentTool),
            o => o.CurrentTool,
            (o, v) => o.CurrentTool = v);

    private string currentTool = "None";
    public string CurrentTool
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
            case "文字":
                var textElement = new TextElement
                {
                    Position = startPoint,
                    Color = DrawingColor,
                    Text = "双击编辑文字"
                };
                currentElement = textElement;
                drawingElements.Add(textElement);
                break;

            case "箭头":
                var arrowElement = new ArrowElement
                {
                    Start = startPoint,
                    End = startPoint,
                    Color = DrawingColor
                };
                currentElement = arrowElement;
                drawingElements.Add(arrowElement);
                break;

            case "圆形":
                var ellipseElement = new EllipseElement
                {
                    Center = startPoint,
                    RadiusX = 0,
                    RadiusY = 0,
                    Color = DrawingColor
                };
                currentElement = ellipseElement;
                drawingElements.Add(ellipseElement);
                break;

            case "矩形":
                var rectangleElement = new RectangleElement
                {
                    Bounds = new Rect(startPoint, startPoint),
                    Color = DrawingColor
                };
                currentElement = rectangleElement;
                drawingElements.Add(rectangleElement);
                break;
        }

        UpdateDrawing();
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        if (currentElement == null) return;

        var currentPoint = e.GetPosition(this);

        switch (currentElement)
        {
            case ArrowElement arrow:
                arrow.End = currentPoint;
                break;

            case EllipseElement ellipse:
                var rx = Math.Abs(currentPoint.X - startPoint.X) / 2;
                var ry = Math.Abs(currentPoint.Y - startPoint.Y) / 2;
                ellipse.Center = new Point(
                    startPoint.X + (currentPoint.X - startPoint.X) / 2,
                    startPoint.Y + (currentPoint.Y - startPoint.Y) / 2);
                ellipse.RadiusX = rx;
                ellipse.RadiusY = ry;
                break;

            case RectangleElement rectangle:
                var x = Math.Min(startPoint.X, currentPoint.X);
                var y = Math.Min(startPoint.Y, currentPoint.Y);
                var width = Math.Abs(currentPoint.X - startPoint.X);
                var height = Math.Abs(currentPoint.Y - startPoint.Y);
                rectangle.Bounds = new Rect(x, y, width, height);
                break;
        }

        UpdateDrawing();
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        currentElement = null;
    }

    private void UpdateDrawing()
    {
        var renderTarget = new RenderTargetBitmap(new PixelSize((int)Width, (int)Height));
        using (var context = renderTarget.CreateDrawingContext(true))
        {
            foreach (var element in drawingElements)
            {
                element.Draw(context);
            }
        }
        drawingImage.Source = renderTarget;
    }

    protected void OnDoubleTap(object? sender, TappedEventArgs e)
    {
        if (CurrentTool != "文字") return;

        var position = e.GetPosition(this);
        var textElement = drawingElements.Find(el => el is TextElement text && text.Contains(position)) as TextElement;

        if (textElement != null)
        {
            // 在这里实现文本编辑逻辑
            // 可以弹出一个对话框让用户输入文字
        }
    }

    public void ClearDrawings()
    {
        drawingElements.Clear();
        UpdateDrawing();
    }
}

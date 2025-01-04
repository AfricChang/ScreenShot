using Avalonia;
using Avalonia.Media;
using System;
using System.Globalization;

namespace ScreenShot.Models;

public interface IDrawingElement
{
    void Draw(DrawingContext context);
    bool Contains(Point point);
}

public class TextElement : IDrawingElement
{
    public Point Position { get; set; }
    public string Text { get; set; } = string.Empty;
    public Color Color { get; set; }
    public double FontSize { get; set; } = 14;

    public void Draw(DrawingContext context)
    {
        var brush = new SolidColorBrush(Color);
        var formattedText = new FormattedText(
            Text,
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            new Typeface("Arial"),
            FontSize,
            brush);

        context.DrawText(formattedText, Position);
    }

    public bool Contains(Point point)
    {
        // 简单的碰撞检测
        var bounds = new Rect(Position, new Size(FontSize * Text.Length, FontSize));
        return bounds.Contains(point);
    }
}

public class ArrowElement : IDrawingElement
{
    public Point Start { get; set; }
    public Point End { get; set; }
    public Color Color { get; set; }
    public double StrokeThickness { get; set; } = 2;

    public void Draw(DrawingContext context)
    {
        var pen = new Pen(new SolidColorBrush(Color), StrokeThickness);
        context.DrawLine(pen, Start, End);

        // 绘制箭头
        var angle = Math.Atan2(End.Y - Start.Y, End.X - Start.X);
        var arrowSize = 10.0;
        
        var arrowPoint1 = new Point(
            End.X - arrowSize * Math.Cos(angle - Math.PI / 6),
            End.Y - arrowSize * Math.Sin(angle - Math.PI / 6));
        
        var arrowPoint2 = new Point(
            End.X - arrowSize * Math.Cos(angle + Math.PI / 6),
            End.Y - arrowSize * Math.Sin(angle + Math.PI / 6));

        context.DrawLine(pen, End, arrowPoint1);
        context.DrawLine(pen, End, arrowPoint2);
    }

    public bool Contains(Point point)
    {
        var distance = DistanceToLine(point, Start, End);
        return distance <= StrokeThickness * 2;
    }

    private double DistanceToLine(Point point, Point lineStart, Point lineEnd)
    {
        var numerator = Math.Abs((lineEnd.Y - lineStart.Y) * point.X -
                                (lineEnd.X - lineStart.X) * point.Y +
                                lineEnd.X * lineStart.Y -
                                lineEnd.Y * lineStart.X);
        
        var denominator = Math.Sqrt(Math.Pow(lineEnd.Y - lineStart.Y, 2) +
                                  Math.Pow(lineEnd.X - lineStart.X, 2));
        
        return numerator / denominator;
    }
}

public class EllipseElement : IDrawingElement
{
    public Point Center { get; set; }
    public double RadiusX { get; set; }
    public double RadiusY { get; set; }
    public Color Color { get; set; }
    public double StrokeThickness { get; set; } = 2;

    public void Draw(DrawingContext context)
    {
        var pen = new Pen(new SolidColorBrush(Color), StrokeThickness);
        context.DrawEllipse(null, pen, Center, RadiusX, RadiusY);
    }

    public bool Contains(Point point)
    {
        var normalizedX = (point.X - Center.X) / RadiusX;
        var normalizedY = (point.Y - Center.Y) / RadiusY;
        var distance = Math.Sqrt(normalizedX * normalizedX + normalizedY * normalizedY);
        return Math.Abs(distance - 1) <= StrokeThickness / Math.Min(RadiusX, RadiusY);
    }
}

public class RectangleElement : IDrawingElement
{
    public Rect Bounds { get; set; }
    public Color Color { get; set; }
    public double StrokeThickness { get; set; } = 2;

    public void Draw(DrawingContext context)
    {
        var pen = new Pen(new SolidColorBrush(Color), StrokeThickness);
        context.DrawRectangle(null, pen, Bounds);
    }

    public bool Contains(Point point)
    {
        var expandedBounds = Bounds.Inflate(StrokeThickness);
        return expandedBounds.Contains(point) && !Bounds.Inflate(-StrokeThickness).Contains(point);
    }
}

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using System;
using System.Collections.Generic;

namespace ScreenShot.Views;

public partial class CaptureWindow : Window
{
    private Point startPoint;
    private Point currentPoint;
    private bool isDrawing;

    public Rect SelectedBounds { get; private set; }

    public CaptureWindow()
    {
        InitializeComponent();
        Topmost = true;
        WindowState = WindowState.FullScreen;
        SystemDecorations = SystemDecorations.None;
        TransparencyLevelHint = new[] { WindowTransparencyLevel.Transparent };
        Background = Brushes.Transparent;
        
        PointerPressed += OnPointerPressed;
        PointerMoved += OnPointerMoved;
        PointerReleased += OnPointerReleased;
        KeyDown += CaptureWindow_KeyDown;
    }

    private void CaptureWindow_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            Close(false);
        }
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        startPoint = e.GetPosition(this);
        currentPoint = startPoint;
        isDrawing = true;
        InvalidateVisual();
    }

    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (!isDrawing) return;

        currentPoint = e.GetPosition(this);
        UpdateSelectedBounds();
        InvalidateVisual();
    }

    private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (!isDrawing) return;

        isDrawing = false;
        UpdateSelectedBounds();
        Close(true);
    }

    private void UpdateSelectedBounds()
    {
        double x = Math.Min(startPoint.X, currentPoint.X);
        double y = Math.Min(startPoint.Y, currentPoint.Y);
        double width = Math.Abs(currentPoint.X - startPoint.X);
        double height = Math.Abs(currentPoint.Y - startPoint.Y);

        SelectedBounds = new Rect(x, y, width, height);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        if (!isDrawing) return;

        // 绘制选择框
        var rect = new Rect(
            Math.Min(startPoint.X, currentPoint.X),
            Math.Min(startPoint.Y, currentPoint.Y),
            Math.Abs(currentPoint.X - startPoint.X),
            Math.Abs(currentPoint.Y - startPoint.Y));

        // 绘制半透明的遮罩
        context.FillRectangle(new SolidColorBrush(Color.FromArgb(128, 0, 0, 0)), new Rect(0, 0, Bounds.Width, Bounds.Height));

        // 清除选择区域的遮罩
        context.FillRectangle(Brushes.Transparent, rect);

        // 绘制选择框边框
        context.DrawRectangle(
            null,
            new Pen(Brushes.Red, 1),
            rect);
    }
}

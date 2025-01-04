using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using ScreenShot.Models;
using System;
using System.Collections.Generic;
using Avalonia.Threading;

namespace ScreenShot.Views
{
    public partial class EditWindow : Window
    {
        private volatile bool isClosing = false;
        private Bitmap? currentImage;

        public EditWindow()
        {
            InitializeComponent();
            
            ArrowButton.Click += ArrowButton_Click;
            TextButton.Click += TextButton_Click;
            RectangleButton.Click += RectangleButton_Click;
            SaveButton.Click += SaveButton_Click;
            CopyButton.Click += CopyButton_Click;
            CloseButton.Click += CloseButton_Click;

            // 设置默认工具
            Canvas.CurrentTool = DrawingTool.Arrow;

            // 添加窗口关闭事件处理
            Closing += EditWindow_Closing;

            // 添加窗口加载完成事件
            Opened += EditWindow_Opened;
        }

        private void EditWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            if (isClosing) return;
            isClosing = true;

            // 确保在UI线程上关闭窗口
            if (!Dispatcher.UIThread.CheckAccess())
            {
                Dispatcher.UIThread.Post(() => Close());
                e.Cancel = true;
            }
        }

        private void EditWindow_Opened(object? sender, EventArgs e)
        {
            if (currentImage != null)
            {
                ApplyImage(currentImage);
            }
        }

        public void SetImage(Bitmap image)
        {
            if (image == null) return;

            currentImage = image;
            if (IsVisible)
            {
                ApplyImage(image);
            }
        }

        private void ApplyImage(Bitmap image)
        {
            try
            {
                // 设置窗口大小
                var screenWidth = Screens.Primary?.Bounds.Width ?? 1920;
                var screenHeight = Screens.Primary?.Bounds.Height ?? 1080;
                
                // 计算适当的窗口大小，确保不超过屏幕大小的80%
                var maxWidth = screenWidth * 0.8;
                var maxHeight = screenHeight * 0.8;
                
                var imageWidth = image.PixelSize.Width;
                var imageHeight = image.PixelSize.Height;
                
                // 如果图片尺寸超过最大限制，按比例缩小
                if (imageWidth > maxWidth || imageHeight > maxHeight)
                {
                    var widthRatio = maxWidth / imageWidth;
                    var heightRatio = maxHeight / imageHeight;
                    var ratio = Math.Min(widthRatio, heightRatio);
                    
                    Width = imageWidth * ratio + 40; // 加上边距
                    Height = imageHeight * ratio + 100; // 加上工具栏和边距的高度
                }
                else
                {
                    Width = imageWidth + 40;
                    Height = imageHeight + 100;
                }
                
                Canvas.SetImage(image);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"设置图片时出错: {ex}");
            }
        }

        private void ArrowButton_Click(object? sender, RoutedEventArgs e)
        {
            Canvas.CurrentTool = DrawingTool.Arrow;
        }

        private void TextButton_Click(object? sender, RoutedEventArgs e)
        {
            Canvas.CurrentTool = DrawingTool.Text;
        }

        private void RectangleButton_Click(object? sender, RoutedEventArgs e)
        {
            Canvas.CurrentTool = DrawingTool.Rectangle;
        }

        private async void SaveButton_Click(object? sender, RoutedEventArgs e)
        {
            try
            {
                var options = new FilePickerSaveOptions
                {
                    Title = "保存截图",
                    DefaultExtension = ".png",
                    FileTypeChoices = new List<FilePickerFileType>
                    {
                        new FilePickerFileType("PNG 图片") { Patterns = new[] { "*.png" } }
                    }
                };

                var file = await StorageProvider.SaveFilePickerAsync(options);
                if (file != null)
                {
                    await Canvas.SaveToFileAsync(file);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"保存文件时出错: {ex}");
            }
        }

        private async void CopyButton_Click(object? sender, RoutedEventArgs e)
        {
            try
            {
                await Canvas.CopyToClipboardAsync();
                Close();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"复制到剪贴板时出错: {ex}");
            }
        }

        private void CloseButton_Click(object? sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}

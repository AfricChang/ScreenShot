using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace ScreenShot.Platform
{
    public class WindowsScreenshotService : IScreenshotService
    {
        public async Task<Bitmap?> CaptureScreenAsync(int x, int y, int width, int height)
        {
            var screen = Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop ?
                desktop.MainWindow?.Screens.Primary :
                null;
            if (screen == null) return null;

            using var bitmap = new System.Drawing.Bitmap(width, height);
            using var graphics = System.Drawing.Graphics.FromImage(bitmap);
            graphics.CopyFromScreen(x, y, 0, 0, new System.Drawing.Size(width, height));

            using var ms = new MemoryStream();
            bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            ms.Position = 0;

            return new Bitmap(ms);
        }

        public async Task SaveImageAsync(Bitmap image, string filePath)
        {
            await using var stream = File.OpenWrite(filePath);
            image.Save(stream);
        }

        public async Task CopyImageToClipboardAsync(Bitmap image)
        {
            var pixelSize = image.PixelSize;
            var dpi = image.Dpi;

            using var renderBitmap = new RenderTargetBitmap(pixelSize, dpi);
            using (var context = renderBitmap.CreateDrawingContext())
            {
                var rect = new Rect(0, 0, pixelSize.Width, pixelSize.Height);
                context.DrawImage(image, rect, rect);
            }

            // 使用Windows API设置DIB格式
            var width = renderBitmap.PixelSize.Width;
            var height = renderBitmap.PixelSize.Height;
            
            // 获取像素数据
            var pixelBufferSize = width * height * 4;
            var pixels = Marshal.AllocHGlobal(pixelBufferSize);
            try
            {
                renderBitmap.CopyPixels(new PixelRect(0, 0, width, height), 
                    pixels, width * 4, 0);
                    
                // 创建DIB格式
                var dibData = CreateDIBFromPixels(width, height, pixels, pixelBufferSize);
                ClipboardHelper.SetDIB(dibData);
            }
            finally
            {
                Marshal.FreeHGlobal(pixels);
            }
        }

        private byte[] CreateDIBFromPixels(int width, int height, IntPtr pixels, int pixelBufferSize)
        {
            // 每行字节数必须是4的倍数
            var stride = (width * 3 + 3) & ~3;
            var imageSize = stride * height;
            var headerSize = 40;
            var dibSize = headerSize + imageSize;
            var dibData = new byte[dibSize];

            // 写入BITMAPINFOHEADER
            using (var writer = new BinaryWriter(new MemoryStream(dibData)))
            {
                writer.Write(40); // biSize
                writer.Write(width); // biWidth
                writer.Write(-height); // biHeight (负值表示从上到下的位图)
                writer.Write((short)1); // biPlanes
                writer.Write((short)24); // biBitCount (24-bit RGB)
                writer.Write(0); // biCompression
                writer.Write(imageSize); // biSizeImage
                writer.Write(2835); // biXPelsPerMeter (72 DPI)
                writer.Write(2835); // biYPelsPerMeter (72 DPI)
                writer.Write(0); // biClrUsed
                writer.Write(0); // biClrImportant
            }

            // 复制并转换像素数据
            var srcData = new byte[pixelBufferSize];
            Marshal.Copy(pixels, srcData, 0, pixelBufferSize);

            for (int y = 0; y < height; y++)
            {
                var srcOffset = y * width * 4;
                var dstOffset = headerSize + (height - y - 1) * stride;
                
                for (int x = 0; x < width; x++)
                {
                    // 从ARGB转换为RGB
                    var srcPixel = srcOffset + x * 4;
                    var dstPixel = dstOffset + x * 3;
                    
                    dibData[dstPixel + 0] = srcData[srcPixel + 0]; // Blue
                    dibData[dstPixel + 1] = srcData[srcPixel + 1]; // Green
                    dibData[dstPixel + 2] = srcData[srcPixel + 2]; // Red
                }
            }

            return dibData;
        }

        private static class ClipboardHelper
        {
            [DllImport("user32.dll")]
            private static extern bool OpenClipboard(IntPtr hWndNewOwner);

            [DllImport("user32.dll")]
            private static extern bool EmptyClipboard();

            [DllImport("user32.dll")]
            private static extern IntPtr SetClipboardData(uint uFormat, IntPtr hMem);

            [DllImport("user32.dll")]
            private static extern bool CloseClipboard();

            private const uint CF_DIB = 8;
            private const uint CF_DIBV5 = 17;

            public static void SetDIB(byte[] dibData)
            {
                OpenClipboard(IntPtr.Zero);
                EmptyClipboard();
                
                var hGlobal = Marshal.AllocHGlobal(dibData.Length);
                try
                {
                    Marshal.Copy(dibData, 0, hGlobal, dibData.Length);
                    
                    if (SetClipboardData(CF_DIB, hGlobal) == IntPtr.Zero)
                    {
                        SetClipboardData(CF_DIBV5, hGlobal);
                    }
                }
                finally
                {
                    CloseClipboard();
                }
            }
        }
    }
}

using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using SysDraw = System.Drawing;
using System.IO;
using System.Runtime.Versioning;

namespace ScreenShot.Platform;

[SupportedOSPlatform("windows6.1")]
public class WindowsScreenCapture : IScreenCapture, IDisposable
{
    [DllImport("user32.dll")]
    private static extern IntPtr GetDC(IntPtr hwnd);

    [DllImport("user32.dll")]
    private static extern bool ReleaseDC(IntPtr hwnd, IntPtr hdc);

    [DllImport("gdi32.dll")]
    private static extern bool BitBlt(IntPtr hdcDest, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, uint dwRop);

    private const uint SRCCOPY = 0x00CC0020;

    public async Task<Bitmap?> CaptureScreenAsync(double x, double y, double width, double height)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return null;

        if (!OperatingSystem.IsWindowsVersionAtLeast(6, 1))
            return null;

        return await Task.Run(() =>
        {
            try
            {
                // 获取屏幕DC
                IntPtr screenDC = GetDC(IntPtr.Zero);
                if (screenDC == IntPtr.Zero)
                    return null;

                try
                {
                    // 创建GDI+位图
                    using var bitmap = new SysDraw.Bitmap((int)width, (int)height, SysDraw.Imaging.PixelFormat.Format32bppArgb);
                    using (var graphics = SysDraw.Graphics.FromImage(bitmap))
                    {
                        var hdcBitmap = graphics.GetHdc();
                        try
                        {
                            // 从屏幕复制到GDI+位图
                            BitBlt(hdcBitmap, 0, 0, (int)width, (int)height, screenDC, (int)x, (int)y, SRCCOPY);
                        }
                        finally
                        {
                            graphics.ReleaseHdc(hdcBitmap);
                        }
                    }

                    // 将GDI+位图转换为DIB格式
                    var bitmapData = bitmap.LockBits(
                        new SysDraw.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                        SysDraw.Imaging.ImageLockMode.ReadOnly,
                        SysDraw.Imaging.PixelFormat.Format32bppArgb);

                    try
                    {
                        // 创建Avalonia位图
                        var avalonBitmap = new WriteableBitmap(
                            new PixelSize(bitmap.Width, bitmap.Height),
                            new Vector(96, 96),
                            PixelFormat.Bgra8888,
                            AlphaFormat.Premul);

                        using (var fb = avalonBitmap.Lock())
                        {
                            unsafe
                            {
                                var sourcePtr = (byte*)bitmapData.Scan0;
                                var destPtr = (byte*)fb.Address;
                                var stride = Math.Min(bitmapData.Stride, fb.RowBytes);
                                var height = Math.Min(bitmap.Height, fb.Size.Height);

                                for (int y = 0; y < height; y++)
                                {
                                    Buffer.MemoryCopy(
                                        sourcePtr + y * bitmapData.Stride,
                                        destPtr + y * fb.RowBytes,
                                        stride,
                                        stride);
                                }
                            }
                        }

                        return avalonBitmap;
                    }
                    finally
                    {
                        bitmap.UnlockBits(bitmapData);
                    }
                }
                finally
                {
                    ReleaseDC(IntPtr.Zero, screenDC);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"截图时出错: {ex}");
                return null;
            }
        });
    }

    public void Dispose()
    {
        // 清理资源
        GC.SuppressFinalize(this);
    }
}

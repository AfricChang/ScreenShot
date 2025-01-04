using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace ScreenShot.Platform;

public class WindowsScreenCapture : IScreenCapture
{
    [DllImport("gdi32.dll")]
    private static extern bool BitBlt(IntPtr hdcDest, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, uint dwRop);

    [DllImport("gdi32.dll")]
    private static extern IntPtr CreateDC(string? lpszDriver, string? lpszDevice, string? lpszOutput, IntPtr lpInitData);

    [DllImport("gdi32.dll")]
    private static extern IntPtr CreateCompatibleDC(IntPtr hdc);

    [DllImport("gdi32.dll")]
    private static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);

    [DllImport("gdi32.dll")]
    private static extern IntPtr SelectObject(IntPtr hdc, IntPtr hObject);

    [DllImport("gdi32.dll")]
    private static extern bool DeleteDC(IntPtr hdc);

    [DllImport("gdi32.dll")]
    private static extern bool DeleteObject(IntPtr hObject);

    private const uint SRCCOPY = 0x00CC0020;

    public async Task<Bitmap?> CaptureScreenAsync(double x, double y, double width, double height)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return null;

        return await Task.Run(() =>
        {
            IntPtr screenDC = CreateDC("DISPLAY", null, null, IntPtr.Zero);
            IntPtr memoryDC = CreateCompatibleDC(screenDC);
            IntPtr bitmap = CreateCompatibleBitmap(screenDC, (int)width, (int)height);
            IntPtr oldBitmap = SelectObject(memoryDC, bitmap);

            try
            {
                BitBlt(memoryDC, 0, 0, (int)width, (int)height, screenDC, (int)x, (int)y, SRCCOPY);

                // 创建 WriteableBitmap
                var writeableBitmap = new WriteableBitmap(new PixelSize((int)width, (int)height), 
                    new Vector(96, 96), 
                    PixelFormat.Bgra8888, 
                    AlphaFormat.Premul);

                using (var fb = writeableBitmap.Lock())
                {
                    // 从 GDI bitmap 复制数据到 WriteableBitmap
                    BitBlt(memoryDC, 0, 0, (int)width, (int)height, (IntPtr)fb.Address, 0, 0, SRCCOPY);
                }

                return writeableBitmap;
            }
            finally
            {
                SelectObject(memoryDC, oldBitmap);
                DeleteObject(bitmap);
                DeleteDC(memoryDC);
                DeleteDC(screenDC);
            }
        });
    }
}

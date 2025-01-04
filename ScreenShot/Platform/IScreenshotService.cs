using Avalonia.Media.Imaging;
using System.Threading.Tasks;

namespace ScreenShot.Platform
{
    public interface IScreenshotService
    {
        Task<Bitmap?> CaptureScreenAsync(int x, int y, int width, int height);
        Task SaveImageAsync(Bitmap image, string filePath);
        Task CopyImageToClipboardAsync(Bitmap image);
    }
}

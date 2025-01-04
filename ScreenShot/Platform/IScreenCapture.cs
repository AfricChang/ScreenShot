using Avalonia.Media.Imaging;
using System.Threading.Tasks;

namespace ScreenShot.Platform;

public interface IScreenCapture
{
    Task<Bitmap?> CaptureScreenAsync(double x, double y, double width, double height);
}

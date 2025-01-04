using Avalonia.Media.Imaging;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace ScreenShot.Platform;

public class MacScreenCapture : IScreenCapture
{
    public Task<Bitmap?> CaptureScreenAsync(double x, double y, double width, double height)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return Task.FromResult<Bitmap?>(null);

        // TODO: 实现 MacOS 的截图功能
        throw new NotImplementedException("MacOS screenshot capture not implemented yet");
    }
}

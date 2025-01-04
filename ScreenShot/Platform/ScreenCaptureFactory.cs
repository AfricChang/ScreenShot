using System;
using System.Runtime.InteropServices;

namespace ScreenShot.Platform;

public static class ScreenCaptureFactory
{
    public static IScreenCapture Create()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return new WindowsScreenCapture();
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return new MacScreenCapture();
        // TODO: 添加 Linux 支持
        throw new PlatformNotSupportedException("Current platform is not supported");
    }
}

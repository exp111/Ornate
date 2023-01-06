using System.Runtime.InteropServices;
using Avalonia;
using Xilium.CefGlue;
using Xilium.CefGlue.Common;
using Xilium.CefGlue.Common.Shared;

namespace Ornate.Lite
{
    class Program
    {

        static int Main(string[] args)
        {
            AppBuilder.Configure<App>()
                      .UsePlatformDetect()
                      .With(new Win32PlatformOptions
                      {
                          UseWindowsUIComposition = false
                      })
                      .AfterSetup(_ => CefRuntimeLoader.Initialize(new CefSettings() {
#if WINDOWLESS
                          WindowlessRenderingEnabled = true
#else
                          WindowlessRenderingEnabled = false
#endif
                      }))
                      .StartWithClassicDesktopLifetime(args);
                      
            return 0;
        }
    }
}

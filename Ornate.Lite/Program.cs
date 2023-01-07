using System.IO;
using Avalonia;
using Xilium.CefGlue;
using Xilium.CefGlue.Common;

namespace Ornate.Lite
{
    class Program
    {

        static void Main(string[] args) => BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);

        static AppBuilder BuildAvaloniaApp() =>
            AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .With(new Win32PlatformOptions
                {
                    UseWindowsUIComposition = false
                })
                .AfterSetup(_ => CefRuntimeLoader.Initialize(new CefSettings()
                { //TODO: instead use a mobile user agent?
                    LogFile = Path.GetFullPath("cef.log"),
                    CachePath = Path.GetFullPath("cache"), // needed for localstorage to persist
                }));
    }
}

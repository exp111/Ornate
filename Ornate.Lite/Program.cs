using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Threading;
using CefNet;
using Ornate.Lite.CefNetStuff;

namespace Ornate.Lite
{
    class Program
    {
        private static CefAppImpl app;

        [STAThread]
        public static void Main(string[] args) //TODO: game crashes after a few seconds
        {
            try
            {
                var path = Path.GetFullPath("CEF");
                var settings = new CefSettings();
                settings.MultiThreadedMessageLoop = true;
                settings.NoSandbox = true;
                settings.WindowlessRenderingEnabled = true;
                settings.LocalesDirPath = Path.Combine(path, "Resources", "locales");
                settings.ResourcesDirPath = Path.Combine(path, "Resources");
                settings.LogSeverity = CefLogSeverity.Warning;
                settings.UncaughtExceptionStackSize = 8;
                settings.CachePath = Path.GetFullPath("cache");
                settings.LogFile = Path.GetFullPath("cef.log");
                //TODO: spoof android user agent

                app = new CefAppImpl();
                //app.CefProcessMessageReceived += App_CefProcessMessageReceived;
                app.ScheduleMessagePumpWorkCallback = OnScheduleMessagePumpWork;

                app.Initialize(Path.Combine(path, "Release"), settings);

                BuildAvaloniaApp().StartWithCefNetApplicationLifetime(args);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public static AppBuilder BuildAvaloniaApp() =>
            AppBuilder.Configure<App>()
                .UsePlatformDetect().LogToTrace();

        private static async void OnScheduleMessagePumpWork(long delayMs)
        {
            await Task.Delay((int)delayMs);
            Dispatcher.UIThread.Post(CefApi.DoMessageLoopWork);
        }

        //TODO: CefProcessMessageReceived?
    }
}

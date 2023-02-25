using System;
using Avalonia;
using Microsoft.Win32;

namespace Ornate.Lite
{
    class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            // If urn:schemas-microsoft-com:compatibility.v1 supportedOS exists in the app.manifest file, the control will not display normally, but it is normal in WPF
            if (IsProgramInCompatibilityMode())
            {
                //TODO: alert the user
                Console.WriteLine("Windows Program Compatibility mode is on. Turn it off and then try again");
                return;
            }

            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args, Avalonia.Controls.ShutdownMode.OnMainWindowClose);
        }

        static bool IsProgramInCompatibilityMode()
        {
            try
            {
                foreach (var item in new[] { Registry.CurrentUser, Registry.LocalMachine })
                {
                    using var layers = item.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\Layers");
                    var value = layers?.GetValue(Environment.ProcessPath)?.ToString();
                    if (value != null)
                    {
                        if (value.Contains("WIN8RTM", StringComparison.OrdinalIgnoreCase)) return true;
                        if (value.Contains("WIN7RTM", StringComparison.OrdinalIgnoreCase)) return true;
                    }
                }
            }
            catch
            {
            }
            return false;
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToTrace();
    }
}

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using System;
using System.Globalization;
using System.IO;

namespace Ornate.Lite
{
    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
            InitWebView2();
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow();
            }

            base.OnFrameworkInitializationCompleted();
        }

        static void InitWebView2()
        {
            if (WebView2.IsSupported)
            {
                WebView2.DefaultCreationProperties = new()
                {
                    Language = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName,
                    UserDataFolder = GetUserDataFolder(),
                };

                static string GetUserDataFolder()
                {
                    var path = Path.GetFullPath("UserData");
                    if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                    return path;
                }
            }
        }
    }
}

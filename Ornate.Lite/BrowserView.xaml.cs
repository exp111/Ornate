using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using Microsoft.Web.WebView2.Core;
using Ornate.Lite.Dialogs;
using System;
using System.IO;

namespace Ornate.Lite
{
    public class BrowserView : UserControl
    {
        private WebView2 browser;
        private TextBlock infoLabel;
        public static readonly string DataPath = "data";
        public static readonly string BundlePath = Path.Combine(DataPath, "bundle.html"); // Uses the relative path

        public BrowserView()
        {
            AvaloniaXamlLoader.Load(this);

            browser = this.FindControl<WebView2>("WebView");
            browser.CoreWebView2InitializationCompleted += (_, _) => OpenGame();
            browser.EnsureCoreWebView2Async(); // Force initialization because Source property isnt set

            // Do loading screen
            browser.IsVisible = false;
            infoLabel = this.FindControl<TextBlock>("InfoLabel");
            if (!WebView2.IsSupported)
            {
                infoLabel.Text = "Couldn't find a compatible Webview2 Runtime installation to host WebViews.";
            }
            else
            {
                browser.DOMContentLoaded += (_,_) => browser.IsVisible = true; ;
            }
        }

        // Checks if the game data exists (or at least the start file)
        public static bool CheckForData()
        {
            return File.Exists(BundlePath);
        }

        public void SetGeolocation()
        {
            //TODO: make this callable
            //TODO: adapt for webview2, https://learn.microsoft.com/en-us/microsoft-edge/webview2/how-to/chromium-devtools-protocol
            //browser.ExecuteDevToolsMethodAsync("Emulation.setGeolocationOverride",
            //    """{"latitude":52.520007,"longitude":13.404954,"accuracy":150}""");
        }

        // Opens the game in the browser
        public bool OpenGame()
        {
            if (CheckForData())
            {
                browser.CoreWebView2?.Navigate($"file://{Path.GetFullPath(BundlePath)}");
                return true;
            }
            else
            {
                // inform the user better
                OKWindow noAssetsFoundWindow = new()
                {
                    Title = "Error",
                    Prompt = $"No data folder or bundle.idx found under {Path.GetFullPath(BundlePath)}.\nExtract the data through File > Extract Content from APK and reload the game."
                };
                noAssetsFoundWindow.ShowDialog(this.FindAncestorOfType<MainWindow>());
                return false;
            }
        }

        public event Action<string> TitleChanged;

        private void OnBrowserTitleChanged(object sender, string title)
        {
            TitleChanged?.Invoke(title);
        }

        public void ReloadGame()
        {
            OpenGame();
        }

        public void OpenDevTools()
        {
            browser.CoreWebView2?.OpenDevToolsWindow();
        }

        public void SetAddress(string url)
        {
            browser.CoreWebView2?.Navigate(url);
        }

        public void Mute(bool muted)
        {
            if (browser.CoreWebView2 != null)
                browser.CoreWebView2.IsMuted = muted;
        }

        public void Dispose()
        {
            browser.Dispose();
        }
    }
}
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using CefNet;
using CefNet.Avalonia;
using CefNet.DevTools.Protocol.Emulation;
using Ornate.Lite.CefNetStuff;
using Ornate.Lite.Dialogs;
using System;
using System.IO;

namespace Ornate.Lite
{
    public class BrowserView : UserControl
    {
        private WebView browser;
        public static readonly string DataPath = "data";
        public static readonly string BundlePath = Path.Combine(DataPath, "bundle.html"); // Uses the relative path

        public BrowserView()
        {
            AvaloniaXamlLoader.Load(this);

            var browserWrapper = this.FindControl<Decorator>("browserWrapper");

            browser = new CustomWebView();
            browser.BrowserCreated += (_, _) => OpenGame();
            browserWrapper.Child = browser;
        }

        // Checks if the game data exists (or at least the start file)
        public static bool CheckForData()
        {
            return File.Exists(BundlePath);
        }

        public void SetGeolocation()
        {
            //TODO: make this callable
            browser.ExecuteDevToolsMethodAsync("Emulation.setGeolocationOverride",
                """{"latitude":52.520007,"longitude":13.404954,"accuracy":150}""");
        }
        // Opens the game in the browser
        public bool OpenGame()
        {
            if (CheckForData())
            {
                browser.Navigate($"file://{Path.GetFullPath(BundlePath)}");
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
            browser.ShowDevTools();
        }

        public void SetAddress(string url)
        {
            browser.Navigate(url);
        }

        public void Dispose()
        {
            browser.Close();
        }
    }
}
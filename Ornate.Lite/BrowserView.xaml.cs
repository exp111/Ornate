using Avalonia.Controls;
using Avalonia.VisualTree;
using GMap.NET;
using Microsoft.Web.WebView2.Core.DevToolsProtocolExtension;
using Ornate.Lite.Dialogs;
using Ornate.Lite.WebView;
using System;
using System.IO;

namespace Ornate.Lite
{
    public partial class BrowserView : UserControl
    {
        public DevToolsProtocolHelper DevTools;
        public WebView2 browser;
        private TextBlock infoLabel;
        public static readonly string DataPath = "data";
        public static readonly string BundlePath = Path.Combine(DataPath, "bundle.html"); // Uses the relative path //TODO: change back to idx when it works?

        public event EventHandler<BrowserInitArgs> OnBrowserInit;

        public Sniffer sniffer;

        public BrowserView()
        {
            InitializeComponent();

            if (Design.IsDesignMode)
                return;

            browser = this.FindControl<WebView2>("WebView");
            browser.CoreWebView2InitializationCompleted += (_, _) => Init();
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
                browser.DOMContentLoaded += (_, _) => browser.IsVisible = true; ;
            }
        }

        private void Init()
        {
            //TODO: handle exception
            DevTools = browser.CoreWebView2?.GetDevToolsProtocolHelper();
            sniffer = new(browser, DevTools);
            sniffer.Start(); //TODO: sniffer toggle

            browser.CoreWebView2.Settings.UserAgent = "Mozilla/5.0 (iPhone; CPU iPhone OS 16_3_1 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Mobile/15E148"; //TODO: change to android?
#if !DEBUG
            OpenGame(); //TODO: autostart config
#endif

            OnBrowserInit?.Invoke(this, new BrowserInitArgs(browser));
        }

        // Checks if the game data exists (or at least the start file)
        public static bool CheckForData()
        {
            return File.Exists(BundlePath);
        }

        public void SetGeolocation(double? latitude, double? longitude, double? accuracy)
        {
            DevTools.Emulation.SetGeolocationOverrideAsync(latitude, longitude, accuracy);
        }

        public void SetGeolocation(PointLatLng pos)
        {
            SetGeolocation(pos.Lat, pos.Lng, 150);
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
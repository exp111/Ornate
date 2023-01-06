using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using SkiaSharp;
using System;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Xilium.CefGlue.Avalonia;
using Xilium.CefGlue.Common.Events;

namespace Ornate.Lite
{
    public class BrowserView : UserControl
    {
        private AvaloniaCefBrowser browser;
        private static readonly string BundlePath = Path.Combine("data", "bundle.idx");

        public BrowserView()
        {
            AvaloniaXamlLoader.Load(this);

            var browserWrapper = this.FindControl<Decorator>("browserWrapper");

            browser = new AvaloniaCefBrowser();
            //browser.Settings.FileAccessFromFileUrls = CefState.Enabled; //TODO: needed?
            //browser.LoadStart += OnBrowserLoadStart; // probably not needed if we delete the address bar
            browser.TitleChanged += OnBrowserTitleChanged;
            // Needed to run .idx files as html
            browser.RequestHandler = new IDXRequestHandler(); //TODO: instead just rename the file to .html?
            //browser.Settings.UniversalAccessFromFileUrls = CefState.Enabled;
            browserWrapper.Child = browser;

            //TODO: save localstorage on change (and load on startup)
            //TODO: GPS

            OpenGame();
        }

        // Checks if the game data exists (or at least the start file)
        static bool CheckForData()
        {
            return File.Exists(BundlePath);
        }

        // Opens the game in the browser
        public bool OpenGame()
        {
            if (CheckForData())
            {
                browser.Address = $"file://{Path.GetFullPath(BundlePath)}";
                return true;
            }
            else
            {
                //TODO: inform the user better
                browser.Address = $"javascript:alert('No data folder or bundle.idx found: {Path.GetFullPath(BundlePath).Replace(@"\", @"\\")}')";
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
            browser.ShowDeveloperTools();
        }

        public void Dispose()
        {
            browser.Dispose();
        }
    }
}
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
using Xilium.CefGlue;
using Xilium.CefGlue.Avalonia;
using Xilium.CefGlue.Common.Events;

namespace Ornate.Lite
{
    public class BrowserView : UserControl
    {
        private AvaloniaCefBrowser browser;
        public static readonly string DataPath = "data";
        public static readonly string BundlePath = Path.Combine(DataPath, "bundle.idx"); // Uses the relative path

        public BrowserView()
        {
            AvaloniaXamlLoader.Load(this);

            var browserWrapper = this.FindControl<Decorator>("browserWrapper");

            browser = new AvaloniaCefBrowser();
            //browser.Settings.FileAccessFromFileUrls = CefState.Enabled; //TODO: needed?
            browser.Settings.UniversalAccessFromFileUrls = CefState.Enabled;
            browser.TitleChanged += OnBrowserTitleChanged;
            // Needed to run .idx files as html
            browser.RequestHandler = new IDXRequestHandler(); //TODO: instead just rename the file to .html?
            browser.Settings.ApplicationCache = CefState.Enabled;
            browser.Settings.LocalStorage = CefState.Enabled;
            browserWrapper.Child = browser;
            
            //TODO: need to grant geolocation permissions
            //TODO: GPS location

            OpenGame();
        }

        // Checks if the game data exists (or at least the start file)
        public static bool CheckForData()
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

        public void SetAddress(string url)
        {
            browser.Address = url;
        }

        public void Dispose()
        {
            browser.Dispose();
        }
    }
}
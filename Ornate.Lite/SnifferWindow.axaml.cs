using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Ornate.Lite.WebView;
using System.Linq;

namespace Ornate.Lite
{
    public class SnifferWindow : Window //TODO: move sniffer func into browser(view), dont open this window on start, instead only show data from sniffer
    {
        public BrowserView BrowserView;
        public WebView2 Browser;
        public Sniffer Sniffer;

        public SnifferWindow()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            var lifetime = (IClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime;
            var mainWindow = lifetime.MainWindow;
            if (mainWindow == null) //TODO: null if called from mainwindow constructor
                return;
            BrowserView = mainWindow.FindControl<BrowserView>("browser");
            Browser = BrowserView.browser;
            Sniffer = BrowserView.sniffer;
            var messages = Sniffer.Messages;
            var first = messages.First(m => m.Value.Request.Url.StartsWith("https://playorna.com/"));
            var req = first.Value.Request;
            var resp = first.Value.Response;
            var b = resp;
        }
    }
}

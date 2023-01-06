using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using System;
using System.Collections;
using Xilium.CefGlue.Common;

namespace Ornate.Lite
{
    public class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            CreateBrowserView();

            var mainMenu = this.FindControl<Menu>("mainMenu");
            mainMenu.AttachedToVisualTree += MenuAttached;
        }

        private void MenuAttached(object sender, VisualTreeAttachmentEventArgs e)
        {
            if (NativeMenu.GetIsNativeMenuExported(this) && sender is Menu mainMenu)
            {
                mainMenu.IsVisible = false;
            }
        }

        private BrowserView ActiveBrowserView => (BrowserView) this.FindControl<Decorator>("browser").Child;

        private void CreateBrowserView()
        {
            var view = new BrowserView();
            this.FindControl<Decorator>("browser").Child = view; //TODO: instead add multiple tabs again for multi accounting?
        }

        private void OnReloadGameNativeMenuItemClick(object sender, EventArgs e)
        {
            ActiveBrowserView.ReloadGame();
        }

        private void OnOpenDevToolsNativeMenuItemClick(object sender, EventArgs e)
        {
            ActiveBrowserView.OpenDevTools();
        }

        private void OnReloadGameMenuItemClick(object sender, RoutedEventArgs e) => OnReloadGameNativeMenuItemClick(sender, e);

        private void OnOpenDevToolsMenuItemClick(object sender, RoutedEventArgs e) => OnOpenDevToolsNativeMenuItemClick(sender, e);
    }
}

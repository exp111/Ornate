using Avalonia;
using Avalonia.Controls;
using Avalonia.Dialogs;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
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

        private async void OnExtractAPKNativeMenuItemClick(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new()
            {
                AllowMultiple = false,
                Filters = new()
                {
                    new()
                    {
                        Name = "APK Files",
                        Extensions = new(){ "apk" }
                    },
                    new()
                    {
                        Name = "All Files",
                        Extensions = new(){ "*" }
                    }
                },

            };
            var files = await openFileDialog.ShowAsync(this);
            if (files == null || files.Length == 0) // Nothing given
                return;

            //TODO: validate apk?

            if (Directory.Exists(BrowserView.DataPath))
            {
                //TODO: warn user + let them confirm
                // delete folder
                Directory.Delete(BrowserView.DataPath, true);
            }

            //TODO: kill browser beforehand?
            // open the apk as a zip, extract the "assets" folder content into "data"
            const string assetsFolder = "assets/";
            var zip = ZipFile.OpenRead(files[0]);
            // find all entries in "assets" folder and below
            var assets = zip.Entries.Where(e => e.FullName.StartsWith(assetsFolder));
            if (!assets.Any()) // didnt find anything
                return; //TODO: warning

            //TODO: progress bar
            foreach (var asset in assets)
            {
                // put the path together: "data/" + path of file we're looking at (removing the "assets/" at the start)
                var path = Path.Combine(BrowserView.DataPath, asset.FullName.Substring(assetsFolder.Length));
                // check if parent dir exists => if not create it
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(Path.GetDirectoryName(path));

                asset.ExtractToFile(path, true);
            }
            //TODO: done info
        }

        private void OnReloadGameMenuItemClick(object sender, RoutedEventArgs e) => OnReloadGameNativeMenuItemClick(sender, e);

        private void OnOpenDevToolsMenuItemClick(object sender, RoutedEventArgs e) => OnOpenDevToolsNativeMenuItemClick(sender, e);

        private void OnExtractAPKMenuItemClick(object sender, RoutedEventArgs e) => OnExtractAPKNativeMenuItemClick(sender, e);
    }
}

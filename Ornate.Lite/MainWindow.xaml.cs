using Avalonia;
using Avalonia.Controls;
using Avalonia.Dialogs;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Ornate.Lite.Dialogs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
            //TODO: add autostart option to disable autostart?
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
                // warn user + let them confirm
                ConfirmationWindow warningDialog = new()
                {
                    Title = "Warning!",
                    Prompt = "You are about to delete and replace your existing content. Are you sure?"
                };
                var result = await warningDialog.ShowDialog<bool?>(this);
                // user canceled or aborted
                if (!result.HasValue || !result.Value)
                    return;

                // Inform the user that we're deleting files, so it doesnt look like the thing just lags
                InfoWindow deletingFilesDialog = new()
                {
                    Title = "Deleting...",
                    Prompt = "Deleting old files..."
                };
                // Run the deletion inside a new thread to not block the UI thread
                deletingFilesDialog.Opened += (_, _) =>
                {
                    var worker = new Thread(() =>
                    {
                        // Delete the folder
                        Directory.Delete(BrowserView.DataPath, true);
                        // Close the dialog from the UI thread
                        Dispatcher.UIThread.Post(() => deletingFilesDialog.Close());
                    });
                    worker.Start();
                };
                await deletingFilesDialog.ShowDialog(this);
            }

            //TODO: kill browser beforehand?
            // open the apk as a zip, extract the "assets" folder content into "data"
            const string assetsFolder = "assets/";
            var zip = ZipFile.OpenRead(files[0]);
            // find all entries in "assets" folder and below
            var assets = zip.Entries.Where(e => e.FullName.StartsWith(assetsFolder));
            if (!assets.Any()) // didnt find anything
            {
                // inform the user about this
                OKWindow noAssetsFoundWindow = new()
                {
                    Title = "Error",
                    Prompt = $"Didn't find a 'assets' folder inside {files[0]}."
                };
                await noAssetsFoundWindow.ShowDialog(this);
                return;
            }

            // create a progress bar for the user
            ProgressBarWindow progressBar = new()
            {
                Title = "Extracting files...",
                Text = "Extracting files...",
                Min = 0,
                Max = 100,
                Value = 0,
            };
            progressBar.Opened += (_, _) =>
            {
                // Run the extraction in a new thread to not block the UI thread
                var worker = new Thread(() =>
                {
                    var total = (float)assets.Count();
                    var current = 0f;
                    foreach (var asset in assets)
                    {
                        // put the path together: "data/" + path of file we're looking at (removing the "assets/" at the start)
                        var path = Path.Combine(BrowserView.DataPath, asset.FullName.Substring(assetsFolder.Length));
                        // check if parent dir exists => if not create it
                        if (!Directory.Exists(path))
                            Directory.CreateDirectory(Path.GetDirectoryName(path));

                        asset.ExtractToFile(path, true);
                        current++;
                        // Update the progress bar in the ui thread
                        Dispatcher.UIThread.Post(() => progressBar.Value = (int)((current / total)*100));
                    }
                    // Close the dialog from the UI thread
                    Dispatcher.UIThread.Post(() => progressBar.Close());
                });
                worker.Start();
            };
            await progressBar.ShowDialog(this);

            // show done info
            OKWindow okWindow = new()
            {
                Title = "Done",
                Prompt = $"Successfully extracted all files to {Path.GetFullPath(BrowserView.DataPath)}"
            };
            await okWindow.ShowDialog(this);
        }

        private async void OnDebugMenuItemClick(object sender, RoutedEventArgs e)
        {
            //TODO: remove when done
        }

        private void OnReloadGameMenuItemClick(object sender, RoutedEventArgs e) => OnReloadGameNativeMenuItemClick(sender, e);

        private void OnOpenDevToolsMenuItemClick(object sender, RoutedEventArgs e) => OnOpenDevToolsNativeMenuItemClick(sender, e);

        private void OnExtractAPKMenuItemClick(object sender, RoutedEventArgs e) => OnExtractAPKNativeMenuItemClick(sender, e);
    }
}

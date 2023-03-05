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

namespace Ornate.Lite
{
    public class MainWindow : Window
    {
        private BrowserView ActiveBrowserView;
        private SnifferWindow Sniffer; //TODO: start sniffer with main window
        public MainWindow()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            if (Design.IsDesignMode)
                return;

            CreateBrowserView();
        }

        private void CreateBrowserView()
        {
            //TODO: add autostart option to disable autostart?
            ActiveBrowserView = this.FindControl<BrowserView>("browser"); //TODO: instead add multiple tabs again for multi accounting?
        }

        private void OnReloadGameMenuItemClick(object sender, RoutedEventArgs e)
        {
            ActiveBrowserView.ReloadGame();
        }

        private void OnOpenDevToolsMenuItemClick(object sender, RoutedEventArgs e)
        {
            ActiveBrowserView.OpenDevTools();
        }

        private async void OnExtractAPKMenuItemClick(object sender, RoutedEventArgs e)
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

            // Copy bundle.idx to bundle.html //TODO: remove when idx works
            var idxPath = Path.Combine(BrowserView.DataPath, "bundle.idx");
            var htmlPath = Path.Combine(BrowserView.DataPath, "bundle.html");
            if (Path.Exists(idxPath))
            {
                File.Copy(idxPath, htmlPath, true);
            }
            // show done info
            OKWindow okWindow = new()
            {
                Title = "Done",
                Prompt = $"Successfully extracted all files to {Path.GetFullPath(BrowserView.DataPath)}"
            };
            await okWindow.ShowDialog(this);
        }

        private async void OnOpenWebsiteMenuItemClick(object sender, RoutedEventArgs e)
        {
            TextInputWindow inputDialog = new()
            {
                Title = "Open Website",
                Prompt = "Input the url:"
            };
            var result = await inputDialog.ShowDialog<string>(this);
            if (result == null)
                return;

            ActiveBrowserView.SetAddress(result);
        }

        private async void OnDebugMenuItemClick(object sender, RoutedEventArgs e)
        {
            //TODO: remove when done
            ActiveBrowserView.SetGeolocation(52.520007, 13.404954, 150);
        }

        private async void OnMuteMenuItemClick(object sender, RoutedEventArgs e)
        {
            //TODO: mute checkbox
            ActiveBrowserView.Mute(true);
        }

        //TODO: options window, auto sniffer attach, auto game start, mute sounds etc
        //TODO: gps window/sidebar

        private async void OnSnifferMenuItemClick(object sender, RoutedEventArgs e)
        {
            if (Sniffer == null)
                Sniffer = new SnifferWindow();
            else if (Sniffer.IsVisible) //TODO: close?
                return;
            else // active but not visible => closed => create new one 
                Sniffer = new SnifferWindow();

            Sniffer.Show(); //TODO: make this unshitty with a dialog or smth?
        }
    }
}

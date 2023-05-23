using Avalonia;
using Avalonia.Controls;
using Avalonia.Dialogs;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using GMap.NET;
using GMap.NET.Avalonia;
using GMap.NET.MapProviders;
using Ornate.Lite.Dialogs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ornate.Lite
{
    public partial class MainWindow : Window
    {
        private BrowserView ActiveBrowserView;
        private SnifferWindow Sniffer;
        public GMapControl MapControl { get; }
        public MainWindow()
        {
            InitializeComponent();

            if (Design.IsDesignMode)
                return;

            // Initialize the MapControl
            MapControl = this.FindControl<GMapControl>("GMap");
            LoadMap();

            CreateBrowserView();
        }
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
        private void LoadMap()
        {
            // Configure GMap.NET settings here, such as center coordinates, zoom level, etc.
            GMapProvider.WebProxy = System.Net.WebRequest.GetSystemWebProxy();
            GMapProvider.WebProxy.Credentials = System.Net.CredentialCache.DefaultNetworkCredentials;
            // Use OpenStreetMap
            MapControl.MapProvider = GMapProviders.OpenStreetMap;
            MapControl.Position = new PointLatLng(50.761119, 6.108917); //TODO: instead use current position
            MapControl.Zoom = 15; //TODO: adjust Map Zoom
            MapControl.FillEmptyTiles = true;
            MapControl.MouseWheelZoomEnabled = true;
            

            // Add a static marker at the specified location
            GMapMarker marker = new GMapMarker(MapControl.Position);
            marker.Shape = new Avalonia.Controls.Shapes.Ellipse
            {
                Width = 10,
                Height = 10,
                Fill = Avalonia.Media.Brushes.Red
            };
            MapControl.Markers.Add(marker);
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

            try
            {
                ActiveBrowserView.SetAddress(result);
            } 
            catch (Exception ex) 
            {
                // Show error window
                OKWindow errorWindow = new()
                {
                    Title = "Error",
                    Prompt = $"Invalid URL: {ex}"
                };
                await errorWindow.ShowDialog(this);
            }
        }

        private async void OnDebugMenuItemClick(object sender, RoutedEventArgs e)
        {
            //TODO: remove when done
            ActiveBrowserView.SetGeolocation(50.761119, 6.108917, 150);
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

        private async void OnCurrentLocationMenuItemClick(object sender, RoutedEventArgs e)
        {
            try
            {
                // Get the current geolocation
                var currentPosition = await GetCurrentGeolocation();

                // Find and remove markers set by the same function
                var existingCurrentLocationMarkers = MapControl.Markers.Where(marker => marker.Tag?.ToString() == "CurrentPositionMarker").ToList();
                foreach (var existingCurrentLocationMarker in existingCurrentLocationMarkers)   // Even though there should only be one marker at a time, we delete all of them. just in case
                {
                    MapControl.Markers.Remove(existingCurrentLocationMarker);
                }

                // Add a marker at the current position
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    GMapMarker marker = new GMapMarker(currentPosition);
                    marker.Shape = new Avalonia.Controls.Shapes.Ellipse
                    {
                        Width = 10,
                        Height = 10,
                        Fill = Avalonia.Media.Brushes.Red
                    };
                    marker.Tag = "CurrentPositionMarker";

                    MapControl.Markers.Add(marker);
                    MapControl.CenterPosition = currentPosition;
                });
            }
            catch (Exception ex)
            {
                // Handle the exception if unable to get the current location
                Console.WriteLine("Error getting current location: " + ex.Message);
            }
        }

        private async Task<PointLatLng> GetCurrentGeolocation()
        {
            // TODO: Implement the code to get the current geolocation

            // Dummy location for testing
            PointLatLng currentPosition = new PointLatLng(50.761119, 6.108917);

            return currentPosition;
        }
    }
}

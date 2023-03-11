using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;

namespace Ornate.Lite
{
    public class SnifferWindow : Window, INotifyPropertyChanged
    {
        public BrowserView BrowserView;
        public WebView2 Browser;
        public Sniffer Sniffer;

        //TODO: as we only need to change the text, we can bind their values to a property of ours
        public TextBlock RequestText;
        public TextBlock ResponseText;
        public TextBlock ParsedRequestText;
        public TextBlock ParsedResponseText;

        // Socket Tab
        public TextBlock ConnectionText;
        public TextBlock FrameText;
        public TextBlock FrameParsedText;

        private bool hideLocalRequests = true;
        private bool showOnlyOrnaRequests = true;
        private bool hideResourceRequests = false;
        private bool hideOptionsRequests = true;

        public class RequestItem
        {
            public string Text;
            public int Key;

            public override string ToString()
            {
                return Text;
            }
        }
        public ObservableCollection<RequestItem> requests = new();
        public ObservableCollection<RequestItem> sockets = new();
        public ObservableCollection<RequestItem> frames = new(); //TODO: use class that also contains Connection id?

        #region Bindings
        // Needed to notify the view that a property has changed
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ObservableCollection<RequestItem> Requests
        {
            get => requests;

            set
            {
                if (value != requests)
                {
                    requests = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public ObservableCollection<RequestItem> Sockets
        {
            get => sockets;

            set
            {
                if (value != sockets)
                {
                    sockets = value;
                    NotifyPropertyChanged();
                }
            }
        }
        
        public ObservableCollection<RequestItem> Frames
        {
            get => frames;

            set
            {
                if (value != frames)
                {
                    frames = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public bool HideLocalRequests
        {
            get => hideLocalRequests;

            set
            {
                if (value != hideLocalRequests)
                {
                    hideLocalRequests = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public bool ShowOnlyOrnaRequests
        {
            get => showOnlyOrnaRequests;

            set
            {
                if (value != showOnlyOrnaRequests)
                {
                    showOnlyOrnaRequests = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public bool HideResourceRequests
        {
            get => hideResourceRequests;

            set
            {
                if (value != hideResourceRequests)
                {
                    hideResourceRequests = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public bool HideOptionRequests
        {
            get => hideOptionsRequests;

            set
            {
                if (value != hideOptionsRequests)
                {
                    hideOptionsRequests = value;
                    NotifyPropertyChanged();
                }
            }
        }
        #endregion

        public SnifferWindow()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            DataContext = this;

            if (Design.IsDesignMode)
                return;

#if DEBUG
            this.AttachDevTools();
#endif

            RequestText = this.FindControl<TextBlock>("RequestText");
            ResponseText = this.FindControl<TextBlock>("ResponseText");
            ParsedRequestText = this.FindControl<TextBlock>("ParsedRequestText");
            ParsedResponseText = this.FindControl<TextBlock>("ParsedResponseText");

            ConnectionText = this.FindControl<TextBlock>("ConnectionText");
            FrameText = this.FindControl<TextBlock>("FrameText");
            FrameParsedText = this.FindControl<TextBlock>("FrameParsedText");

            var lifetime = (IClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime;
            var mainWindow = lifetime.MainWindow;
            if (mainWindow == null)
                return;
            BrowserView = mainWindow.FindControl<BrowserView>("browser");
            Browser = BrowserView.browser;
            Sniffer = BrowserView.sniffer;

            // Fill message list with messages //TODO: autorefresh or refresh button 
            GenerateRequestList();
        }

        public void GenerateRequestList()
        {
            // clear list
            Requests.Clear();
            var messages = Sniffer.Messages;
            // then feed our items into it
            foreach (var message in messages)
            {
                var text = message.Key.ToString();
                var req = message.Value.Request;
                if (req != null)
                {
                    var uri = new Uri(req.Uri);
                    // local file check
                    if (hideLocalRequests)
                    {
                        if (uri.IsFile)
                            continue;

                        if (uri.Scheme.Equals("blob", StringComparison.InvariantCultureIgnoreCase)
                            || uri.Scheme.Equals("data", StringComparison.InvariantCultureIgnoreCase))
                            continue;
                    }

                    // host check
                    if (showOnlyOrnaRequests && 
                        (uri.Scheme.Equals(Uri.UriSchemeHttp, StringComparison.InvariantCultureIgnoreCase) 
                            || uri.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.InvariantCultureIgnoreCase))
                        && !uri.Host.Equals("playorna.com"))
                        continue;

                    // option method check
                    if (hideOptionsRequests && req.Method.Equals("OPTIONS", StringComparison.InvariantCultureIgnoreCase))
                        continue;

                    //TODO: resource check //if (hideResourceRequests &&

                    text = $"{req.Method} {req.Uri}";
                }

                Requests.Add(new() { Text = text, Key = message.Key });
            }
        }

        public void OnFilterCheckboxClick(object sender, RoutedEventArgs e)
        {
            if (Design.IsDesignMode)
                return;

            // Regenerate the request list
            GenerateRequestList(); //TODO: fix scrollviewer overlapping into the Request/Response label
        }

        public async void OnRequestListSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Design.IsDesignMode)
                return;

            var selected = e.AddedItems.Count > 0 ? e.AddedItems[0] : null;
            if (selected == null)
                return;
            var text = ((RequestItem)selected).Key;
            if (!Sniffer.Messages.TryGetValue(text, out var message))
                return;

            var req = message.Request;
            var resp = message.Response;

            // Get and set request body
            var reqText = $"{req.Method} {req.Uri}\n";
            foreach (var header in resp.Headers)
                reqText += $"{header.Key}: {header.Value}\n";

            // post data, if it exists
            if (req.Content != null)
            {
                reqText += "\n\n";
                reqText += message.PostData;
            }
            RequestText.Text = reqText;

            //TODO: parse request

            // Get and set response body
            var respText = "";
            if (resp == null)
            {
                respText = "No Response received";
            }
            else
            {
                var statusName = Enum.GetName((HttpStatusCode)resp.StatusCode);
                respText += $"{resp.StatusCode} {statusName}\n";
                foreach (var header in resp.Headers)
                    respText += $"{header.Key}: {header.Value}\n";
                respText += "\n\n";
                // Get the response body
                respText += message.ResponseData;
            }
            ResponseText.Text = respText;

            //TODO: parse response
        }

        public async void OnSocketListSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Design.IsDesignMode)
                return;

            //TODO: socketlist
        }
        
        public async void OnFramesListSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Design.IsDesignMode)
                return;

            //TODO: frameslist
        }
    }
}

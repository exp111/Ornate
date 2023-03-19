using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Ornate.Lite.Messages;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
        public TextBlock SocketRequestText;
        public TextBlock SocketResponseText;
        public TextBlock FrameText;
        public TextBlock FrameParsedText;
        public ListBox SocketList;

        private bool hideLocalRequests = true;
        private bool showOnlyOrnaRequests = true;
        private bool hideResourceRequests = false;
        private bool hideOptionsRequests = true;

        public class RequestItem
        {
            public string Text;
            public object Key; //TODO: make this either int or string :weary:

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

        private void InitializeComponent() //TODO: clear button
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

            SocketRequestText = this.FindControl<TextBlock>("SocketRequestText");
            SocketResponseText = this.FindControl<TextBlock>("SocketResponseText");
            FrameText = this.FindControl<TextBlock>("FrameText");
            FrameParsedText = this.FindControl<TextBlock>("FrameParsedText");
            SocketList = this.FindControl<ListBox>("SocketList");

            var lifetime = (IClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime;
            var mainWindow = lifetime.MainWindow;
            if (mainWindow == null)
                return;
            BrowserView = mainWindow.FindControl<BrowserView>("browser");
            Browser = BrowserView.browser;
            Sniffer = BrowserView.sniffer;

            // Fill message list with messages //TODO: autorefresh or refresh button 
            GenerateRequestList();
            GenerateSocketList();
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

        public void GenerateSocketList() //TODO: refresh button
        {
            // clear list
            Sockets.Clear();
            var sockets = Sniffer.WebSockets;
            // then feed our items into it
            foreach (var socket in sockets)
            {
                var text = socket.Key.ToString();
                var con = socket.Value;
                if (con != null)
                {
                    text = $"GET {con.URL}";
                }

                Sockets.Add(new() { Text = text, Key = socket.Key });
            }
        }

        public void GenerateFramesList(string socketID) //TODO: refresh button
        {
            // clear list
            Frames.Clear();
            if (!Sniffer.WebSockets.TryGetValue(socketID, out var socket))
                return;
            var frames = socket.Messages;
            // then feed our items into it
            for (var i = 0; i < frames.Count; i++)
            {
                var frame = frames[i];
                var dirIndicator = frame.Direction == Direction.Sent ? "->" : "<-";
                var text = $"{dirIndicator} {frame.Frame.Opcode} {frame.Frame.PayloadData}";

                Frames.Add(new() { Text = text, Key = i });
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
            var requestHash = (int)((RequestItem)selected).Key;
            if (!Sniffer.Messages.TryGetValue(requestHash, out var message))
                return;

            var req = message.Request;
            var resp = message.Response;

            var uri = new Uri(message.Request.Uri);
            var localPath = uri.LocalPath;

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

            // parse request
            var parsedReqText = "Not Implemented";
            if (MessageHelper.TryGetRequest(uri, message.PostData, out var parsed))
                parsedReqText = parsed.ToString();
            //TODO: else make it generic
            //TODO: make this dynamic treeview
            ParsedRequestText.Text = parsedReqText;

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

            // parse response
            var parsedRespText = "Not Implemented";
            if (MessageHelper.TryGetResponse(uri, message.ResponseData, out var parsedResponse))
                parsedRespText = parsedResponse.ToString();
            //TODO: else make it generic
            //TODO: make this dynamic treeview
            ParsedResponseText.Text = parsedRespText;
        }

        public async void OnSocketListSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Design.IsDesignMode)
                return;

            var selected = e.AddedItems.Count > 0 ? e.AddedItems[0] : null;
            if (selected == null)
                return;
            var socketID = (string)((RequestItem)selected).Key;
            if (!Sniffer.WebSockets.TryGetValue(socketID, out var socket))
                return;

            var reqText = $"GET {socket.URL}";
            if (socket.Response != null)
                reqText = $"{socket.Response.RequestHeadersText}"; // headers are saved in response for some reason
            SocketRequestText.Text = reqText;

            var respText = "";
            if (socket.Response != null)
            {
                respText += socket.Response.HeadersText;
            }
            SocketResponseText.Text = respText;
            GenerateFramesList(socketID);
        }

        public async void OnFramesListSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Design.IsDesignMode)
                return;

            var selected = e.AddedItems.Count > 0 ? e.AddedItems[0] : null;
            if (selected == null)
                return;

            var selectedSocket = (RequestItem)SocketList.SelectedItem;
            var socketID = (string)selectedSocket.Key;
            if (!Sniffer.WebSockets.TryGetValue(socketID, out var socket))
                return;

            var index = (int)((RequestItem)selected).Key;
            if (index > socket.Messages.Count)
            {
                FrameText.Text = "Frame index out of range";
                FrameParsedText.Text = "";
            }

            var msg = socket.Messages[index];
            var reqText = $"Opcode: {msg.Frame.Opcode}\nPayloadData:\n{msg.Frame.PayloadData}"; //TODO: do we need to care about masking?
            FrameText.Text = reqText;

            //parse msg
            var parsedText = "Not Implemented";
            if (MessageHelper.TryGetMessage(msg.Direction, msg.Frame.PayloadData, out var parsed))
                parsedText = parsed.ToString();
            //TODO: else make it generic
            //TODO: make this dynamic treeview
            FrameParsedText.Text = parsedText;
        }
    }
}

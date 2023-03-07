using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Ornate.Lite
{
    public class SnifferWindow : Window, INotifyPropertyChanged
    {
        public BrowserView BrowserView;
        public WebView2 Browser;
        public Sniffer Sniffer;

        public ListBox RequestList;
        public TextBlock RequestText;
        public TextBlock ResponseText;

        private bool hideLocalRequests = true;
        private bool showOnlyOrnaRequests = true;
        private bool hideResourceRequests = false;
        private bool hideOptionsRequests = true;

        public class RequestItem
        {
            public string Text;
            public string Key;

            public override string ToString()
            {
                return Text;
            }
        }
        public ObservableCollection<RequestItem> requests = new();

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

            RequestList = this.FindControl<ListBox>("RequestList");
            RequestText = this.FindControl<TextBlock>("RequestText");
            ResponseText = this.FindControl<TextBlock>("ResponseText");

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
                var text = message.Key;
                var req = message.Value.Request;
                if (req != null)
                {
                    var uri = new Uri(req.Url);
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

                    text = $"{req.Method} {req.Url}";
                }

                Requests.Add(new() { Text = text, Key = message.Key });
            }
        }

        public void OnFilterCheckboxClick(object sender, RoutedEventArgs e)
        {
            // Regenerate the request list
            GenerateRequestList();
        }

        public async void OnRequestListSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = e.AddedItems.Count > 0 ? e.AddedItems[0] : null;
            if (selected == null)
                return;
            var text = ((RequestItem)selected).Key;
            if (!Sniffer.Messages.TryGetValue(text, out var message))
                return;

            var req = message.Request;
            var resp = message.Response;

            // Get and set request body
            var reqText = $"{req.Method} {req.Url}";
            reqText += "\n";
            reqText += req.Headers.ToString(); //TODO: get headers

            // post data, if it exists
            if (req.HasPostData != null)
            {
                reqText += "\n\n";
                try
                {
                    var post = await BrowserView.DevTools.Network.GetRequestPostDataAsync(text);
                    reqText += post;
                } 
                catch (Exception ex) 
                {
                    reqText += $"Exception: {ex.Message}";
                }
            }
            RequestText.Text = reqText;

            // Get and set response body
            var body = "";
            if (resp == null)
            {
                body = "No Response received";
            }
            else
            {
                try
                {
                    var result = await BrowserView.DevTools.Network.GetResponseBodyAsync(text);
                    body = result.Body;
                }
                catch (Exception ex)
                {
                    body += $"Exception: {ex.Message}";
                }
            }
            ResponseText.Text = body;
        }
    }
}
